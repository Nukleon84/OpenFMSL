using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public class EnthalpyRoute : Expression

    {
        ThermodynamicSystem _system;
        int NC;
        Variable T;
        Variable p;
        List<Variable> x;

        Expression _htotal;
        Expression[] _hi;
        PhaseState _phase = PhaseState.Liquid;

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

        public PhaseState Phase
        {
            get
            {
                return _phase;
            }

            set
            {
                _phase = value;
            }
        }

        public EnthalpyRoute(ThermodynamicSystem system, Variable T, Variable p, List<Variable> x, PhaseState phase)
        {
            Symbol = "H" + (phase == PhaseState.Liquid ? "L" : "V");
            _system = system;

            this.T = T;
            this.p = p;
            this.x = x;

            Parameters.Add(T);
            Parameters.Add(p);
            foreach (var comp in x)
                Parameters.Add(comp);

            NC = _system.Components.Count;

            _hi = new Expression[NC];


            for (int i = 0; i < NC; i++)
            {
                if (phase == PhaseState.Liquid)
                    _hi[i] = x[i] * _system.EquationFactory.GetLiquidEnthalpyExpression(_system, i, T);
                else
                    _hi[i] = x[i] * _system.EquationFactory.GetVaporEnthalpyExpression(_system, i, T);
            }

            _htotal = Sym.Binding(Symbol, (Sym.Sum(0, NC, (idx) => _hi[idx])));
            DiffFunctional = (cache, v) => NumDiff(cache, v);
            EvalFunctional = (cache) => Evaluate(cache);
        }


        double NumDiff(Evaluator cache, Variable v)
        {
            if (v == T || v == p)
                return _htotal.Diff(cache, v);
            else
            {
                int idx = Parameters.IndexOf(v) - 2;
                return _hi[idx].Diff(cache, x[idx]);
            }

            
        }
        double Evaluate(Evaluator cache)
        {
            return _htotal.Eval(cache);
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
            return Symbol + "(T,p,x)";
        }

    }
}
