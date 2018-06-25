using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.UnitsOfMeasure;

namespace OpenFMSL.Core.Flowsheeting
{
    public class MaterialStream : Stream
    {
        Phase _liquid;
        Phase _vapor;
        Phase _mixed;
        Phase _solid;
        Phase _liquid2;
        PhaseState _state = PhaseState.LiquidVapor;
        Variable vfmolar;
        Variable vfmass;
        Variable Beta;
        Variable MW;

        Variable MinVfBeta;
        Variable MaxVfBeta;

        Variable[] K;
        Variable[] gamma;

        #region Properties
        public Phase Liquid
        {
            get
            {
                return _liquid;
            }

            set
            {
                _liquid = value;
            }
        }

        public Phase Vapor
        {
            get
            {
                return _vapor;
            }

            set
            {
                _vapor = value;
            }
        }

        public Phase Mixed
        {
            get
            {
                return _mixed;
            }

            set
            {
                _mixed = value;
            }
        }

        public Phase Solid
        {
            get
            {
                return _solid;
            }

            set
            {
                _solid = value;
            }
        }

        public Phase Liquid2
        {
            get
            {
                return _liquid2;
            }

            set
            {
                _liquid2 = value;
            }
        }

        public PhaseState State
        {
            get
            {
                return _state;
            }

            set
            {
                _state = value;
            }
        }

        public Variable Vfmolar
        {
            get
            {
                return vfmolar;
            }

            set
            {
                vfmolar = value;
            }
        }

        public Variable Vfmass
        {
            get
            {
                return vfmass;
            }

            set
            {
                vfmass = value;
            }
        }

        public Variable[] KValues
        {
            get
            {
                return K;
            }

            set
            {
                K = value;
            }
        }
        #endregion

        public MaterialStream(string name, ThermodynamicSystem system) : base(name, system)
        {

            Class = "MaterialStream";
            if (system.EquilibriumMethod.AllowedPhases == AllowedPhases.VLE)
            {
                Mixed = new Phase("", system);
                Liquid = new Phase("L", system);
                Vapor = new Phase("V", system);

                Vfmolar = system.VariableFactory.CreateVariable("VF", "Mole-based vapor fraction", PhysicalDimension.MolarFraction);
                Vfmolar.Group = "Equilibrium";
                Beta = new Variable("d", 0.5, -10, 10, SI.nil, "Phase equilibrium defect");
                Beta.Group = "Equilibrium";

                MinVfBeta = new Variable("d2", 0.1, -100, 100, SI.nil, "min(Vapor fraction,Beta)");
                MinVfBeta.Group = "Equilibrium";
                MaxVfBeta = new Variable("d3", 0.1, -100, 100, SI.nil, "max(Vapor fraction, beta)");
                MaxVfBeta.Group = "Equilibrium";
                AddVariable(MinVfBeta);
                AddVariable(MaxVfBeta);

                MW = system.VariableFactory.CreateVariable("MW", "Average molar weight (bulk)", PhysicalDimension.MolarWeight);                              

                gamma = new Variable[System.Components.Count];

                KValues = new Variable[System.Components.Count];
                for (int i = 0; i < System.Components.Count; i++)
                {
                    KValues[i] = system.VariableFactory.CreateVariable("K", "Equilibrium distribution coefficient", PhysicalDimension.Dimensionless);
                    KValues[i].Subscript = System.Components[i].ID;
                    KValues[i].Group = "Equilibrium";
                    KValues[i].ValueInSI = 1.4;
                    System.EquationFactory.EquilibriumCoefficient(System, KValues[i], Mixed.Temperature, Mixed.Pressure, Liquid.ComponentMolarFraction, Vapor.ComponentMolarFraction, i);


                    gamma[i] = system.VariableFactory.CreateVariable("gamma", "Activity coefficient", PhysicalDimension.Dimensionless);
                    gamma[i].Subscript = System.Components[i].ID;
                    gamma[i].Group = "Equilibrium";
                    gamma[i].ValueInSI = 1.4;
                    System.EquationFactory.ActivityCoefficient(System, gamma[i], Mixed.Temperature, Liquid.ComponentMolarFraction, i);

                }
                AddVariable(MW);
                AddVariable(Vfmolar);
                AddVariable(Beta);
                foreach (var vari in Mixed.Variables)
                    vari.Group = "Mixed";
                foreach (var vari in Liquid.Variables)
                    vari.Group = "Liquid";
                foreach (var vari in Vapor.Variables)
                    vari.Group = "Vapor";

                Variables.AddRange(Mixed.Variables);
                Variables.AddRange(Liquid.Variables);
                Variables.AddRange(Vapor.Variables);
                Variables.AddRange(KValues);
                Variables.AddRange(gamma);

                Variables.Remove(Liquid.Temperature);
                Variables.Remove(Vapor.Temperature);
                Variables.Remove(Liquid.Pressure);
                Variables.Remove(Vapor.Pressure);

                for (int i = 0; i < System.Components.Count; i++)
                {
                    Variables.Remove(Mixed.ComponentMolarVolume[i]);
                    Variables.Remove(Vapor.ComponentMolarVolume[i]);
                    Variables.Remove(Mixed.ComponentEnthalpy[i]);

                }
            }
            else
                throw new NotSupportedException("Only VLE supported");
        }
        public MaterialStream FixBubblePoint()
        {
            State = PhaseState.BubblePoint;
            return this;
        }
        public MaterialStream FixDewPoint()
        {
            State = PhaseState.DewPoint;
            return this;
        }

