using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics.Solvers
{
    /// <summary>
    /// Newton-Raphson type solver adapted from the WIKIPEDIA article.
    /// This solver uses sparse symbolic Jacobian information.
    /// Convergence is rather limited if starting values are not very good.
    /// Uses the CSparse.NET LU linear solver by default.
    /// </summary>
    public class Newton
    {
        #region Fields
        EquationSystem _problemData;

        double _tolerance = 1e-6;
        double _maximumNewtonStep = 1e12;
        int _maximumIterations = 30;
        double _brakeFactor = 1.0;
        double _lambdaMin = 0.2;
        bool _doScaling = true;
        bool _doLinesearch = true;

        MatrixScalingFrequency _scalingFrequency = MatrixScalingFrequency.Always;
        int _iterations;
        string _status;
        private bool _isConverged = false;
        private bool _isAborted = false;
        bool _debugMode = true;

        System.Diagnostics.Stopwatch _watch;

        Action<string> onLog;
        Action<string> onLogDebug;
        Action<string> onLogError;
        Action<string> onLogWarning;
        Action<string> onLogSuccess;
        Action<string> onLogInfo;

        Action<EquationSystem> onIteration;

        #endregion

        #region Properties
        public Action<string> OnLogDebug
        {
            get { return onLogDebug; }
            set { onLogDebug = value; }
        }

        public Action<string> OnLogError
        {
            get { return onLogError; }
            set { onLogError = value; }
        }

        public Action<string> OnLogWarning
        {
            get { return onLogWarning; }
            set { onLogWarning = value; }
        }

        public Action<string> OnLogSuccess
        {
            get { return onLogSuccess; }
            set { onLogSuccess = value; }
        }

        public Action<string> OnLogInfo
        {
            get { return onLogInfo; }
            set { onLogInfo = value; }
        }

        public EquationSystem ProblemData
        {
            get { return _problemData; }
            set { _problemData = value; }
        }

        public Action<string> OnLog
        {
            get { return onLog; }

            set { onLog = value; }
        }

        public Action<EquationSystem> OnIteration
        {
            get
            {
                return onIteration;
            }

            set
            {
                onIteration = value;
            }
        }

        public int Iterations
        {
            get
            {
                return _iterations;
            }

            set
            {
                _iterations = value;
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public bool DoScaling
        {
            get
            {
                return _doScaling;
            }

            set
            {
                _doScaling = value;
            }
        }

        public bool DoLinesearch
        {
            get
            {
                return _doLinesearch;
            }

            set
            {
                _doLinesearch = value;
            }
        }

        public double Tolerance
        {
            get
            {
                return _tolerance;
            }

            set
            {
                _tolerance = value;
            }
        }

        public int MaximumIterations
        {
            get
            {
                return _maximumIterations;
            }

            set
            {
                _maximumIterations = value;
            }
        }

        public double BrakeFactor
        {
            get
            {
                return _brakeFactor;
            }

            set
            {
                _brakeFactor = value;
            }
        }

        public bool IsConverged
        {
            get
            {
                return _isConverged;
            }

            set
            {
                _isConverged = value;
            }
        }

        public bool IsAborted
        {
            get
            {
                return _isAborted;
            }

            set
            {
                _isAborted = value;
            }
        }

        public double MaximumNewtonStep
        {
            get
            {
                return _maximumNewtonStep;
            }

            set
            {
                _maximumNewtonStep = value;
            }
        }

        public MatrixScalingFrequency ScalingFrequency
        {
            get
            {
                return _scalingFrequency;
            }

            set
            {
                _scalingFrequency = value;
            }
        }

        public bool DebugMode
        {
            get
            {
                return _debugMode;
            }

            set
            {
                _debugMode = value;
            }
        }

        public double LambdaMin
        {
            get
            {
                return _lambdaMin;
            }

            set
            {
                _lambdaMin = value;
            }
        }
        #endregion

        #region Logging Callbacks
        private void LogDebug(string message)
        {
            Log(message);
        }

        private void LogSuccess(string message)
        {
            if (OnLogSuccess != null)
                OnLogSuccess(String.Format("{0}", message));
            else
            {
                Log(message);
            }
        }
        private void LogInfo(string message)
        {
            if (OnLogInfo != null)
                OnLogInfo(String.Format("{0}", message));
            else
            {
                Log(message);
            }

        }
        private void LogWarning(string message)
        {
            if (OnLogWarning != null)
                OnLogWarning(String.Format("{0}", message));
            else
            {
                Log(message);
            }
        }
        private void LogError(string message)
        {
            if (OnLogError != null)
                OnLogError(String.Format("{0}", message));
            else
            {
                Log(message);
            }
        }

        public void Log(string msg)
        {
            OnLog?.Invoke(msg);
        }
        #endregion


        public void Solve(EquationSystem system)
        {
            Log(String.Format("{0} {1,15} {2,15} {3,7} {4}", "Iter", "Step Length", "Infeasibility", "Damping", "Algorithm"));

            if (!system.IsSquare())
            {
                Status = "Newton-Solver can only solve square problems.";
                IsAborted = true;
                IsConverged = false;
                return;
            }
            ProblemData = system;

            ProblemData.CreateIndex();
            ProblemData.GenerateJacobian();

            Evaluator eval = new Evaluator();
            _watch = System.Diagnostics.Stopwatch.StartNew();

            var algorithm = "Newton";
            var status = false;
            var scalingLogSum = new MatrixScalingLogSum(2);
            double[] U = null, V = null;
            Iterations = 0;
            IsConverged = false;
            IsAborted = false;

            double varNorm = 0;
            double eqNorm = 0;
            Vector delta = new Vector(system.NumberOfVariables);
            var lambda = BrakeFactor;
            for (int i = 0; i < system.NumberOfVariables; i++)
            {
                delta[i] = system.Variables[i].ValueInSI;
            }
            while (!IsConverged && !IsAborted)
            {
                string flags = "";

                #region Jacobian and Residuals
                eval.Reset();

                var b = CSparseWrapper.FillResiduals(system, eval);
                var A = CSparseWrapper.FillJacobian(system, eval);
                #endregion

                varNorm = delta.GetNorm();
                eqNorm = 0;//
                string debugString = "";

                for (int i = 0; i < b.Size; i++)
                {
                    var babs = Math.Abs(b[i]);
                    if (babs > eqNorm)
                    {
                        eqNorm = babs;
                        if (DebugMode)
                            debugString = ProblemData.Equations[i].ToString();
                    }
                }
                //b.ToDouble().Max((s) => Math.Abs(s));
                //if(DebugMode)
                //debugString= ProblemData.Equations.Max(eq=> Math.Abs(eq.Residual()))

                Log(String.Format(" {0:000} {1,15} {2,15} {3,7} {4,4} {5} {6} {7}", Iterations, varNorm.ToString("G2", CultureInfo.InvariantCulture), eqNorm.ToString("G8", CultureInfo.InvariantCulture), lambda, "NEWTON", algorithm, flags, debugString));


                OnIteration?.Invoke(system);
                CheckAbortionCriteria(Iterations, MaximumIterations, eqNorm, Tolerance);

                if (IsAborted || IsConverged)
                    break;

                #region Scaling    
                //TODO: Only rescale when necessary? Maybe decide based on condition number?
                //      Current version only scales once at the beginning.
                if (ScalingFrequency == MatrixScalingFrequency.Once && (U == null || V == null)
                    || ScalingFrequency == MatrixScalingFrequency.Always)
                    scalingLogSum.GetMatrixScalingFactors(A, out U, out V);

                if (DoScaling)
                {
                    for (int i = 0; i < system.NumberOfEquations; i++)
                    {
                        b[i] = b[i] * U[i];
                    }
                    var UM = CSparseWrapper.CreateDiagonal(system.NumberOfEquations, U);
                    var VM = CSparseWrapper.CreateDiagonal(system.NumberOfEquations, V);
                    A = UM.Multiply(A);
                    A = A.Multiply(VM);
                }
                #endregion


                delta = CSparseWrapper.SolveLinearSystem(A, delta, -b, out status, out algorithm);


                #region Scaling 2
                if (DoScaling)
                {
                    for (int i = 0; i < delta.Size; i++)
                    {
                        delta[i] = delta[i] * V[i];
                    }
                }
                #endregion
                lambda = BrakeFactor;

                if (delta.GetNorm() > _maximumNewtonStep)
                {
                    //LogWarning("Variable step above limit. Truncating Newton step");
                    flags += "T";
                    delta /= delta.GetNorm() / _maximumNewtonStep;
                }
                else
                    flags += "_";


                if (status == false)
                {
                    LogWarning("Error during factorization. Performing steepest descent step");
                    //delta = new Vector(system.NumberOfVariables, 1e-6);                    
                    //Force rescaling when now direction could be found
                    if (ScalingFrequency == MatrixScalingFrequency.OnDemand)
                    {
                        U = null;
                        V = null;
                    }
                    flags += "D";
                    lambda *= 0.1;
                    A.TransposeMultiply((-b).ToDouble(), delta.ToDouble());
                }
                else
                    flags += "_";



                Func<Vector, Double> FuncLineSearch = ((r) => r.ToDouble().Max(s => Math.Abs(s)));
                var F0 = FuncLineSearch(b);
                var x0 = system.Variables.Select(v => v.ValueInSI).ToArray();
                var lineSearchIter = 0;
                var currentStepLength = lambda;
                var lambdaMin = _lambdaMin;

                if (DoLinesearch)
                {
                    while (lineSearchIter < 10)
                    {
                        for (int i = 0; i < delta.Size; i++)
                        {
                            var vari = system.Variables[i];
                            vari.ValueInSI = x0[i];
                        }
                        ApplyNewtonStep(system, lambda, delta);

                        eval.Reset();
                        b = CSparseWrapper.FillResiduals(system, eval);

                        if (DoScaling)
                            for (int i = 0; i < system.NumberOfEquations; i++)
                            {
                                b[i] = b[i] * U[i];
                            }

                        var F1 = FuncLineSearch(b);


                        if (F1 >= F0)
                        {
                            lambda *= 0.5;

                            if (lambda < lambdaMin)
                                lambda = lambdaMin;
                            currentStepLength = lambda;

                            if (!flags.Contains("L"))
                                flags += "L";
                        }
                        else
                        {
                            if (!flags.Contains("L"))
                                flags += "_";
                            currentStepLength = lambda;
                            break;
                        }
                        lineSearchIter++;
                    }
                }
                else
                {
                    flags += "_";
                    ApplyNewtonStep(system, lambda, delta);

                }


                Iterations++;
            }
        }

        private void ApplyNewtonStep(EquationSystem system, double lambda, Vector delta)
        {

            for (int i = 0; i < delta.Size; i++)
            {
                if (Double.IsNaN(delta[i]))
                {
                    delta[i] = 0;
                    continue;
                }

                var vari = system.Variables[i];
                var step = lambda * delta[i];

                if (vari.Dimension == PhysicalDimension.Temperature)
                {
                    if (Math.Abs(step) > 25)
                        step = Math.Sign(step) * 25;

                    vari.ValueInSI += step;
                    BoundVariable(vari);
                }

                else if (vari.Dimension == PhysicalDimension.MolarFlow)
                {
                    var oldValue = vari.ValueInSI;

                    if (Math.Abs(step) > 200)
                        step = Math.Sign(step) * 200;

                    vari.ValueInSI += step;

                    if (vari.ValueInSI < vari.LowerBound)
                        vari.ValueInSI = oldValue * 0.01;

                    if (vari.LowerBound == 0.0 && vari.ValueInSI < 1e-10)
                        vari.ValueInSI = 0;

                    BoundVariable(vari);
                }
                else if (vari.Dimension == PhysicalDimension.MolarFraction)
                {
                    var oldValue = vari.ValueInSI;

                    if (Math.Abs(step) > 1)
                        step = Math.Sign(step) * 0.01;

                    vari.ValueInSI += step;

                    if (vari.ValueInSI < 0)
                        vari.ValueInSI = oldValue * 0.01;

                    if (vari.ValueInSI < 1e-12)
                        vari.ValueInSI = 0;

                    BoundVariable(vari);
                }

                else
                {
                    var oldValue = vari.ValueInSI;
                    vari.ValueInSI += step;
                    BoundVariable(vari);

                }
            }
        }

        private void CheckAbortionCriteria(int iter, int maxIter, double equationNorm, double tol)
        {
            // &&variableNorm <= tol  ||
            if (iter > 0 && equationNorm <= tol)
            {
                IsConverged = true;
                IsAborted = true;
                _watch.Stop();
                LogSuccess("Problem " + ProblemData.Name + " was successfully solved because constraint violation is below tolerance (" + iter + " iterations, " + _watch.Elapsed.TotalSeconds.ToString("0.00") + " seconds, problem size: " + ProblemData.Variables.Count + ")");
                return;
            }
            if (equationNorm > 1e16)
            {
                IsConverged = false;
                IsAborted = true;
                _watch.Stop();
                LogError("Problem diverged! (" + _watch.Elapsed.TotalSeconds.ToString("0.00") + " seconds, Size: " + ProblemData.Variables.Count + ")");
                return;
            }

            if (iter >= maxIter)
            {
                IsConverged = false;
                IsAborted = true;
                _watch.Stop();
                LogError("Maximum number of iterations exceeded (" + _watch.Elapsed.TotalSeconds.ToString("0.00") + " seconds)");
                return;
            }
        }

        private void BoundVariable(Variable vari)
        {
            if (vari.ValueInSI > vari.UpperBound)
            {
                vari.ValueInSI = vari.UpperBound;

            }
            if (vari.ValueInSI < vari.LowerBound)
            {
                vari.ValueInSI = vari.LowerBound;
            }
        }



    }
}
