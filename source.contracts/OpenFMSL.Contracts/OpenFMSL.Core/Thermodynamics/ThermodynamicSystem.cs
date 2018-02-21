using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public class ThermodynamicSystem
    {
        string _name;
        List<MolecularComponent> _components = new List<MolecularComponent>();
        CorrelationFactory _correlationFactory = new CorrelationFactory();
        VariableFactory _variableFactory = new VariableFactory();
        EquilibriumCalculationMethod _equilibriumMethod = new EquilibriumCalculationMethod();
        EnthalpyCalculationMethod _enthalpyMethod = new EnthalpyCalculationMethod();
        PropertyFunctionFactory _equationFactory = new PropertyFunctionFactory();

        List<BinaryInteractionParameterSet> _binaryParameters = new List<BinaryInteractionParameterSet>();
        #region Properties
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

        public List<MolecularComponent> Components
        {
            get
            {
                return _components;
            }

            set
            {
                _components = value;
            }
        }

        public CorrelationFactory CorrelationFactory
        {
            get
            {
                return _correlationFactory;
            }

            set
            {
                _correlationFactory = value;
            }
        }

        public VariableFactory VariableFactory
        {
            get
            {
                return _variableFactory;
            }

            set
            {
                _variableFactory = value;
            }
        }

        public EquilibriumCalculationMethod EquilibriumMethod
        {
            get
            {
                return _equilibriumMethod;
            }

            set
            {
                _equilibriumMethod = value;
            }
        }

        public EnthalpyCalculationMethod EnthalpyMethod
        {
            get
            {
                return _enthalpyMethod;
            }

            set
            {
                _enthalpyMethod = value;
            }
        }

        public PropertyFunctionFactory EquationFactory
        {
            get
            {
                return _equationFactory;
            }

            set
            {
                _equationFactory = value;
            }
        }

        public List<BinaryInteractionParameterSet> BinaryParameters
        {
            get
            {
                return _binaryParameters;
            }

            set
            {
                _binaryParameters = value;
            }
        }

        #endregion

    }
}
