using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Expression
    {
        private string _symbol;

        protected Func<Evaluator, double> EvalFunctional;
        protected Func<Evaluator, Variable, double> DiffFunctional;


        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        public virtual HashSet<Variable> Incidence()
        {
            return new HashSet<Variable>();
        }

        protected double GetValueOrCalculate(Evaluator evaluator, Func<Evaluator, double> function)
        {
            var value = Double.NaN;

            if (evaluator.ExpressionCache.TryGetValue(this, out value))
                return value;
            else
            {
                value = function(evaluator);
                evaluator.ExpressionCache.Add(this, value);
                return value;
            }
        }
        protected double GetDiffValueOrCalculate(Evaluator evaluator, Variable var, Func<Evaluator, Variable, double> function)
        {
            var value = Double.NaN;

            Dictionary<Expression, double> existingDict;
            var found = evaluator.DiffCache.TryGetValue(this, out existingDict);
            if (found && existingDict.TryGetValue(var, out value))
            {
                return value;
            }
            else
            {
                value = function(evaluator, var);
                if (Double.IsNaN(value))
                {
                    value = 0;
                }
                if (evaluator.DiffCache.ContainsKey(this))
                {
                    evaluator.DiffCache[this].Add(var, value);
                }
                else
                {
                    var dict = new Dictionary<Expression, double>();
                    dict.Add(var, value);
                    evaluator.DiffCache.Add(this, dict);
                }
                return value;
            }
        }
        public virtual double Diff(Evaluator evaluator, Variable var)
        {
            return GetDiffValueOrCalculate(evaluator, var, DiffFunctional);
        }

        public virtual Expression SymbolicDiff(Variable var)
        {
            return null;
        }

        public virtual double Eval(Evaluator evaluator)
        {
            return GetValueOrCalculate(evaluator, EvalFunctional);
        }

        public static implicit operator Expression(double d)
        {
            return new DoubleLiteral(d);
        }

        public static implicit operator Expression(int d)
        {
            return new IntegerLiteral(d);
        }

        public Equation IsEqualTo(int expr)
        {
            return new Equation(this, new IntegerLiteral(expr));
        }

        public Equation IsEqualTo(double expr)
        {
            return new Equation(this, new DoubleLiteral(expr));
        }

        public Equation IsEqualTo(Expression expr)
        {
            return new Equation(this, expr);
        }
        /* public Constraint IsEqualTo(int expr)
         {
             return new EqualityConstraint(this, new IntegerLiteral(expr));
         }

         public Constraint IsEqualTo(double expr)
         {
             return new EqualityConstraint(this, new DoubleLiteral(expr));
         }

         public Constraint IsEqualTo(Expression expr)
         {
             return new EqualityConstraint(this, expr);
         }

         public Constraint IsGreaterThanOrEqualTo(Expression expr)
         {
             return new GreaterThanOrEqualConstraint(this, expr);
         }

         public Constraint IsGreaterThanOrEqualTo(int expr)
         {
             return new GreaterThanOrEqualConstraint(this, new IntegerLiteral(expr));
         }

         public Constraint IsGreaterThanOrEqualTo(double expr)
         {
             return new GreaterThanOrEqualConstraint(this, new DoubleLiteral(expr));
         }


         public Constraint IsLessThanOrEqualTo(Expression expr)
         {
             return new LessThanOrEqualConstraint(this, expr);
         }

         public Constraint IsLessThanOrEqualTo(double expr)
         {
             return new LessThanOrEqualConstraint(this, new DoubleLiteral(expr));
         }

         public Constraint IsLessThanOrEqualTo(int expr)
         {
             return new LessThanOrEqualConstraint(this, new IntegerLiteral(expr));
         }
         */

        public virtual Expression Copy()
        {
            return MemberwiseClone() as Expression;
        }



        #region Addition

        public static Expression operator +(Expression u1, Expression u2)
        {
            return new Addition { Left = u1, Right = u2 };
        }

        public static Expression operator +(Expression u1, int u2)
        {
            return new Addition { Left = u1, Right = new IntegerLiteral { Value = u2 } };
        }

        public static Expression operator +(Expression u1, double u2)
        {
            return new Addition { Left = u1, Right = new DoubleLiteral { Value = u2 } };
        }

        public static Expression operator +(int u1, Expression u2)
        {
            return new Addition { Left = new IntegerLiteral { Value = u1 }, Right = u2 };
        }

        public static Expression operator +(double u1, Expression u2)
        {
            return new Addition { Left = new DoubleLiteral { Value = u1 }, Right = u2 };
        }

        #endregion

        #region Multiplication

        public static Expression operator *(Expression u1, Expression u2)
        {
            var u1lit = u1 as DoubleLiteral;
            if (u1lit != null && u1lit.Value == 0.0)
                return 0;
            if (u1lit != null && u1lit.Value ==1.0)
                return u2;


            var u2lit = u2 as DoubleLiteral;
            if (u2lit != null && u2lit.Value == 0.0)
                return 0;
            if (u2lit != null && u2lit.Value == 1.0)
                return u1;

            return new Multiplication { Left = u1, Right = u2 };
        }

        public static Expression operator *(Expression u1, int u2)
        {
            if (u2 == 0)
                return 0;
            if (u2 == 1)
                return u1;

            var u1lit = u1 as DoubleLiteral;
            if (u1lit != null && u1lit.Value == 0.0)
                return 0;
            if (u1lit != null && u1lit.Value == 1.0)
                return u2;

            return new Multiplication { Left = u1, Right = new IntegerLiteral { Value = u2 } };
        }

        public static Expression operator *(Expression u1, double u2)
        {
            if (u2 == 0.0)
                return 0;
            if (u2 == 1.0)
                return u1;

            var u1lit = u1 as DoubleLiteral;
            if (u1lit != null && u1lit.Value == 0.0)
                return 0;
            if (u1lit != null && u1lit.Value == 1.0)
                return u2;

            return new Multiplication { Left = u1, Right = new DoubleLiteral { Value = u2 } };
        }

        public static Expression operator *(int u1, Expression u2)
        {
            if (u1 == 0)
                return 0;
            if (u1 == 1)
                return u2;

            var u2lit = u2 as DoubleLiteral;
            if (u2lit != null && u2lit.Value == 0.0)
                return 0;
            if (u2lit != null && u2lit.Value == 1.0)
                return u1;

            return new Multiplication { Left = new IntegerLiteral { Value = u1 }, Right = u2 };


        }

        public static Expression operator *(double u1, Expression u2)
        {
            if (u1 == 0.0)
                return 0;
            if (u1 == 1.0)
                return u2;

            var u2lit = u2 as DoubleLiteral;
            if (u2lit != null && u2lit.Value == 0.0)
                return 0;
            if (u2lit != null && u2lit.Value == 1.0)
                return u1;

            return new Multiplication { Left = new DoubleLiteral { Value = u1 }, Right = u2 };
        }

        #endregion

        #region Subtraction

        public static Expression operator -(Expression u)
        {
            return new Negation { Child = u };
        }

        public static Expression operator -(Expression u1, Expression u2)
        {
            return new Subtraction { Left = u1, Right = u2 };
        }

        public static Expression operator -(Expression u1, int u2)
        {
            return new Subtraction { Left = u1, Right = new IntegerLiteral { Value = u2 } };
        }

        public static Expression operator -(Expression u1, double u2)
        {
            return new Subtraction { Left = u1, Right = new DoubleLiteral { Value = u2 } };
        }

        public static Expression operator -(int u1, Expression u2)
        {
            return new Subtraction { Left = new IntegerLiteral { Value = u1 }, Right = u2 };
        }

        public static Expression operator -(double u1, Expression u2)
        {
            return new Subtraction { Left = new DoubleLiteral { Value = u1 }, Right = u2 };
        }

        #endregion

        #region Division


        public static Expression operator /(Expression u1, Expression u2)
        {
            return new Division { Left = u1, Right = u2 };
        }

        public static Expression operator /(Expression u1, int u2)
        {
            return new Division { Left = u1, Right = new IntegerLiteral { Value = u2 } };
        }

        public static Expression operator /(Expression u1, double u2)
        {
            return new Division { Left = u1, Right = new DoubleLiteral { Value = u2 } };
        }

        public static Expression operator /(int u1, Expression u2)
        {
            return new Division { Left = new IntegerLiteral { Value = u1 }, Right = u2 };
        }

        public static Expression operator /(double u1, Expression u2)
        {
            return new Division { Left = new DoubleLiteral { Value = u1 }, Right = u2 };
        }

        #endregion
    }
}
