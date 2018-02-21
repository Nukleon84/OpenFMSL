using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{    
    public abstract class Literal : Expression
    {
    }
        
    public class DoubleLiteral : Literal
    {
        private double _value;

        public DoubleLiteral(double value)
        {
            Value = value;
        }

        public DoubleLiteral()
        {
            Value = 0;
        }

        public double Value
        {
            get { return _value; }
            set { this._value = value; }
        }
        
        public override double Eval(Evaluator evaluator)
        {
            return Value;
        }

        public override double Diff(Evaluator evaluator, Variable var)
        {
            return 0;
        }

        public override string ToString()
        {
            return Value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        public override Expression SymbolicDiff(Variable var)
        {
            return new IntegerLiteral { Value = 0 };
        }
    }

    public class IntegerLiteral : Literal
    {
        private int _value;

        public IntegerLiteral()
        {
            Value = 0;
        }

        public IntegerLiteral(int value)
        {
            Value = value;
        }

        public int Value
        {
            get { return _value; }
            set { this._value = value; }
        }

        public override double Diff(Evaluator evaluator, Variable var)
        {
            return 0;
        }



        public override string ToString()
        {
            return Value.ToString();
        }

        public override double Eval(Evaluator evaluator)
        {
            return Value;
        }

        public override Expression SymbolicDiff(Variable var)
        {
            return new IntegerLiteral { Value = 0 };
        }
    }
}
