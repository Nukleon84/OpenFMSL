using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public class ActivityCoefficientUNIQUAC : Expression
    {
        ThermodynamicSystem _system;
        int index = -1;
        int NC;
        Variable T;
        List<Variable> x;
        Expression[,] tau;
        Expression _gamma_exp;
        Expression _dgamma_dT;
        Expression[] _dgamma_dx;

        private List<Expression> _parameters = new List<Expression>();

        public List<Expression> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }

        public override Expression SymbolicDiff(Variable var)
        {
            if (T == var)
            {
                if (_dgamma_dT == null)
                    _dgamma_dT = _gamma_exp.SymbolicDiff(var);
                return _dgamma_dT;
            }

            for (int i = 0; i < NC; i++)
            {
                if (x[i] == var)
                {
                    if (_dgamma_dx[i] == null)
                        _dgamma_dx[i] = _gamma_exp.SymbolicDiff(var);
                    return _dgamma_dx[i];
                }
            }
            return new IntegerLiteral(0);
        }
        


        /// <summary>
        /// Creates a new instance of the liquid phase activity coefficient calculation object
        /// </summary>
        /// <param name="sys">Thermodynamic system to take parameters from</param>
        /// <param name="T">Temperature variable</param>
        /// <param name="x">Vector of composition variables (molar fractions)</param>
        /// <param name="idx">Index of the active variables to calculate for</param>
        public ActivityCoefficientUNIQUAC(ThermodynamicSystem sys, Variable T, List<Variable> x, int idx)
        {
            Symbol = "UNIQUAC_GAMMA";
            _system = sys;
            index = idx;
            //Bind variables to this instance
            this.T = T;
            this.x = x;
            Parameters.Add(T);
            foreach (var comp in x)
                Parameters.Add(comp);
            NC = _system.Components.Count;
            tau = new Expression[NC, NC];

            int i = index;

            var parameterSet = _system.BinaryParameters.FirstOrDefault(ps => ps.Name == "UNIQUAC");
            if (parameterSet == null)
                throw new ArgumentNullException("No UNIQUAC parameters defined");

            double[,] a = parameterSet.Matrices["A"];
            double[,] b = parameterSet.Matrices["B"];
            double[,] c = parameterSet.Matrices["C"];
            double[,] d = parameterSet.Matrices["D"];
            double[,] e = parameterSet.Matrices["E"];

            for (int ii = 0; ii < NC; ii++)
            {
                for (int j = 0; j < NC; j++)
                {
                    tau[ii, j] = Sym.Exp(a[ii, j] + b[ii, j] / T + c[ii, j] * Sym.Ln(T) + d[ii, j] * T + e[ii, j] / Sym.Pow(T, 2));
                }
            }
            Expression lnGamma = 0.0;
            Expression lnGammaComb = 0.0;
            Expression lnGammaRes = 0.0;
            Expression Vi = 0;
            Expression Fi = 0;
            Expression FiP = 0;
            Expression Sxl = 0;

            var ri = _system.Components[i].GetConstant(ConstantProperties.UniquacR);
            var qi = _system.Components[i].GetConstant(ConstantProperties.UniquacQ);
            var qpi = _system.Components[i].GetConstant(ConstantProperties.UniquacQP);

            Expression li = 5 * (ri - qi) - (ri - 1);

            Vi = ri * x[i] / Sym.Sum(0, NC, (j) => ri * x[j]);
            Fi = qi * x[i] / Sym.Sum(0, NC, (j) => qi * x[j]);
            FiP = qpi * x[i] / Sym.Sum(0, NC, (j) => qpi * x[j]);
            Sxl = Sym.Sum(0, NC, (j) => (5 * (ri - qi) - (ri - 1)) * x[j]);

            lnGammaComb = Sym.Ln(Vi / x[i]) + 5 * qi * Sym.Ln(Fi / Vi) + li - Vi / x[i] * Sxl;

            Expression[] FPj = new Expression[_system.Components.Count];
            for (int j = 0; j < NC; j++)
            {
                FPj[j] = qpi * x[j] / Sym.Sum(0, NC, (kj) => qpi * x[kj]);
            }

            var doubleSum = Sym.Sum(0, NC, (j) => FPj[j] * tau[i, j] / (Sym.Sum(0, NC, (k) => FPj[k] * tau[k, j])));
            var SFP = Sym.Sum(0, NC, (j) => FPj[j] * tau[j, i]);

            lnGammaRes = qpi * (1 - Sym.Ln(SFP) - doubleSum);
            lnGamma = lnGammaComb + lnGammaRes;
            _gamma_exp = Sym.Exp(lnGamma);

            _dgamma_dx = new Expression[NC];
            
            DiffFunctional = (cache, v) => _gamma_exp.Diff(cache, v);
            EvalFunctional = (cache) => _gamma_exp.Eval(cache);            
        }

        public override HashSet<Variable> Incidence()
        {
            var inc = new HashSet<Variable>();

            foreach (var parameter in Parameters)
            {
                inc.UnionWith(parameter.Incidence());
            }
            return inc;
        }

        public override string ToString()
        {
            return "NRTL_UNIQUAC(T,x)";
        }

    }
}
