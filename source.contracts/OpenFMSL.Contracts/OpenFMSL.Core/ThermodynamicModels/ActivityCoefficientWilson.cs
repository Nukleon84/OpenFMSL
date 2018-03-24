using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public class ActivityCoefficientWilson: Expression
    {
        ThermodynamicSystem _system;
        int index = -1;
        int NC;

        Variable T;
        List<Variable> x;
       
        Expression _gammaExp;
        Expression _dgammaDt;
        Expression[] _dgammaDx;

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

        public ActivityCoefficientWilson(ThermodynamicSystem system, Variable T, List<Variable> x, int idx)
        {
            Symbol = "WILSON_GAMMA";
            _system = system;
            index = idx;

            Parameters.Add(T);
            foreach (var comp in x)
                Parameters.Add(comp);

            NC = system.Components.Count;
            this.T = T;
            this.x = x;

            var parameterSet = _system.BinaryParameters.FirstOrDefault(ps => ps.Name == "WILSON");
            if (parameterSet == null)
                throw new ArgumentNullException("No WILSON parameters defined");

            double[,] a = parameterSet.Matrices["A"];
            double[,] b = parameterSet.Matrices["B"];
            double[,] c = parameterSet.Matrices["C"];
            double[,] d = parameterSet.Matrices["D"];

            var lambda = new Expression[NC, NC];

            for (int i = 0; i < NC; i++)
            {
                for (int j = 0; j < NC; j++)
                {
                    lambda[i, j] = Sym.Exp(a[i, j] + b[i, j] / T + c[i,j]*Sym.Ln(T) + d[i,j]*T);
                }
            }
            Expression H1 = Sym.Sum(0, NC, (k) => x[k] * lambda[index, k]);
            Expression H2 = Sym.Sum(0, NC, (k) => x[k] * lambda[k, index] / Sym.Sum(0, NC, (j) => x[j] * lambda[k, j]));
            _gammaExp = Sym.Exp(1 - Sym.Ln(H1) - H2);            
            _dgammaDx = new Expression[NC];

            DiffFunctional = (cache, v) => _gammaExp.Diff(cache, v);
            EvalFunctional = (cache) => _gammaExp.Eval(cache); 
            
        }


        public override Expression SymbolicDiff(Variable var)
        {
            if (T == var)
            {
                if (_dgammaDt == null)
                    _dgammaDt = _gammaExp.SymbolicDiff(var);
                return _dgammaDt;
            }

            for (int i = 0; i < NC; i++)
            {
                if (x[i] == var)
                {
                    if (_dgammaDx[i] == null)
                        _dgammaDx[i] = _gammaExp.SymbolicDiff(var);
                    return _dgammaDx[i];
                }
            }
            return new IntegerLiteral(0);
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
            return "WILSON_GAMMA(T,x)";
        }
    }
}







