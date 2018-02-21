using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Expressions
{
    public class Variable : Expression
    {
        private bool _isFixed = false;
        private bool _isConstant = false;

        private string _description;
        private string _name;
        private string _subscript;
        private string _superscript;
        private string _modelName;
        private string _modelClass;
        private string _group;

        private double _value;
        private double _upperBound = 1e20;
        private double _lowerBound = -1e20;
        private Unit _internalUnit = SI.nil;
        private Unit _inputUnit = SI.nil;
        private Unit _outputUnit = SI.nil;
        private PhysicalDimension _dimension = PhysicalDimension.Dimensionless;

        private Expression _definingExpression = null;
        HashSet<Variable> _incidence = null;

        public Variable()
        {
            Symbol = "Variable";
        }


        public Variable(string name, double value)
        {
            Name = name;
            ValueInSI = value;
            Symbol = "Variable";
        }

        public Variable(string name, double value, double lowerBound, double upperBound)
        {
            Name = name;
            ValueInSI = value;
            UpperBound = upperBound;
            LowerBound = lowerBound;
            Symbol = "Variable";
        }

        public Variable(string name, double value, Unit internalUnit, string description)
                : this(name, value, internalUnit)
        {
            Description = description;
        }

        public Variable(string name, double value, Unit internalUnit)
        {
            Name = name;
            ValueInSI = value;
            InternalUnit = internalUnit;
            InputUnit = internalUnit;
            OutputUnit = internalUnit;
            Symbol = "Variable";
        }



        public Variable(string name, double value, double lowerBound, double upperBound, Unit internalUnit, string description)
                : this(name, value, lowerBound, upperBound, internalUnit)
        {
            Description = description;
        }


        public Variable(string name, double value, double lowerBound, double upperBound, Unit internalUnit)
                : this(name, value, internalUnit)
        {
            UpperBound = upperBound;
            LowerBound = lowerBound;
        }

        public Unit InternalUnit
        {
            get { return _internalUnit; }
            set { _internalUnit = value; }
        }

        public Unit InputUnit
        {
            get { return _inputUnit; }
            set { _inputUnit = value; }
        }

        public Unit OutputUnit
        {
            get { return _outputUnit; }
            set { _outputUnit = value; }
        }


        public bool IsFixed
        {
            get { return _isFixed; }
            set { _isFixed = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string FullName
        {
            //get { return String.Format("{0}^{{{1}}}_{{{2}}}", _name, _superscript, _subscript); }
            get
            {
                return Name + (!string.IsNullOrEmpty(Superscript) ? "[" + Superscript + "]" : "") +
                (!string.IsNullOrEmpty(Subscript) ? "[" + Subscript + "]" : "");
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public double ValueInSI
        {
            get { return _value; }
            set { _value = value; }
        }

        public double ValueInOutputUnit
        {
            get { return GetValue(); }
            set
            {
                SetValue(value, OutputUnit);
            }
        }

        public string Subscript
        {
            get { return _subscript; }
            set { _subscript = value; }
        }

        public string Superscript
        {
            get { return _superscript; }
            set { _superscript = value; }
        }





        public double UpperBound
        {
            get { return _upperBound; }
            set { _upperBound = value; }
        }

        public double LowerBound
        {
            get { return _lowerBound; }
            set { _lowerBound = value; }
        }




        public Expression DefiningExpression
        {
            get { return _definingExpression; }
        }

        public PhysicalDimension Dimension
        {
            get
            {
                return _dimension;
            }

            set
            {
                _dimension = value;
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

        public bool IsConstant
        {
            get
            {
                return _isConstant;
            }

            set
            {
                _isConstant = value;
            }
        }

        public override HashSet<Variable> Incidence()
        {
            if (DefiningExpression != null)
            {
                if (_incidence == null)
                    _incidence = DefiningExpression.Incidence();
                return _incidence;
            }

            return new HashSet<Variable> { this };
        }

        public void BindTo(Expression expression)
        {
            _definingExpression = expression;
            _incidence = null;

        }

        public void Unbind()
        {
            _definingExpression = null;
            _incidence = null;
        }


        public override Expression Copy()
        {
            var copy = new Variable();
            copy.Name = Name;
            copy.Subscript = Subscript;
            copy.Superscript = Superscript;
            copy.LowerBound = LowerBound;
            copy.UpperBound = UpperBound;
            copy.ValueInSI = ValueInSI;
            copy.Symbol = Symbol;

            copy.Group = Group;
            copy.ModelClass = ModelClass;
            copy.ModelName = ModelName;
            copy.Description = Description;
            copy.Dimension = Dimension;
            copy.IsConstant = IsConstant;
            copy.IsFixed = IsFixed;

            copy.InternalUnit = InternalUnit;
            copy.InputUnit = InputUnit;
            copy.OutputUnit = OutputUnit;
            return copy;
        }


        public override double Diff(Evaluator evaluator, Variable var)
        {
            if (DefiningExpression != null)
            {
                if (_incidence == null)
                    Incidence();

                if (_incidence.Contains(var))
                {
                    return DefiningExpression.Diff(evaluator, var);
                }
                else
                    return 0;
            }

            return this == var ? 1 : 0;
        }

        public override Expression SymbolicDiff(Variable var)
        {
            if (DefiningExpression != null)
            {
                if (_incidence == null)
                    Incidence();

                if (_incidence.Contains(var))
                {
                    // var dxdy = new Variable("d" + Name + "d" + var.Name, 1);
                    //dxdy.BindTo(DefiningExpression.Diff(var));
                    //return dxdy;
                    return DefiningExpression.SymbolicDiff(var);
                }
                else
                    return 0;
            }


            return new IntegerLiteral { Value = this == var ? 1 : 0 };
        }

        public string WriteReport()
        {
            return string.Format("[{5,-20}] {4,10}.{0,-25} = {1, 12} {2,-15} {3}", FullName, GetValue(OutputUnit).ToString("G4"), OutputUnit.Symbol,
                Description, ModelName, ModelClass);
        }

        public string WriteReport(string offset)
        {
            return offset + WriteReport();
        }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(ModelName))
                return ModelName + "." + FullName;
            else
                return FullName;
        }

        public override double Eval(Evaluator evaluator)
        {
            if (DefiningExpression != null)
            {
                ValueInSI = DefiningExpression.Eval(evaluator);

                if (Double.IsNaN(ValueInSI))
                    ValueInSI = 0;
                if (ValueInSI > UpperBound)
                    ValueInSI = UpperBound;
                if (ValueInSI < LowerBound)
                    ValueInSI = LowerBound;

              //  ValueInSI = Math.Max(Math.Min(ValueInSI, UpperBound), LowerBound);

            }

            return ValueInSI;
        }
        public void UnfixAndSetBounds(double lower, double upper)
        {
            IsFixed = false;
            LowerBound = lower;
            UpperBound = upper;
        }

        public void SetValue(double value)
        {
            var convertedValue = Unit.Convert(InputUnit, InternalUnit, value);
            ValueInSI = convertedValue;

        }

        public void SetValue(double value, Unit newUnit)
        {
            if (Unit.AreSameDimension(newUnit, InternalUnit))
            {
                var convertedValue = Unit.Convert(newUnit, InternalUnit, value);
                ValueInSI = convertedValue;

            }
            else
                throw new Exception("Dimensions of assignment do not fit." + newUnit + " " + InternalUnit);
        }

        public void Unfix()
        {
            IsFixed = false;
        
        }

        public void FixValue(double value)
        {
            var convertedValue = Unit.Convert(InputUnit, InternalUnit, value);
            ValueInSI = convertedValue;
            IsFixed = true;

        }

        public void FixValue(double value, Unit newUnit)
        {
            if (Unit.AreSameDimension(newUnit, InternalUnit))
            {
                var convertedValue = Unit.Convert(newUnit, InternalUnit, value);
                ValueInSI = convertedValue;
                IsFixed = true;
            }
            else
                throw new Exception("Dimensions of assignment do not fit." + newUnit + " " + InternalUnit);
        }

        public double GetValue()
        {
            var convertedValue = Unit.Convert(InternalUnit, OutputUnit, Eval(new Evaluator()));
            if (Math.Abs(convertedValue) < 1e-10)
                return 0;
            return convertedValue;
        }

        public double GetValue(Unit desiredUnit)
        {
            if (Unit.AreSameDimension(desiredUnit, InternalUnit))
            {
                var convertedValue = Unit.Convert(InternalUnit, desiredUnit, Eval(new Evaluator()));
                return convertedValue;
            }
            throw new Exception("Dimensions of conversion do not fit." + desiredUnit + " " + InternalUnit);
        }
    }
}
