using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public class SafeLiquidDensity : Expression
    {
        Expression _densityLiquid;
        Expression _densityVapor;

        public SafeLiquidDensity(Expression densityLiquid, Expression densityVapor)
        {
            _densityLiquid = densityLiquid;
            _densityVapor = densityVapor;
            Symbol = "+";

            EvalFunctional = (c) =>
            {
                var dens = _densityLiquid.Eval(c);
                if (Double.IsNaN(dens) || Math.Abs(dens) < 1e-12)
                    return _densityVapor.Eval(c);
                else
                    return dens;
            };

            DiffFunctional = (c, v) => _densityLiquid.Diff(c, v);
        }


        public override Expression SymbolicDiff(Variable var)
        {
            return _densityLiquid.SymbolicDiff(var);
        }

        public override string ToString()
        {
            return "SafeLiquidDensity()";
        }
    }
}