        public MaterialStream Unfix()
        {
            foreach (var variable in Variables)
                variable.IsFixed = false;

            return this;
        }

        public MaterialStream InitMolarFlowFromFractions()
        {
            var total = Mixed.TotalMolarflow.ValueInSI;
            for (var i = 0; i < System.Components.Count; i++)
            {
                Mixed.ComponentMolarflow[i].ValueInSI = total * Mixed.ComponentMolarFraction[i].ValueInSI;
            }
            return this;
        }
        public MaterialStream FixMolarFlows()
        {
            for (var i = 0; i < System.Components.Count; i++)
            {
                Mixed.ComponentMolarflow[i].IsFixed = true;
            }
            return this;
        }

        public MaterialStream InitMolarFlowFromMassFlows()
        {            
            for (var i = 0; i < System.Components.Count; i++)
            {
                Mixed.ComponentMolarflow[i].ValueInSI = Mixed.ComponentMassflow[i].ValueInSI / System.Components[i].MolarWeight.ValueInSI;
            }
            return this;
        }

        public MaterialStream FlashPT()
        {
            FlashRoutines calc = new FlashRoutines(new Numerics.Solvers.Newton());
            calc.CalculateTP(this);
            return this;
        }
        public MaterialStream FlashPZ()
        {
            FlashRoutines calc = new FlashRoutines(new Numerics.Solvers.Newton());
            calc.CalculateZP(this);
            return this;
        }

        public MaterialStream CopyFrom(MaterialStream original)
        {
            Mixed.Temperature.ValueInSI = original.Mixed.Temperature.ValueInSI;
            Mixed.Pressure.ValueInSI = original.Mixed.Pressure.ValueInSI;

            for (int i = 0; i < System.Components.Count; i++)
            {
                Mixed.ComponentMolarflow[i].ValueInSI = original.Mixed.ComponentMolarflow[i].ValueInSI;
            }
            return this;

        }
        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;

            var vliq = Sym.Sum(0, NC, i => Liquid.ComponentMolarflow[i] * Liquid.ComponentMolarVolume[i]);
            var vvap = Vapor.TotalMolarflow / Vapor.DensityMolar;

            MW.BindTo(System.EquationFactory.GetAverageMolarWeightExpression(System, Mixed.ComponentMolarFraction.ToArray()));
            Liquid.TotalVolumeflow.BindTo(vliq);
            Vapor.TotalVolumeflow.BindTo(vvap);
            Mixed.TotalVolumeflow.BindTo(vliq + vvap);

            Mixed.TotalMassflow.BindTo(Sym.Sum(Mixed.ComponentMassflow));
            Liquid.TotalMassflow.BindTo(Sym.Sum(Liquid.ComponentMassflow));
            Vapor.TotalMassflow.BindTo(Sym.Sum(Vapor.ComponentMassflow));

            Liquid.SpecificEnthalpy.BindTo(Sym.Par(Sym.Sum(0, NC, (idx) => Liquid.ComponentMolarFraction[idx] * Liquid.ComponentEnthalpy[idx])));
            Vapor.SpecificEnthalpy.BindTo(Sym.Par(Sym.Sum(0, NC, (idx) => Vapor.ComponentMolarFraction[idx] * Vapor.ComponentEnthalpy[idx])));

            Mixed.Density.BindTo(Mixed.TotalMassflow / Mixed.TotalVolumeflow);
            Liquid.Density.BindTo(Liquid.TotalMassflow / Liquid.TotalVolumeflow);
            Vapor.Density.BindTo(Vapor.TotalMassflow / Vapor.TotalVolumeflow);

            Mixed.DensityMolar.BindTo(Mixed.TotalMolarflow / Mixed.TotalVolumeflow);
            Liquid.DensityMolar.BindTo(Liquid.TotalMolarflow / Liquid.TotalVolumeflow);
            Vapor.DensityMolar.BindTo(System.EquationFactory.GetAverageVaporDensityExpression(System,Mixed.Temperature,Mixed.Pressure, Vapor.ComponentMolarFraction));


