using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Equation
    {
        Expression _left;
        Expression _right;
        HashSet<Variable> _incidenceVector;

        string _name;
        string _modelClass;
        string _modelName;
        string _group;
        string _description;

        bool _isActive = true;


        public Expression Left
        {
            get
            {
                return _left;
            }

            protected set
            {
                _left = value;
            }
        }

        public Expression Right
        {
            get
            {
                return _right;
            }

            protected set
            {
                _right = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public string ModelClass
        {
            get
            {
                return _modelClass;
            }

            set
            {
                _modelClass = value;
            }
        }

        public string ModelName
        {
            get
            {
                return _modelName;
            }

            set
            {
                _modelName = value;
            }
        }

        public string Group
        {
            get
            {
                return _group;
            }

            set
            {
                _group = value;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                _description = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                _isActive = value;
            }
        }

        public Equation(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }
        public Equation(Expression left)
        {
            Left = left;
            Right = 0;
        }

        public Equation Copy()
        {
            var copy = new Equation(Left.Copy(), Right.Copy());
            copy.Name = Name;         
            copy.Group = Group;
            copy.ModelClass = ModelClass;
            copy.ModelName = ModelName;
            copy.Description = Description;           
            return copy;
        }



        public double Residual(Evaluator evaluator)
        {
            return Right.Eval(evaluator) - Left.Eval(evaluator);           
        }

        public double Diff(Evaluator evaluator, Variable var)
        {
            return Right.Diff(evaluator,var) - Left.Diff(evaluator,var);
        }

        public void ResetIncidenceVector()
        {
            _incidenceVector = null;
        }

        public HashSet<Variable> Incidence(Evaluator evaluator)
        {
            if (_incidenceVector == null)
            {
                var ileft = Left.Incidence();
                var iright = Right.Incidence();

                _incidenceVector = new HashSet<Variable>();
                _incidenceVector.UnionWith(ileft);
                _incidenceVector.UnionWith(iright);               
                return _incidenceVector;
            }
            else
                return _incidenceVector;
        }

        public override string ToString()
        {
            return Left.ToString() + " == " + Right.ToString();
        }
    }
}
