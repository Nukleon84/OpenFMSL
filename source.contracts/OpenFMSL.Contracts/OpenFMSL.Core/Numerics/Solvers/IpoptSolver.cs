using System;
using System.Linq;
using System.Globalization;

using Cureos.Numerics;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.UnitsOfMeasure;

namespace OpenFMSL.Core.Numerics.Solvers
{
    public class IpoptSolver
    {
        #region Fields
        OptimizationProblem problemData;
        Action<string> onLog;
        double[] r;
        int iterations = 0;
        private int _logIter = 5;
        Evaluator evaluator;
        public int Iterations
        {
            get { return iterations; }
        }


        double[] cLB;
        double[] cUB;
        double[] x0;
        double[] xLB;
        double[] xUB;


        #endregion

        #region Properties

        public OptimizationProblem ProblemData
        {
            get { return problemData; }
            set { problemData = value; }
        }
        #endregion

        #region Public functions
        public void Log(string msg)
        {
            // MessageDispatcher.Raise<LogMessage>(new LogMessage { Channel = LogChannels.Information, MessageText = msg, Sender = this, TimeStamp = DateTime.Now });
            OnLog?.Invoke(msg);

        }
        public bool Solve(OptimizationProblem problem)
        {
            ProblemData = problem;
            return Solve();
        }

        bool Solve()
        {
            IpoptReturnCode status;

            cLB = new double[this.ProblemData.Equations.Count + this.ProblemData.Constraints.Count];
            cUB = new double[this.ProblemData.Equations.Count + this.ProblemData.Constraints.Count];
            x0 = ProblemData.Variables.Select(v => v.ValueInSI).ToArray();
            xLB = new double[ProblemData.Variables.Count];
            xUB = new double[ProblemData.Variables.Count];
            r = new double[ProblemData.Equations.Count + ProblemData.Constraints.Count];

            for (int i = 0; i < ProblemData.Variables.Count; i++)
            {
                xLB[i] = ProblemData.Variables[i].LowerBound;
                xUB[i] = ProblemData.Variables[i].UpperBound;
            }

            for (int i = 0; i < ProblemData.Equations.Count; i++)
            {
                cLB[i] = 0.0;
                cUB[i] = 0.0;
            }
            for (int i = 0; i < ProblemData.Constraints.Count; i++)
            {
                cLB[i + ProblemData.Equations.Count] = ProblemData.Constraints[i].LowerBound;
                cUB[i + ProblemData.Equations.Count] = ProblemData.Constraints[i].UpperBound;
            }
            ProblemData.CreateIndex();
            ProblemData.GenerateJacobian();
            var m = ProblemData.Equations.Count + ProblemData.Constraints.Count;
            using (var problem = new IpoptProblem(ProblemData.NumberOfVariables,
                xLB,
                xUB,
                m,
                cLB,
                cUB,
                ProblemData.Jacobian.Count,
                0,
                eval_f, eval_g, eval_grad_f, eval_jac_g, eval_h))
            {

                evaluator = new Evaluator();
                problem.AddOption("hessian_approximation", "limited-memory");
                problem.AddOption("tol", 1e-5);
                problem.AddOption("max_iter", 250);
                problem.AddOption("max_cpu_time", 30);
                problem.AddOption("mu_strategy", "adaptive");
              //  problem.AddOption("derivative_test", "first-order");
                problem.AddOption("output_file", "ipopt.out");
                problem.AddOption("linear_solver", "ma27");
                problem.AddOption("print_user_options", "yes");
                // problem.AddOption("bound_relax_factor", 1e-8);
                  problem.AddOption("bound_frac" , 1e-8);
                   problem.AddOption("bound_push" ,1e-8);
                  problem.AddOption("constr_viol_tol", 1e-5);


                problem.SetIntermediateCallback(intermediate);
                double obj;
                Log(String.Format("{0} {4,15} {1,15} {3,15} {2}", "ITER", "D_NORM", "ALG", "INF_PR", "OBJ"));
                status = problem.SolveProblem(x0, out obj, r);
                //ProblemData.Update(ProblemData.VariableValues);
                Log(String.Format("{0:0000} {4,15} {1,15} {3,15} {2}", iterations, "", status, "", obj.ToString("G4", CultureInfo.InvariantCulture)));
            }

            // var status == IpoptReturnCode.Solve_Succeeded;
            bool result = false;
            switch (status)
            {
                case IpoptReturnCode.Solve_Succeeded:
                case IpoptReturnCode.Solved_To_Acceptable_Level:
                    result = true;
                    break;
                case IpoptReturnCode.Maximum_CpuTime_Exceeded:
                    result = false;
                    break;
                case IpoptReturnCode.Maximum_Iterations_Exceeded:
                    result = false;
                    break;
                case IpoptReturnCode.Infeasible_Problem_Detected:
                case IpoptReturnCode.Restoration_Failed:
                    result = false;
                    break;

            }
            return result;
        }
        #endregion

