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

              //  var u = Left.Diff(c, var);
               // var v = Right.Diff(c, var);

                if (Left.Eval(c) < Right.Eval(c))
                    return Left.Diff(c, var);
                else
                    return Right.Diff(c, var);
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

    //log(exp(kx) + exp(ky) ) / k


    public class SmoothMax : Binary
    {
        Expression _expr;

        public SmoothMax(Expression left, Expression right, double k=10)
        {
            Symbol = "smax";
            Left = left;
            Right = right;
            //_expr = 0.5 * (Left + Right + Sym.Abs(Left - Right));
            _expr = Sym.Ln(Sym.Exp(k * left) + Sym.Exp(k * right)) / k;

            EvalFunctional = (c) => Math.Max(Left.Eval(c), Right.Eval(c));
            DiffFunctional = (c, var) =>  _expr.Diff(c, var);            
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
    public class SmoothMin : Binary
    {
        Expression _expr;

        public SmoothMin(Expression left, Expression right, double k = 10)
        {
            Symbol = "smin";
            Left = left;
            Right = right;
            //_expr = 0.5 * (Left + Right + Sym.Abs(Left - Right));
            _expr = Sym.Ln(Sym.Exp(-k * left) + Sym.Exp(-k * right)) / -k;

            EvalFunctional = (c) => Math.Min(Left.Eval(c), Right.Eval(c));
            DiffFunctional = (c, var) => _expr.Diff(c, var);           
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
                 
                // var u = Left.Diff(c, var);
                // var v = Right.Diff(c, var);

                 if (Left.Eval(c) > Right.Eval(c))
                     return Left.Diff(c, var);
                 else
                     return Right.Diff(c, var);
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
