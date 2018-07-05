using OpenFMSL.Core.ThermodynamicModels;
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

        List<Chemistry> _chemistryBlocks = new List<Chemistry>();

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



        public List<Chemistry> ChemistryBlocks
        {
            get
            {
                return _chemistryBlocks;
            }

            set
            {
                _chemistryBlocks = value;
            }
        }

        #endregion

        public ThermodynamicSystem()
        {

        }
        public ThermodynamicSystem(string name, string baseMethod="Ideal", string uomset="default")
        {
            Name = name;
            MakeDefault(baseMethod);

            if (uomset.ToLower() == "default")
                VariableFactory.SetOutputDimensions(UnitsOfMeasure.UnitSet.CreateDefault());
            if (uomset == "SI")
                VariableFactory.SetOutputDimensions(UnitsOfMeasure.UnitSet.CreateSI());

        }

        public ThermodynamicSystem AddComponent(MolecularComponent comp)
        {
            Components.Add(comp);
            var enthalpy = PureEnthalpyFunction.Create(this, comp);
            enthalpy.ReferenceState = PhaseState.Vapour;
            enthalpy.Tref.ValueInSI = 298.15;
            EnthalpyMethod.PureComponentEnthalpies.Add(enthalpy);

            return this;
        }
        public BinaryInteractionParameterSet GetBinaryParameters(string name)
        {
            return BinaryParameters.FirstOrDefault(s => s.Name == name);
        }
        public void MakeDefault(string baseMethod)
        {
            switch (baseMethod)
            {

                case "SRK":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.PhiPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.SoaveRedlichKwong;
                    EquilibriumMethod.EquationOfState = EquationOfState.SoaveRedlichKwong;
                    EquilibriumMethod.Activity = ActivityMethod.Ideal;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                case "NRTL":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.NRTL;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                case "NRTLRP":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.NRTLRP;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                case "UNIQUAC":
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.UNIQUAC;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
                default:
                    EquilibriumMethod.EquilibriumApproach = EquilibriumApproach.GammaPhi;
                    EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    EquilibriumMethod.Activity = ActivityMethod.Ideal;
                    EquilibriumMethod.AllowHenryComponents = false;
                    EquilibriumMethod.PoyntingCorrection = false;
                    EquilibriumMethod.AllowedPhases = AllowedPhases.VLE;
                    break;
            }
        }
    }
}
