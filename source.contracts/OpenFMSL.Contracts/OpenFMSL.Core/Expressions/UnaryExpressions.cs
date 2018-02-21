using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public abstract class Unary : Expression
    {
        private Expression _child;

        public Expression Child
        {
            get { return _child; }
            set
            {
                _child = value;

            }
        }

        public override HashSet<Variable> Incidence()
        {
            return Child.Incidence();
        }


    }

    public class Parentheses : Unary
    {
        public Parentheses()
        {
            Symbol = "(";

            EvalFunctional = (c) => (Child.Eval(c));
            DiffFunctional = (c, var) => (Child.Diff(c, var));

        }

        public Parentheses(Expression expr):this()
        {         
            Child = expr;
        }
        
        public override Expression SymbolicDiff(Variable var)
        {
            return Child.SymbolicDiff(var);
        }

        public override string ToString()
        {
            return "(" + Child + ")";
        }

    }

    public class Negation : Unary
    {
        public Negation()
        {
            Symbol = "neg";

            EvalFunctional = (c) => -(Child.Eval(c));
            DiffFunctional = (c, var) => -(Child.Diff(c, var));


        }
        
        public override Expression SymbolicDiff(Variable var)
        {
            return new Negation { Child = Child.SymbolicDiff(var) };
        }

        public override string ToString()
        {
            return "-" + "" + Child + "";
        }

    }

    public class Abs : Unary
    {

        public Abs()
        {
            Symbol = "abs";

            EvalFunctional = (c) => Math.Abs(Child.Eval(c));
            DiffFunctional = (c, var) => Math.Abs(Child.Diff(c, var));

        }

        public Abs(Expression x) : this()
        {
            Child = x;
        }

        public override Expression SymbolicDiff(Variable var)
        {
            return Sym.Sqrt(Sym.Pow(Child, 2) + 1e-12).SymbolicDiff(var);
        }

        public override string ToString()
        {
            return "|" + Child + "|";
        }

    }

}
