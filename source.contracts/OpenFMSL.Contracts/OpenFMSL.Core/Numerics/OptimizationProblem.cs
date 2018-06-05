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
        List<HessianElement> _hessian = new List<HessianElement>();
        List<HessianElement> _objectiveHessian = new List<HessianElement>();
        List<HessianStructureEntry> _hessianStructure = new List<HessianStructureEntry>();

        bool _useHessian = true;

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

        public List<HessianElement> Hessian
        {
            get
            {
                return _hessian;
            }

            set
            {
                _hessian = value;
            }
        }

        public bool UseHessian
        {
            get
            {
                return _useHessian;
            }

            set
            {
                _useHessian = value;
            }
        }

        public List<HessianStructureEntry> HessianStructure
        {
            get
            {
                return _hessianStructure;
            }

            set
            {
                _hessianStructure = value;
            }
        }

        public List<HessianElement> ObjectiveHessian
        {
            get
            {
                return _objectiveHessian;
            }

            set
            {
                _objectiveHessian = value;
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
            var testEval = new Evaluator();


            foreach (var equation in Equations)
            {
                var incidenceVector = equation.Incidence(testEval);

                foreach (var variable in incidenceVector)
                {
                    if (!variable.IsConstant && variable.DefiningExpression == null)
                    {
                        int j = -1;
                        if (_variableIndex.TryGetValue(variable, out j))
                        {
                            Jacobian.Add(new JacobianElement() { EquationIndex = i, VariableIndex = j, Value = 1.0 });

                            if (UseHessian)
                            {
                                var differential = (equation.Right - equation.Left).SymbolicDiff(variable);

                                foreach (var variable2 in incidenceVector)
                                {
                                    if (!variable2.IsConstant && variable2.DefiningExpression == null)
                                    {
                                        int k = -1;
                                        if (_variableIndex.TryGetValue(variable2, out k) && k <= j)
                                        {
                                            Hessian.Add(new HessianElement() { EquationIndex = i, Variable1Index = j, Variable2Index = k, Expression = differential, Value = 1.0 });
                                        }
                                    }
                                }
                            }


                        }
                    }
                }
                i++;
            }



            if (UseHessian)
            {
                var incidenceVector = ObjectiveFunction.Incidence();

                foreach (var variable1 in incidenceVector)
                {
                    if (!variable1.IsConstant && variable1.DefiningExpression == null)
                    {
                        int j = -1;
                        if (_variableIndex.TryGetValue(variable1, out j))
                        {
                            var differential = ObjectiveFunction.SymbolicDiff(variable1);
                            foreach (var variable2 in incidenceVector)
                            {
                                if (!variable2.IsConstant && variable2.DefiningExpression == null)
                                {
                                    int k = -1;
                                    if (_variableIndex.TryGetValue(variable2, out k) && k <= j)
                                    {
                                        ObjectiveHessian.Add(new HessianElement() { EquationIndex = 0, Variable1Index = j, Variable2Index = k, Expression = differential, Value = 1.0 });
                                    }
                                }
                            }
                        }
                    }
                }
            }

                        
            foreach (var equation in Constraints)
            {
                var incidenceVector = equation.Incidence(testEval);
                foreach (var variable in incidenceVector)
                {
                    if (!variable.IsConstant && variable.DefiningExpression == null)
                    {
                        int j = -1;
                        if (_variableIndex.TryGetValue(variable, out j))
                        {
                            Jacobian.Add(new JacobianElement() { EquationIndex = i, VariableIndex = j, Value = 1.0 });

                            if (UseHessian)
                            {
                                var differential = (equation.Right- equation.Left).SymbolicDiff(variable);

                                foreach (var variable2 in incidenceVector)
                                {
                                    if (!variable2.IsConstant && variable2.DefiningExpression == null)
                                    {
                                        int k = -1;
                                        if (_variableIndex.TryGetValue(variable2, out k) && k <= j)
                                        {
                                            Hessian.Add(new HessianElement() { EquationIndex = i, Variable1Index = j, Variable2Index = k, Expression = differential, Value = 1.0 });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                i++;
            }

            if (UseHessian)
                GenerateHessianStructureInfo();
        }

        private void GenerateHessianStructureInfo()
        {           

            var hessianStructureLookUp = new Dictionary<long, int>();

            foreach (var entry in ObjectiveHessian)
            {
                long key = (entry.Variable1Index + Constraints.Count) * (Variables.Count + 1) + entry.Variable2Index;

                bool alreadyDefined = hessianStructureLookUp.ContainsKey(key);
                if (!alreadyDefined)
                {
                    entry.StructuralIndex = HessianStructure.Count;
                    HessianStructure.Add(new HessianStructureEntry { Var1 = entry.Variable1Index, Var2 = entry.Variable2Index });
                    hessianStructureLookUp.Add(key, entry.StructuralIndex);
                }
                else
                {
                    int index = hessianStructureLookUp[key];
                    entry.StructuralIndex = index;
                }
            }

            foreach (var entry in Hessian)
            {
              //  int key = 23;
            //    key = (key * 37) + entry.Variable1Index;
          //      key = (key * 37) + entry.Variable2Index;
                long key = (entry.Variable1Index + Constraints.Count) * (Variables.Count + 1) + entry.Variable2Index;

                bool alreadyDefined = hessianStructureLookUp.ContainsKey(key);
                if (!alreadyDefined)
                {
                    entry.StructuralIndex = HessianStructure.Count;
                    HessianStructure.Add(new HessianStructureEntry { Var1 = entry.Variable1Index, Var2 = entry.Variable2Index });
                    hessianStructureLookUp.Add(key, entry.StructuralIndex);
                }
                else
                {
                    int index = hessianStructureLookUp[key];
                    entry.StructuralIndex = index;
                }
            }            

        }
    }
}
