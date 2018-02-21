using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class Snapshot : Entity
    {
        List<Variable> _variables = new List<Variable>();
        List<Variable> _expressions = new List<Variable>();
        List<Equation> _equations = new List<Equation>();

        public Snapshot():base()
        {
            IconName = "Camera";
        }

        public Snapshot(string name, EquationSystem problem) : this()
        {
            Name = name;
            if (problem != null)
            {
                foreach (var variable in problem.Variables)
                {
                    Variables.Add(variable.Copy() as Variable);
                }
                foreach (var variable in problem.DefinedVariables)
                {
                    Expressions.Add(variable.Copy() as Variable);
                }
                foreach (var eq in problem.Equations)
                {
                    Equations.Add(eq.Copy() as Equation);
                }
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

        public List<Variable> Expressions
        {
            get
            {
                return _expressions;
            }

            set
            {
                _expressions = value;
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
    }
}