        #region Private Callbacks for CSIPOPT

        public bool intermediate(IpoptAlgorithmMode alg_mod, int iter_count, double obj_value, double inf_pr, double inf_du,
           double mu, double d_norm, double regularization_size, double alpha_du, double alpha_pr, int ls_trials)
        {

            iterations = iter_count;
            if (iter_count % LogIter == 0)
            {
                Log(String.Format("{0:0000} {4,15} {1,15} {3,15} {2}", iter_count, d_norm.ToString("G4", CultureInfo.InvariantCulture), alg_mod, inf_pr.ToString("G4", CultureInfo.InvariantCulture), obj_value.ToString("G4", CultureInfo.InvariantCulture)));

                // foreach (var vari in ProblemData.Variables)
                //     Log(vari.WriteReport());
                //foreach (var eq in ProblemData.Constraints.Where(c => Math.Abs(c.ResidualProperty) > 1e-4).OrderByDescending(c => Math.Abs(c.ResidualProperty)).Take(Math.Min(3, ProblemData.Constraints.Count)))
                //    Log(String.Format("{2,-20} {3,-10} {4,-15} {0,12} ( {1} )", eq.ResidualProperty.ToString("0.0000"), eq, eq.MetaData.ClassName, eq.MetaData.InstanceName, eq.MetaData.Type));



            }


            return true;
        }
        void UpdateProblem(double[] x)
        {
            for (int i = 0; i < ProblemData.Variables.Count; i++)
            {
                ProblemData.Variables[i].ValueInSI = x[i];
            }
        }

        bool eval_f(int n, double[] x, bool new_x, out double obj_value)
        {
            if (new_x)
            {
                UpdateProblem(x);
                evaluator.Reset();
            }
            obj_value = ProblemData.ObjectiveFunction.Eval(evaluator);

            return true;
        }
        bool eval_grad_f(int n, double[] x, bool new_x, double[] grad_f)
        {
            if (new_x)
            {
                UpdateProblem(x);
                evaluator.Reset();
            }
            for (int i = 0; i < ProblemData.Variables.Count; i++)
            {
                if (ProblemData.ObjectiveFunction.Incidence().Contains(ProblemData.Variables[i]))
                    grad_f[i] = ProblemData.ObjectiveFunction.Diff(evaluator, ProblemData.Variables[i]);
                else
                    grad_f[i] = 0;
            }
            return true;
        }
        bool eval_g(int n, double[] x, bool new_x, int m, double[] g)
        {
            if (new_x)
            {
                UpdateProblem(x);
                evaluator.Reset();
            }
            for (int i = 0; i < ProblemData.Equations.Count; i++)
            {
                g[i] = ProblemData.Equations[i].Residual(evaluator);
            }

            for (int i = 0; i < ProblemData.Constraints.Count; i++)
            {
                g[ProblemData.Equations.Count + i] = ProblemData.Constraints[i].Residual(evaluator);
            }
            return true;
        }
        bool eval_jac_g(int n, double[] x, bool new_x, int m, int nele_jac, int[] iRow, int[] jCol, double[] values)
        {
            if (values == null)
            {
                for (int i = 0; i < ProblemData.Jacobian.Count; i++)
                {
                    iRow[i] = ProblemData.Jacobian[i].EquationIndex;
                    jCol[i] = ProblemData.Jacobian[i].VariableIndex;
                }
            }
            else
            {

                if (new_x)
                {
                    UpdateProblem(x);
                    evaluator.Reset();
                }
                for (int i = 0; i < ProblemData.Jacobian.Count; i++)
                {
                    var entry = ProblemData.Jacobian[i];
                    var value = 0.0;
                    if (entry.EquationIndex < ProblemData.Equations.Count)
                        value = ProblemData.Equations[entry.EquationIndex].Diff(evaluator, ProblemData.Variables[entry.VariableIndex]);
                    else
                        value = ProblemData.Constraints[entry.EquationIndex - ProblemData.Equations.Count].Diff(evaluator, ProblemData.Variables[entry.VariableIndex]);

                    values[i] = value;
                }

            }

            return true;
        }
        bool eval_h(int n, double[] x, bool new_x, double obj_factor,
                    int m, double[] lambda, bool new_lambda,
                    int nele_hess, int[] iRow, int[] jCol,
                    double[] values)
        {
            return false;
        }
        #endregion


        public double[] ConstraintResiduals
        {
            get { return r; }
        }

        public Action<string> OnLog
        {
            get { return onLog; }
            set { onLog = value; }
        }

        public int LogIter
        {
            get { return _logIter; }
            set { _logIter = value; }
        }
    }
}
