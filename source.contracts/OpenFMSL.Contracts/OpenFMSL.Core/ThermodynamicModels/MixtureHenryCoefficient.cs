using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public class MixtureHenryCoefficient : Expression
    {
        ThermodynamicSystem _system;
        int index = -1;
        int NC;

        Variable T;
        List<Variable> x;
        private List<Expression> _parameters = new List<Expression>();

        Expression _henry;
        Expression _dhenryDT;
        Expression[] _dhenryDx;


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

        public MixtureHenryCoefficient(ThermodynamicSystem system, Variable T, List<Variable> x, int idx)
        {
            index = idx;
            _system = system;

            var parameterSet = _system.BinaryParameters.FirstOrDefault(ps => ps.Name == "HENRY");
            if (parameterSet == null)
                throw new ArgumentNullException("No HENRY parameters defined");

            double[,] a = parameterSet.Matrices["A"];
            double[,] b = parameterSet.Matrices["B"];
            double[,] c = parameterSet.Matrices["C"];
            double[,] d = parameterSet.Matrices["D"];


            Symbol = "HENRY";

            this.T = T;
            this.x = x;

            Parameters.Add(T);
            foreach (var comp in x)
                Parameters.Add(comp);

            NC = system.Components.Count;

            Expression sumxj = 0;
            Expression _lnHenry = 0;

            for (int j = 0; j < system.Components.Count; j++)
            {
                if (!system.Components[j].IsInert)
                {

                    Expression lnHij = a[idx, j];
                    if (b[idx, j] != 0.0)
                        lnHij += b[idx, j] / T;
                    if (c[idx, j] != 0.0)
                        lnHij += c[idx, j] * Sym.Ln(T);
                    if (d[idx, j] != 0.0)
                        lnHij += d[idx, j] * T;

                    sumxj += x[j];
                    _lnHenry += x[j] * Sym.Par(lnHij);
                }
            }

            _lnHenry = _lnHenry / Sym.Par(sumxj);

            _henry = Sym.Exp(_lnHenry);

            _dhenryDx = new Expression[NC];

            DiffFunctional = (cache, v) => _henry.Diff(cache, v);
            EvalFunctional = (cache) => _henry.Eval(cache);
        }


        public override Expression SymbolicDiff(Variable var)
        {
            if (T == var)
            {
                if (_dhenryDT == null)
                    _dhenryDT = _henry.SymbolicDiff(var);
                return _dhenryDT;
            }

            for (int i = 0; i < NC; i++)
            {
                if (x[i] == var)
                {

                    if (_dhenryDx[i] == null)
                        _dhenryDx[i] = _henry.SymbolicDiff(var);
                    return _dhenryDx[i];
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
            return "Henry(T,x)";
        }

    }
}
