using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Min : Binary
    {
        Expression _expr;

        public Min(Expression left, Expression right)
        {
            Symbol = "min";
            Left = left;
            Right = right;
            _expr = 0.5 * (Left + Right - Sym.Abs(Left - Right));

            EvalFunctional = (c) => Math.Min(Left.Eval(c), Right.Eval(c));
            DiffFunctional = (c, var) =>
            {
                // var diffExpr = _expr.SymbolicDiff(var);
                //  return diffExpr.Eval(c);

                var u = Left.Diff(c, var);
                var v = Right.Diff(c, var);

                if (Left.Eval(c) < Right.Eval(c))
                    return u;
                else
                    return v;
            };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            return _expr.SymbolicDiff(var);
        }

        public override string ToString()
        {
            return Symbol + "(" + Left + "," + Right + ")";
        }



    }


    public class Max : Binary
    {
        Expression _expr;

        public Max(Expression left, Expression right)
        {
            Symbol = "max";
            Left = left;
            Right = right;
            _expr = 0.5 * (Left + Right + Sym.Abs(Left - Right));
            EvalFunctional = (c) => Math.Max(Left.Eval(c), Right.Eval(c));
            DiffFunctional = (c, var) =>
             {
                 var u = Left.Diff(c, var);
                 var v = Right.Diff(c, var);

                 if (Left.Eval(c) > Right.Eval(c))
                     return u;
                 else
                     return v;
             };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            return _expr.SymbolicDiff(var);
        }

        public override string ToString()
        {
            return Symbol + "(" + Left + "," + Right + ")";
        }
    }


}
