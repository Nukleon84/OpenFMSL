using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Power : Binary
    {       
        // 	u^v 	v*u^(v-1)*du/dx+u^v*ln(u)*dv/dx 	

        public Power()
        {
            Symbol = "^";

            EvalFunctional = (c) => Math.Pow(Left.Eval(c), Right.Eval(c));
            DiffFunctional = (c, var) =>
             {
                 var u = Left.Eval(c);
                 var v = Right.Eval(c);
                 var dudx = Left.Diff(c, var);
                 var dvdx = Right.Diff(c, var);
                 return v * Math.Pow(u, v - 1) * dudx + Math.Pow(u, v) * Math.Log(u) * dvdx;
             };

        }
   

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Left;
            var v = Right;
            var dudx = Left.SymbolicDiff(var);
            var dvdx = Right.SymbolicDiff(var);
            return v * Sym.Pow(u, v - 1) * dudx + Sym.Pow(u, v) * Sym.Ln(u) * dvdx;
        }
    }
    
    public abstract class UnaryFunction : Expression
    {
        private string _name;
        private Expression _parameter;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Expression Parameter
        {
            get { return _parameter; }
            set
            {
                _parameter = value;
            }
        }

        public override HashSet<Variable> Incidence()
        {
            return Parameter.Incidence();
        }

        public override string ToString()
        {
            return Name + "(" + Parameter + ")";
        }
    }

    public class Sqrt : UnaryFunction
    {  
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Sqrt()
        {
            Name = "sqrt";
            Symbol = "sqrt";

            EvalFunctional = (c) => Math.Sqrt(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var dudx = Parameter.Diff(c, var);
                var value = 1.0 / (2.0 * Eval(c)) * dudx;
                return value;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var dudx = Parameter.SymbolicDiff(var);
            return 1.0 / (2.0 * this) * dudx;
        }
        
    }

    public class Ln : UnaryFunction
    {
        public Ln()
        {
            Name = "ln";
            Symbol = "ln";

            EvalFunctional = (c) => Math.Log(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return 1 / u * dudx;
            };

        }
        
        //  	ln(u) 	1/u*du/dx
        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);

            return 1 / u * dudx;
        }

    }

    public class Exp : UnaryFunction
    {
        public Exp(Expression param)
            : this()
        {
            Parameter = param;
        }

        public Exp()
        {
            Name = "exp";
            Symbol = "exp";

            EvalFunctional = (c) => Math.Exp(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math.Exp(u) * dudx;
            };

        }
          
        // 	e^u 	e^u*du/dx
        public override Expression SymbolicDiff(Variable var)
        {
            var dudx = Parameter.SymbolicDiff(var);

            return this * dudx;
        }

    }

    public class Convert : Expression
    {
        private Unit _conversionTarget;

        private Variable _variable;

        public Variable Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }

        public Unit ConversionTarget
        {
            get { return _conversionTarget; }
            set { _conversionTarget = value; }
        }

        public Convert()
        {
            EvalFunctional = (c) =>
            {
                if (Variable.InternalUnit == ConversionTarget)
                    return Variable.Eval(c);
                else
                    return Unit.Convert(Variable.InternalUnit, ConversionTarget, Variable.Eval(c));
            };
            DiffFunctional = (c, var) =>
            {
                return Unit.GetConversionFactor(Variable.InternalUnit, ConversionTarget) * Variable.Diff(c, var);
            };
        }

        public override HashSet<Variable> Incidence()
        {
            return Variable.Incidence();
        }
        
        public override Expression SymbolicDiff(Variable var)
        {
            return Unit.GetConversionFactor(Variable.InternalUnit, ConversionTarget) * Variable.SymbolicDiff(var);
        }

        public override string ToString()
        {
            return "" + Variable + "(in " + ConversionTarget + ")";
        }
    }
}