            for (int i = 0; i < NC; i++)
            {
                Mixed.ComponentMassflow[i].BindTo(Mixed.ComponentMolarflow[i] * Sym.Convert(System.Components[i].GetConstant(ConstantProperties.MolarWeight), SI.kg / SI.mol));
                Mixed.ComponentMassFraction[i].BindTo(Mixed.ComponentMassflow[i] / Sym.Max(1e-12, Mixed.TotalMassflow));

                Liquid.ComponentMassflow[i].BindTo(Liquid.ComponentMolarflow[i] * Sym.Convert(System.Components[i].GetConstant(ConstantProperties.MolarWeight), SI.kg / SI.mol));
                Liquid.ComponentMassFraction[i].BindTo(Liquid.ComponentMassflow[i] / Sym.Max(1e-12, Liquid.TotalMassflow));

                Liquid.ComponentMolarVolume[i].BindTo((1.0 / System.EquationFactory.GetLiquidDensityExpression(System, System.Components[i], Mixed.Temperature, Mixed.Pressure)));
                Liquid.ComponentEnthalpy[i].BindTo(System.EquationFactory.GetLiquidEnthalpyExpression(System, i, Mixed.Temperature));

                Vapor.ComponentMassflow[i].BindTo(Vapor.ComponentMolarflow[i] * Sym.Convert(System.Components[i].GetConstant(ConstantProperties.MolarWeight), SI.kg / SI.mol));
                Vapor.ComponentMassFraction[i].BindTo(Vapor.ComponentMassflow[i] / Sym.Max(1e-12, Vapor.TotalMassflow));

                
                Vapor.ComponentEnthalpy[i].BindTo(System.EquationFactory.GetVaporEnthalpyExpression(System, i, Mixed.Temperature));
            }

            Mixed.TotalEnthalpy.BindTo(((Mixed.TotalMolarflow * Mixed.SpecificEnthalpy)));
            Liquid.TotalEnthalpy.BindTo(((Liquid.TotalMolarflow * Liquid.SpecificEnthalpy)));
            Vapor.TotalEnthalpy.BindTo(((Vapor.TotalMolarflow * Vapor.SpecificEnthalpy)));


            if (!Mixed.SpecificEnthalpy.IsFixed)
                Mixed.SpecificEnthalpy.BindTo((Sym.Par(Vapor.TotalMolarflow * Vapor.SpecificEnthalpy + Liquid.TotalMolarflow * Liquid.SpecificEnthalpy) / Mixed.TotalMolarflow));
            else
                AddEquationToEquationSystem(problem, (Mixed.SpecificEnthalpy * Mixed.TotalMolarflow).IsEqualTo(Vapor.TotalMolarflow * Vapor.SpecificEnthalpy + Liquid.TotalMolarflow * Liquid.SpecificEnthalpy), "Enthalpy Balance");


            AddEquationToEquationSystem(problem, Mixed.TotalMolarflow.IsEqualTo(Sym.Sum(Mixed.ComponentMolarflow)), "Mass Balance");

            for (int i = 0; i < NC; i++)
            {
                // AddEquationToEquationSystem(problem, (Mixed.ComponentMolarFraction[i] * Mixed.TotalMolarflow).IsEqualTo(Liquid.ComponentMolarFraction[i] * Liquid.TotalMolarflow + Vapor.ComponentMolarFraction[i] * Vapor.TotalMolarflow));
                AddEquationToEquationSystem(problem, (Mixed.ComponentMolarflow[i]).IsEqualTo(Liquid.ComponentMolarflow[i] + Vapor.ComponentMolarflow[i]));
                // Mixed.ComponentMolarflow[i].BindTo(Liquid.ComponentMolarflow[i] + Vapor.ComponentMolarflow[i]);
            }
            for (int i = 0; i < NC; i++)
            {
                AddEquationToEquationSystem(problem, (Mixed.ComponentMolarFraction[i] * Mixed.TotalMolarflow).IsEqualTo(Mixed.ComponentMolarflow[i]));
                //Mixed.ComponentMolarFraction[i].BindTo(Mixed.ComponentMolarflow[i]/ Mixed.TotalMolarflow);
            }
            for (int i = 0; i < NC; i++)
            {
                AddEquationToEquationSystem(problem, (Liquid.ComponentMolarFraction[i] * Liquid.TotalMolarflow).IsEqualTo(Liquid.ComponentMolarflow[i]));
                //Liquid.ComponentMolarFraction[i].BindTo(Liquid.ComponentMolarflow[i] / Sym.Max(1e-12, Liquid.TotalMolarflow));
            }
            for (int i = 0; i < NC; i++)
            {
                AddEquationToEquationSystem(problem, (Vapor.ComponentMolarFraction[i] * Vapor.TotalMolarflow).IsEqualTo(Vapor.ComponentMolarflow[i]));
                // Vapor.ComponentMolarFraction[i].BindTo(Vapor.ComponentMolarflow[i] / Sym.Max(1e-12, Vapor.TotalMolarflow));
            }

