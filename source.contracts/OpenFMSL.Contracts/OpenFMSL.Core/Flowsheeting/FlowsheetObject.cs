using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Flowsheeting
{
    public abstract class FlowsheetObject:BaseFlowsheetObject
    {
    
        string _description;
        string _class;
      
        Guid _id;
        private readonly ThermodynamicSystem _system;
        List<Variable> _variables = new List<Variable>();

       

        public Guid Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public List<Variable> Variables
        {
            get
            {
                return _variables;
            }

            set
            {
                _variables = value;
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

        public string Class
        {
            get
            {
                return _class;
            }

            set
            {
                _class = value;
            }
        }

        public ThermodynamicSystem System
        {
            get
            {
                return _system;
            }
        }

       

        public FlowsheetObject()
        {
            Id = new Guid();
            Class = "Flowsheet";
        }

        public FlowsheetObject(string name, ThermodynamicSystem system) : this()
        {
            Name = name;
            _system = system;

        }

        public virtual void FillEquationSystem(EquationSystem problem)
        {
            foreach (var vari in Variables)
            {
                vari.ModelClass = Class;
                vari.ModelName = Name;

                if (vari.IsFixed)
                {
                    if (!problem.TreatFixedVariablesAsConstants && !vari.IsConstant)
                    {
                        //if (vari.DefiningExpression == null)
                        //{
                        //    problem.AddVariables(vari);
                        //}
                        double scale = 1.0;
                        if (vari.Dimension == PhysicalDimension.Pressure)
                            scale = 1e5;
                        if (vari.Dimension == PhysicalDimension.Temperature)
                            scale = 1e3;
                        AddEquationToEquationSystem(problem, (vari / scale).IsEqualTo(vari.ValueInSI / scale), "Specification");
                        // problem.RemoveVariable(vari);
                    }
                    else
                    {
                        problem.RemoveVariable(vari);
                    }
                }

                if (vari.DefiningExpression != null)
                {
                    problem.AddDefinedVariables(vari);
                }
                else
                {
                    if (!(problem.TreatFixedVariablesAsConstants && vari.IsFixed ) && !vari.IsConstant)
                        problem.AddVariables(vari);
                }





            }
        }





        public Variable GetVariable(string name)
        {
            return Variables.FirstOrDefault(v => v.FullName == name);
        }




        public FlowsheetObject Specify(string variable, double value)
        {
            Specify(variable, value, null);
            return this;
        }
        public FlowsheetObject Init(string variable, double value)
        {
            Init(variable, value, null);
            return this;
        }
        public FlowsheetObject Init(string variable, double value, Unit unit)
        {
            var vari = GetVariable(variable);
            if (vari != null)
            {
                if (unit != null)
                    vari.SetValue(value, unit);
                else
                    vari.SetValue(value);
            }
            else
                throw new InvalidOperationException("Unknown variable " + vari + " in object " + Name);

            return this;
        }



        public FlowsheetObject Specify(string variable, double value, Unit unit)
        {
            var vari = GetVariable(variable);
            if (vari != null)
            {
                if (unit != null)
                    vari.FixValue(value, unit);
                else
                    vari.FixValue(value);
            }
            else
                throw new InvalidOperationException("Unknown variable " + vari + " in object " + Name);

            return this;
        }

        public FlowsheetObject Unspecify(string variable)
        {
            var vari = GetVariable(variable);
            if (vari != null)
            {
                vari.Unfix();
            }
            else
                throw new InvalidOperationException("Unknown variable " + vari + " in object " + Name);

            return this;
        }

        protected FlowsheetObject AddVariables(params Variable[] variables)
        {
            foreach (var vari in variables)
            {
                if (!Variables.Contains(vari))
                {
                    vari.ModelClass = Class;
                    vari.ModelName = Name;
                    Variables.Add(vari);
                }
            }
            return this;
        }

        protected FlowsheetObject AddVariable(Variable vari)
        {
            if (!Variables.Contains(vari))
            {
                vari.ModelClass = Class;
                vari.ModelName = Name;
                Variables.Add(vari);
            }
            return this;
        }



        protected void AddVariableToEquationSystem(EquationSystem system, Variable vari)
        {
            AddVariableToEquationSystem(system, vari, "Generic");
        }

        protected void AddVariableToEquationSystem(EquationSystem system, Variable vari, string group)
        {
            vari.ModelName = Name;
            vari.ModelClass = Class;
            vari.Group = group;
            system.AddVariables(vari);
        }


        protected void AddEquationToEquationSystem(EquationSystem system, Equation eq)
        {
            AddEquationToEquationSystem(system, eq, eq.Group);
        }

        protected void AddEquationToEquationSystem(EquationSystem system, Equation eq, string group)
        {
            eq.ModelName = Name;
            eq.ModelClass = Class;
            eq.Group = group;

            if (String.IsNullOrEmpty(eq.Name))
                eq.Name = "EQ" + (system.Equations.Count + 1).ToString("00000");
            system.AddConstraints(eq);
        }


    }
}
