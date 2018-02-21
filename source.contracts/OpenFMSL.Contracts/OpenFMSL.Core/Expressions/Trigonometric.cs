using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Cos : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Cos()
        {
            Name = "cos";
            Symbol = "cos";

            EvalFunctional = (c) => Math.Cos(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return -(Math.Sin(u) * dudx);
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);

            return -Sym.Sin(u) * dudx;
        }

    }

    public class Sin : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Sin()
        {
            Name = "sin";
            Symbol = "sin";

            EvalFunctional = (c) => Math.Sin(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math.Cos(u) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);

            return Sym.Cos(u) * dudx;
        }


    }
    public class Sinh : UnaryFunction
    {

        public Sinh()
        {
            Name = "sinh";
            Symbol = "sinh";

            EvalFunctional = (c) => Math.Sinh(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math.Cosh(u) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);
            return Sym.Cosh(u) * dudx;
        }


    }
    public class Cosh : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Cosh()
        {
            Name = "cosh";
            Symbol = "cosh";

            EvalFunctional = (c) => Math.Cosh(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math.Sinh(u) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);
            return Sym.Sinh(u) * dudx;
        }


    }

    public class Coth : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Coth()
        {
            Name = "coth";
            Symbol = "coth";

            EvalFunctional = (c) => Math2.Coth(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return -Math.Pow(Math2.Cosech(u), 2) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);
            return -Sym.Pow(Sym.Cosech(u), 2) * dudx;
        }


    }

    public class Tanh : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Tanh()
        {
            Name = "tanh";
            Symbol = "tanh";

            EvalFunctional = (c) => Math.Tanh(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math.Pow(Math2.Sech(u), 2) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);
            return Sym.Pow(Sym.Sech(u), 2) * dudx;
        }

    }


    public class Cosech : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Cosech()
        {
            Name = "cosech";
            Symbol = "cosech";

            EvalFunctional = (c) => Math2.Cosech(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math2.Cosech(u) * Math2.Coth(u) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);

            return Sym.Cosech(u) * Sym.Coth(u) * dudx;
        }

    }


    public class Sech : UnaryFunction
    {
        // 	 sqrt(u) 	1/(2*sqrt(u))*du/dx

        public Sech()
        {
            Name = "sech";
            Symbol = "sech";

            EvalFunctional = (c) => Math2.Sech(Parameter.Eval(c));
            DiffFunctional = (c, var) =>
            {
                var u = Parameter.Eval(c);
                var dudx = Parameter.Diff(c, var);
                return Math2.Sech(u) * Math.Tanh(u) * dudx;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Parameter;
            var dudx = Parameter.SymbolicDiff(var);

            return Sym.Sech(u) * Sym.Tanh(u) * dudx;
        }


    }

}
