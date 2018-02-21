using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Binary : Expression
    {
        private Expression _left;
        private Expression _right;

        public Expression Left
        {
            get { return _left; }
            set
            {
                _left = value;
            }
        }

        public Expression Right
        {
            get { return _right; }
            set
            {
                _right = value;
            }
        }

        public override HashSet<Variable> Incidence()
        {
            var ileft = Left.Incidence();
            var iright = Right.Incidence();

            var inc = new HashSet<Variable>();
            inc.UnionWith(ileft);
            inc.UnionWith(iright);
            return inc;
        }



        public override string ToString()
        {
            return Left + " " + Symbol + " " + Right;
        }
    }

    public class Addition : Binary
    {
        public Addition()
        {
            Symbol = "+";

            EvalFunctional = (c) => Left.Eval(c) + Right.Eval(c);
            DiffFunctional = (c, v) => Left.Diff(c, v) + Right.Diff(c, v);
        }


        public override Expression SymbolicDiff(Variable var)
        {
            return Left.SymbolicDiff(var) + Right.SymbolicDiff(var);
        }
    }

    public class Subtraction : Binary
    {
        public Subtraction()
        {
            Symbol = "-";
            EvalFunctional = (c) => Left.Eval(c) - Right.Eval(c);
            DiffFunctional = (c, var) => Left.Diff(c, var) - Right.Diff(c, var);
        }

        public override Expression SymbolicDiff(Variable var)
        {
            return Left.SymbolicDiff(var) - Right.SymbolicDiff(var);
        }
    }

    public class Multiplication : Binary
    {
        //	u*v 	u*dv/dx+v*du/dx

        public Multiplication()
        {
            Symbol = "*";

            EvalFunctional = (c) =>
            {
                var value = Double.NaN;
                var leftval = Left.Eval(c);
                if (Math.Abs(leftval) <= Double.Epsilon)
                    value = 0;
                else
                    value = leftval * Right.Eval(c);
                return value;
            };

            DiffFunctional = (c, var) =>
             {
                 var value = Double.NaN;
                 var u = Left.Eval(c);
                 var v = Right.Eval(c);

                 double dudx = 0;
                 double dvdx = 0;

                 if (Math.Abs(v) > Double.Epsilon)
                     dudx = Left.Diff(c, var);

                 if (Math.Abs(u) > Double.Epsilon)
                     dvdx = Right.Diff(c, var);


                // var dudx = Left.Diff(c, var);
               //  var dvdx = Right.Diff(c, var);

                 value = u * dvdx + v * dudx;
                 return value;
             };

        }


        public override Expression SymbolicDiff(Variable var)
        {
            var u = Left;
            var v = Right;

            var dudx = Left.SymbolicDiff(var);
            var dvdx = Right.SymbolicDiff(var);

            return u * dvdx + v * dudx;
        }


    }

    public class Division : Binary
    {
        //u/v 	(v*du/dx-u*dv/dx)/v^2

        public Division()
        {
            Symbol = "/";

            EvalFunctional = (c) =>
            {
                var value = Double.NaN;
                var leftval = Left.Eval(c);

                if (Math.Abs(leftval) <= Double.Epsilon)
                    value = 0;
                else
                {
                    var rightVal = Right.Eval(c);
                    if (Math.Abs(rightVal) <= Double.Epsilon)
                        return 0;
                    value = leftval / rightVal;
                }
                return value;
            };

            DiffFunctional = (c, var) =>
              {
                  var value = Double.NaN;
                  var u = Left.Eval(c);
                  var v = Right.Eval(c);
                  double dudx = 0;
                  double dvdx = 0;

                  if (Math.Abs(v) > Double.Epsilon)
                      dudx = Left.Diff(c, var);

                  if (Math.Abs(u) > Double.Epsilon)
                       dvdx = Right.Diff(c, var);

                  value = (v * dudx - u * dvdx) / Math.Pow(v, 2);
                  return value;
              };
        }

        public override Expression SymbolicDiff(Variable var)
        {
            var u = Left;
            var v = Right;
            var dudx = Left.SymbolicDiff(var);
            var dvdx = Right.SymbolicDiff(var);

            return (v * dudx - u * dvdx) / Sym.Pow(v, 2);
        }
    }
}
