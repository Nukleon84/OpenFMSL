using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public enum ConstraintComparisonOperator { LessThanOrEqual, GreaterThanOrEqual };

    public class Constraint : Equation
    {
        ConstraintComparisonOperator _operator;
        public Constraint(Expression left, ConstraintComparisonOperator comparison, Expression right) : base(left, right)
        {
            _operator = comparison;
        }

        public double LowerBound
        {
            get
            {
                if(_operator== ConstraintComparisonOperator.LessThanOrEqual)
                    return 0;
                if (_operator == ConstraintComparisonOperator.GreaterThanOrEqual)
                    return -1e20;
                return 0;
            }
        }

        public double UpperBound
        {
            get
            {
                if (_operator == ConstraintComparisonOperator.LessThanOrEqual)
                    return 1e20;
                if (_operator == ConstraintComparisonOperator.GreaterThanOrEqual)
                    return 0;
                return 0;
            }
        }

        public new Constraint Copy()
        {
            var copy = new Constraint(Left.Copy(), _operator, Right.Copy());
            copy.Name = Name;
            copy.Group = Group;
            copy.ModelClass = ModelClass;
            copy.ModelName = ModelName;
            copy.Description = Description;
            return copy;
        }

        public override string ToString()
        {
            if (_operator == ConstraintComparisonOperator.LessThanOrEqual)
                return Left.ToString() + " <= " + Right.ToString();
            if (_operator == ConstraintComparisonOperator.GreaterThanOrEqual)
                    return Left.ToString() + " >= " + Right.ToString();

            return Left.ToString() + " == " + Right.ToString();
        }
    }
}
