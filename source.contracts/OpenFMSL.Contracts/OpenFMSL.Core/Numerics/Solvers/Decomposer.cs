using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics.Solvers
{
    public class Decomposer
    {
        EquationSystem problemData;
        Action<string> onLog;
        Action<string> onLogDebug;
        Action<string> onLogError;
        Action<string> onLogWarning;
        Action<string> onLogSuccess;
        Action<string> onLogInfo;

        bool _supressLogging = false;
        bool _doLinesearch = true;
        double _newtonTolerance = 1e-6;
        int _newtonMaxIter = 50;
        double _minNewtonLambda = 0.2;

        public Action<string> OnLog
        {
            get { return onLog; }
            set { onLog = value; }
        }

        public EquationSystem ProblemData
        {
            get { return problemData; }
            set { problemData = value; }
        }

        public IList<EquationSystem> Subproblems
        {
            get { return _decomposedNlps; }
            set { _decomposedNlps = value; }
        }

        private bool _activateInit = false;
        private bool _isInsufficientRank = false;
        private bool _isOverconstrained = false;

        int _minimumSubsystemSize = 1;


        public bool IsInsufficientRank
        {
            get { return _isInsufficientRank; }
            set { _isInsufficientRank = value; }
        }

        public bool IsOverconstrained
        {
            get { return _isOverconstrained; }
            set { _isOverconstrained = value; }
        }

        public bool ActivateInit
        {
            get { return _activateInit; }
            set { _activateInit = value; }
        }

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



        public int MinimumSubsystemSize
        {
            get
            {
                return _minimumSubsystemSize;
            }

            set
            {
                _minimumSubsystemSize = value;
            }
        }

        public bool SuppressLogging
        {
            get
            {
                return _supressLogging;
            }

            set
            {
                _supressLogging = value;
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

        public double NewtonTolerance
        {
            get
            {
                return _newtonTolerance;
            }

            set
            {
                _newtonTolerance = value;
            }
        }

        public int NewtonMaxIter
        {
            get
            {
                return _newtonMaxIter;
            }

            set
            {
                _newtonMaxIter = value;
            }
        }

        public double MinNewtonLambda
        {
            get
            {
                return _minNewtonLambda;
            }

            set
            {
                _minNewtonLambda = value;
            }
        }

        void Log(string message)
        {
            if (SuppressLogging)
                return;

            if (OnLog != null)
                OnLog(message);
        }


        private void LogDebug(string message)
        {
            Log(message);

        }
        private void LogSuccess(string message)
        {
            if (SuppressLogging)
                return;

            if (OnLogSuccess != null)
                OnLogSuccess(String.Format("{0}", message));
            else
            {
                Log(message);
            }
        }
        private void LogInfo(string message)
        {
            if (SuppressLogging)
                return;


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



        private IList<EquationSystem> _decomposedNlps;

        public string GetDebugInfo()
        {
            StringBuilder sb = new StringBuilder();

            if (IsOverconstrained)
            {
                sb.AppendLine("Error: System has an overconstrained part. Consider removing one of the following constraints:");

                foreach (var eq in Subproblems.First().Equations)
                {
                    sb.AppendLine(String.Format("{1,-20} {2,-10} {3,-15} ( {0} )", eq, eq.ModelClass, eq.ModelName, eq.Group));
                }
            }

            if (IsInsufficientRank)
            {
                sb.AppendLine("Warning: System has an underspecified part. Consider fixing one of the following variables.");
                foreach (var variable in Subproblems.Last().Variables)
                    sb.AppendLine(String.Format("{0,-20}", variable.WriteReport()));
            }

            return sb.ToString();
        }

        public void Decompose(EquationSystem problem)
        {
            this.ProblemData = problem;

            //LogInfo("Setting up problem...");
            _isInsufficientRank = false;
            _isOverconstrained = false;
            // LogInfo("Starting decomposition");
            // Log(String.Format("Original Problem : {0} Variables and {1} Equations", problem.NumberOfVariables, problem.NumberOfEquations));
            //LogInfo("Performing Dulmage-Mendelsohn decomposition...");
            var dmd = new DulmageMendelsohnDecomposition();
            var dm = dmd.Generate(ProblemData);
            LogInfo(String.Format("Decomposition Result: V={0}, E={1}, Blocks={2}, Singletons={3}", problem.NumberOfVariables, problem.NumberOfEquations, dm.Blocks, dm.Singletons));
            if (dm.StructuralRank != problem.NumberOfEquations)
                LogInfo("Structural Rank : " + dm.StructuralRank);
            if (dm.StructuralRank < problem.NumberOfEquations)
                IsInsufficientRank = true;
            // LogInfo("DMD created " + dm.Blocks + " blocks...");
            _decomposedNlps = new List<EquationSystem>();
            var currentSystem = new EquationSystem() { Name = "Subproblem 1" };
            var lastVarCount = 0;
            for (int i = dm.Blocks - 1; i >= 0; i--)
            {
                var varcount = dm.s[i + 1] - dm.s[i];

                if (varcount > 1 || (varcount == 1 && lastVarCount > 1) || currentSystem.Variables.Count >= MinimumSubsystemSize)
                {
                    if (currentSystem.Variables.Count > 0 || currentSystem.Equations.Count > 0)
                        _decomposedNlps.Add(currentSystem);

                    currentSystem = new EquationSystem()
                    {
                        Name = "Subproblem " + (_decomposedNlps.Count + 1),
                    };
                }

                for (int j = 0; j < varcount; j++)
                {
                    var vari = dm.q[dm.s[i] + j];
                    currentSystem.AddVariables(ProblemData.Variables[vari]);
                }

                var eqcount = dm.r[i + 1] - dm.r[i];
                for (int j = 0; j < eqcount; j++)
                {
                    var index = dm.p[dm.r[i] + j];
                    currentSystem.AddConstraints(ProblemData.Equations[index]);
                }
                lastVarCount = varcount;

            }

            _decomposedNlps.Add(currentSystem);

            if (_decomposedNlps.First().NumberOfEquations != _decomposedNlps.First().Variables.Count)
                IsOverconstrained = true;

            if (_decomposedNlps.Last().NumberOfEquations != _decomposedNlps.Last().Variables.Count)
                IsInsufficientRank = true;

            if (IsInsufficientRank || IsOverconstrained)
                LogError(GetDebugInfo());

            foreach (var subproblem in _decomposedNlps.ToArray())
            {
                if (subproblem.Variables.Count == 0)
                    _decomposedNlps.Remove(subproblem);
            }

        }



        public bool Solve(EquationSystem problem)
        {
            if (Subproblems != null)
                Subproblems.Clear();

            this.ProblemData = problem;

            ProblemData.CreateIndex();
            ProblemData.GenerateJacobian();

            Decompose(problem);

            if (IsOverconstrained && IsInsufficientRank)
                return false;

            Newton newtonSubsolver = new Newton();
            newtonSubsolver.MaximumIterations = NewtonMaxIter;
            newtonSubsolver.Tolerance = NewtonTolerance;
            newtonSubsolver.DoLinesearch = DoLinesearch;
            newtonSubsolver.LambdaMin = MinNewtonLambda;
            //newtonSubsolver.BrakeFactor = 0.99;

            //newtonSubsolver.OnLog += OnLog;
            // newtonSubsolver.OnLogSuccess += OnLogSuccess;
            newtonSubsolver.OnLogError += OnLogError;
            newtonSubsolver.OnLogWarning += OnLogWarning;

            int i = 1;
            // LogInfo("Solving decomposed sub-problems...");      
            bool hasError = false;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var decomposedNlp in _decomposedNlps)
            {
                if (decomposedNlp.Variables.Count == 0)
                    continue;

                var statusmessage = "Solving problem " + i + " of " + _decomposedNlps.Count + " (Size: " + decomposedNlp.Variables.Count + ")";
                //   PublishProgress(statusmessage, (int)((i / (double)_decomposedNlps.Count) * 100));
                //  PublishStatus(statusmessage);
                newtonSubsolver.Solve(decomposedNlp);

                if (!newtonSubsolver.IsConverged)
                {
                    var eval = new Evaluator();
                    // PublishStatus("Solving problem " + decomposedNlp.Name + " failed!");
                    hasError = true;
                    LogError("Solving problem " + decomposedNlp.Name + " (Size: " + decomposedNlp.Variables.Count + ") failed!");
                    LogError("The 20 most problematic constraints are:");
                    foreach (var eq in decomposedNlp.Equations.OrderByDescending(c => Math.Abs(c.Residual(eval))).Take(Math.Min(20, decomposedNlp.Equations.Count)))
                    {
                        LogError(String.Format("{2,-20} {3,-10} {4,-15} {0,20} ( {1} )", eq.Residual(eval).ToString("G8"), eq, eq.ModelClass, eq.ModelName, eq.Group));
                    }
                    LogError("");

                    break;
                }
                else
                {
                    //LogSuccess("Problem " + decomposedNlp.Name + " solved in " + subsolver.Iterations + " iterations.");                    
                }

                i++;
            }
            watch.Stop();

            if (!hasError)
            {
                LogSuccess("Problem " + problem.Name + " was successfully solved (" + watch.Elapsed.TotalSeconds.ToString("0.00") + " seconds)");
                return true;
            }
            else
            {
                LogError("Problem " + problem.Name + " was not successfully solved (Result = " + ")");
                return false;
            }
            //PublishStatus("Solution finished...");
            //  PublishProgress("No background work", 0);




        }
    }

}