            AddEquationToEquationSystem(problem, (Vfmolar * Mixed.TotalMolarflow).IsEqualTo((Vapor.TotalMolarflow)), "Vapor Fraction");

            switch (State)
            {
                case PhaseState.BubblePoint:
                    AddEquationToEquationSystem(problem, (Sym.Sum(Liquid.ComponentMolarFraction) - Sym.Sum(Vapor.ComponentMolarFraction)).IsEqualTo(Beta), "Equilibrium");
                    AddEquationToEquationSystem(problem, Beta.IsEqualTo(0), "Equilibrium");
                    AddEquationToEquationSystem(problem, Liquid.TotalMolarflow.IsEqualTo(Sym.Sum(Liquid.ComponentMolarflow)), "Mass Balance");
                    AddEquationToEquationSystem(problem, (MinVfBeta).IsEqualTo(0), "Equilibrium");
                    AddEquationToEquationSystem(problem, (MaxVfBeta).IsEqualTo(0), "Equilibrium");
                    break;
                case PhaseState.DewPoint:
                    AddEquationToEquationSystem(problem, (Sym.Sum(Liquid.ComponentMolarFraction) - Sym.Sum(Vapor.ComponentMolarFraction)).IsEqualTo(Beta), "Equilibrium");
                    AddEquationToEquationSystem(problem, Beta.IsEqualTo(0), "Equilibrium");
                    AddEquationToEquationSystem(problem, Vapor.TotalMolarflow.IsEqualTo(Sym.Sum(Vapor.ComponentMolarflow)), "Mass Balance");
                    AddEquationToEquationSystem(problem, (MinVfBeta).IsEqualTo(0), "Equilibrium");
                    AddEquationToEquationSystem(problem, (MaxVfBeta).IsEqualTo(0), "Equilibrium");
                    break;
                default:
                    if (Vfmolar.IsFixed)
                    {
                        AddEquationToEquationSystem(problem, (Sym.Sum(Liquid.ComponentMolarFraction) - Sym.Sum(Vapor.ComponentMolarFraction)).IsEqualTo(Beta), "Equilibrium");
                        AddEquationToEquationSystem(problem, Beta.IsEqualTo(0), "Equilibrium");

                        AddEquationToEquationSystem(problem, (MinVfBeta).IsEqualTo(0), "Equilibrium");
                        AddEquationToEquationSystem(problem, (MaxVfBeta).IsEqualTo(0), "Equilibrium");


                    }
                    else
                    {
                        AddEquationToEquationSystem(problem, (Sym.Sum(Liquid.ComponentMolarFraction) - Sym.Sum(Vapor.ComponentMolarFraction)).IsEqualTo(Beta), "Equilibrium");

                        AddEquationToEquationSystem(problem, (Sym.Min(Vfmolar, Beta)).IsEqualTo(MinVfBeta), "Equilibrium");
                        AddEquationToEquationSystem(problem,( Sym.Max(Vfmolar, Beta)).IsEqualTo(MaxVfBeta), "Equilibrium");

                        //AddEquationToEquationSystem(problem, Sym.Max(Sym.Min(Vfmolar, Beta), Sym.Min(Sym.Max(Vfmolar, Beta), Vfmolar - 1)).IsEqualTo(0), "Equilibrium");
                        AddEquationToEquationSystem(problem, Sym.Max(MinVfBeta, Sym.Min(MaxVfBeta, Vfmolar - 1)).IsEqualTo(0), "Equilibrium");
                    }
                    AddEquationToEquationSystem(problem, (Mixed.TotalMolarflow).IsEqualTo(Vapor.TotalMolarflow + Liquid.TotalMolarflow), "Mass Balance");
                    break;
            }

            for (int i = 0; i < NC; i++)
            {
                System.EquationFactory.EquilibriumCoefficient(System, KValues[i], Mixed.Temperature, Mixed.Pressure, Liquid.ComponentMolarFraction, Vapor.ComponentMolarFraction, i);
                AddEquationToEquationSystem(problem, Vapor.ComponentMolarFraction[i].IsEqualTo(KValues[i] * Liquid.ComponentMolarFraction[i]), "Equilibrium");         
            }

            base.FillEquationSystem(problem);

        }
    }
}
