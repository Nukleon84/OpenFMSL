using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics
{
    public class OptimizationProblem : EquationSystem
    {
        Expression _objectiveFunction;
        List<Constraint> _constraints = new List<Constraint>();
        List<Variable> _decisionVariables = new List<Variable>();

        public List<Constraint> Constraints
        {
            get
            {
                return _constraints;
            }

            set
            {
                _constraints = value;
            }
        }

        public Expression ObjectiveFunction
        {
            get
            {
                return _objectiveFunction;
            }

            set
            {
                _objectiveFunction = value;
            }
        }

        public List<Variable> DecisionVariables
        {
            get
            {
                return _decisionVariables;
            }

            set
            {
                _decisionVariables = value;
            }
        }

        public void SetObjective(Expression objective)
        {
            if (objective != null)
                ObjectiveFunction = objective;
            else
                throw new ArgumentNullException("Objective function can not be NULL");
        }
        public void AddInequalityConstraints(params Constraint[] constr)
        {
            foreach (var constraint in constr)
                Constraints.Add(constraint);
        }

        public void AddDecisionVariables(params Variable[] variables)
        {
            foreach (var variable in variables)
                if (!Variables.Contains(variable))
                    Variables.Add(variable);
            foreach (var variable in variables)
                if (!DecisionVariables.Contains(variable))
                    DecisionVariables.Add(variable);
        }


        public override void GenerateJacobian()
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

            foreach (var equation in Constraints)
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
    }
}
