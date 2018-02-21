using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics
{
    public class EquationSystem
    {
        #region Fields
        List<Variable> _variables = new List<Variable>();
        List<Variable> _definedVariables = new List<Variable>();
        List<Equation> _equations = new List<Equation>();
        string _name;
        bool _treatFixedVariablesAsConstants = false;
        List<JacobianElement> _jacobian = new List<JacobianElement>();
        protected Dictionary<Variable, int> _variableIndex = new Dictionary<Variable, int>();
        #endregion

        #region Properties
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

        public List<Equation> Equations
        {
            get
            {
                return _equations;
            }

            set
            {
                _equations = value;
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

        public int NumberOfVariables
        {
            get { return Variables.Count; }
        }
        public int NumberOfEquations
        {
            get { return Equations.Count; }
        }

        public List<Variable> DefinedVariables
        {
            get
            {
                return _definedVariables;
            }

            set
            {
                _definedVariables = value;
            }
        }

        public bool TreatFixedVariablesAsConstants
        {
            get
            {
                return _treatFixedVariablesAsConstants;
            }

            set
            {
                _treatFixedVariablesAsConstants = value;
            }
        }

        public List<JacobianElement> Jacobian
        {
            get
            {
                return _jacobian;
            }

            set
            {
                _jacobian = value;
            }
        }
        #endregion


        public EquationSystem()
        {

        }

        #region Public Methods

        public void CreateIndex()
        {
            _variableIndex.Clear();
            for (int i = 0; i < Variables.Count; i++)
            {
                _variableIndex.Add(Variables[i], i);
            }
        }

        public virtual void GenerateJacobian()
        {
            int i = 0;
            Jacobian.Clear();
            foreach (var equation in Equations)
            {
                foreach (var variable in equation.Incidence(new Evaluator()))
                {
                    if (!variable.IsConstant && variable.DefiningExpression == null)
                    {
                        int j = -1;
                        if (_variableIndex.TryGetValue(variable, out j))
                        {
                            Jacobian.Add(new JacobianElement() { EquationIndex = i, VariableIndex = j, Value = 1.0 });
                        }
                    }
                }
                i++;
            }
        }

        public void AddDefinedVariables(params Variable[] vars)
        {
            foreach (Variable var in vars)
            {
                DefinedVariables.Add(var);
            }
        }
        public void RemoveDefinedVariable(Variable var)
        {
            if (DefinedVariables.Contains(var))
            {
                DefinedVariables.Remove(var);
            }
        }


        public void AddVariables(params Variable[] vars)
        {
            foreach (Variable var in vars)
            {
                Variables.Add(var);
            }
        }
        public void RemoveVariable(Variable var)
        {
            if (Variables.Contains(var))
            {
                Variables.Remove(var);
            }
        }
        public void AddConstraints(params Equation[] constr)
        {
            foreach (var constraint in constr)
                Equations.Add(constraint);
        }

        public bool IsSquare()
        {
            return Variables.Count == Equations.Count;
        }
        #endregion

    }
}
