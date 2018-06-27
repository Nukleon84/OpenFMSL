using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public static class Sym
    {
        public static Variable Binding(string name, Expression expr)
        {
            var variable = new Variable(name, 1.0);
            variable.BindTo(expr);
            return variable;
        }
        public static Expression Par(Expression expr)
        {
            return new Parentheses(expr);
        }

        public static Expression Pow(Expression param, Expression exp)
        {
            return new Power { Left = param, Right = exp };
        }

        public static Expression Pow(Expression param, double exp)
        {
            return new Power { Left = param, Right = new DoubleLiteral(exp) };
        }

        public static Expression Pow(Expression param, int exp)
        {
            return new Power { Left = param, Right = new IntegerLiteral(exp) };
        }

        public static Expression Ln(Expression param)
        {
            return new Ln { Parameter = param };
        }

        public static Expression Exp(Expression param)
        {
            return new Exp { Parameter = param };
        }

        public static Expression Sqrt(Expression param)
        {
            return new Sqrt { Parameter = param };
        }

        public static Expression Abs(Expression param)
        {
            return new Abs { Child = param };
        }

        public static Expression Sin(Expression param)
        {
            return new Sin { Parameter = param };
        }

        public static Expression Cos(Expression param)
        {
            return new Cos { Parameter = param };
        }

        public static Expression Cosech(Expression param)
        {
            return new Cosech { Parameter = param };
        }

        public static Expression Sech(Expression param)
        {
            return new Sech { Parameter = param };
        }

        public static Expression Coth(Expression param)
        {
            return new Coth { Parameter = param };
        }
        public static Expression Sinh(Expression param)
        {
            return new Sinh { Parameter = param };
        }
        public static Expression Cosh(Expression param)
        {
            return new Cosh { Parameter = param };
        }
        public static Expression Tanh(Expression param)
        {
            return new Tanh { Parameter = param };
        }

        public static Expression Min(Expression left, Expression right)
        {
            return new Min(left, right);
        }
        public static Expression Max(Expression left, Expression right)
        {
            return new Max(left, right);
        }

        public static Expression SmoothMax(Expression left, Expression right)
        {
            return new SmoothMax(left, right);
        }
        public static Expression SmoothMin(Expression left, Expression right)
        {
            return (new SmoothMin(left, right));
        }

        public static Expression SumX(int start, int end, int excludedIndex, Func<int, Expression> mapping)
        {
            var sum = mapping(start);
            for (var i = start + 1; i < end; i++)
            {
                if (i != excludedIndex)
                    sum += mapping(i);
            }
            return Par(sum);
        }

        public static Expression Sum(int start, int end, Func<int, Expression> mapping)
        {
            var sum = mapping(start);
            for (var i = start + 1; i < end; i++)
            {
                sum += mapping(i);
            }
            return Par(sum);
        }
        public static Expression Sum(IEnumerable<Expression> array, Func<Expression, int, Expression> mapping)
        {
            var sum = mapping(array.ElementAt(0), 0);
            for (var i = 1; i < array.Count(); i++)
            {
                sum += mapping(array.ElementAt(i), i);
            }
            return Par(sum);
        }

        public static Expression Product(Expression[] array, Func<Expression, int, Expression> mapping)
        {
            var sum = mapping(array[0], 0);
            for (var i = 1; i < array.Length; i++)
            {
                sum *= mapping(array[i], i);
            }
            return Par(sum);
        }

        public static Expression Sum(Variable[] array, Func<Expression, int, Expression> mapping)
        {
            var sum = (Expression)array[0];
            for (var i = 1; i < array.Length; i++)
            {
                sum += mapping(array.ElementAt(i), i);
            }
            return Par(sum);
        }
        public static Expression Sum(List<Variable> array)
        {
            var sum = (Expression)array[0];
            for (var i = 1; i < array.Count; i++)
            {
                sum += array[i];
            }
            return Par(sum);
        }
        public static Expression Sum(Variable[] array)
        {
            var sum = (Expression)array[0];
            for (var i = 1; i < array.Length; i++)
            {
                sum += array[i];
            }
            return Par(sum);
        }

        public static Expression Sum(Expression[] array)
        {
            var sum = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                sum += array[i];
            }
            return Par(sum);
        }



        public static Expression Convert(Variable param, UnitsOfMeasure.Unit targetUnit)
        {
            return new Convert() { Variable = param, ConversionTarget = targetUnit };
        }
    }
}
