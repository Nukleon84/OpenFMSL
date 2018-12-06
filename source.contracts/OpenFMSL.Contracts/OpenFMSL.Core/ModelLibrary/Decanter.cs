using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ModelLibrary
{
    public class Decanter : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable Q;
        private Variable VF;
        private Variable[] KLL;
        private Variable[] S;

        public Decanter(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Decanter";
            Icon.IconType = IconTypes.ThreePhaseFlash;

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Vap", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Liq1", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Liq2", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in flash", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in flash", PhysicalDimension.Temperature);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            VF = system.VariableFactory.CreateVariable("VF", "Vapor Fraction", PhysicalDimension.MolarFraction);

            KLL = new Variable[system.Components.Count];
            S = new Variable[system.Components.Count];

            for (int i = 0; i < system.Components.Count; i++)
            {
                KLL[i] = system.VariableFactory.CreateVariable("KLL", "Equilibrium partition coefficient (LLE)", PhysicalDimension.Dimensionless);
                KLL[i].Subscript = system.Components[i].ID;
                KLL[i].ValueInSI = 1.2;
                KLL[i].UpperBound = 1e6;

                S[i] = system.VariableFactory.CreateVariable("S", "Split fraction between Liquid Phases)", PhysicalDimension.Dimensionless);
                S[i].Subscript = system.Components[i].ID;
                S[i].ValueInSI = 0.5;
                S[i].UpperBound = 1;


            }
            dp.LowerBound = -1e10;
            dp.ValueInSI = 0;

            AddVariable(p);
            AddVariable(T);
            AddVariable(Q);
            AddVariable(VF);
            AddVariable(dp);
            AddVariables(KLL);
            AddVariables(S);

        }

        public override void FillEquationSystem(EquationSystem problem)
        {

            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Vap = FindMaterialPort("Vap");
            var Liq1 = FindMaterialPort("Liq1");
            var Liq2 = FindMaterialPort("Liq2");

            if (!Liq1.IsConnected || !Liq2.IsConnected)
                throw new InvalidOperationException("Decanter requires Liq1 and Liq2 port to be connected");


            //Vap.Streams[0].State = PhaseState.DewPoint;
            //Liq1.Streams[0].State = PhaseState.BubblePoint;
            //Liq2.Streams[0].State = PhaseState.BubblePoint;

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                if (Vap.IsConnected)
                    AddEquationToEquationSystem(problem, (In.Streams[0].Mixed.ComponentMolarflow[cindex]).IsEqualTo(Vap.Streams[0].Mixed.ComponentMolarflow[cindex] + Liq1.Streams[0].Mixed.ComponentMolarflow[cindex] + Liq2.Streams[0].Mixed.ComponentMolarflow[cindex]), "Mass Balance");
                else
                    AddEquationToEquationSystem(problem, (In.Streams[0].Mixed.ComponentMolarflow[cindex]).IsEqualTo(Liq1.Streams[0].Mixed.ComponentMolarflow[cindex] + Liq2.Streams[0].Mixed.ComponentMolarflow[cindex]), "Mass Balance");


                AddEquationToEquationSystem(problem, (Liq1.Streams[0].Mixed.ComponentMolarflow[cindex]).IsEqualTo(S[cindex] * In.Streams[0].Mixed.ComponentMolarflow[cindex]), "Mass Balance");
            }


            AddEquationToEquationSystem(problem, (p / 1e4).IsEqualTo(Sym.Par(In.Streams[0].Mixed.Pressure - dp) / 1e4), "Pressure drop");

            if (Vap.IsConnected)
            {
                AddEquationToEquationSystem(problem, (Vap.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");
                AddEquationToEquationSystem(problem, (Vap.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Temperature Balance");
            }
            AddEquationToEquationSystem(problem, (Liq1.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Liq1.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Temperature Balance");
            AddEquationToEquationSystem(problem, (Liq2.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Liq2.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Temperature Balance");

            if (Vap.IsConnected)
                AddEquationToEquationSystem(problem, (In.Streams[0].Mixed.TotalMolarflow * VF).IsEqualTo(Vap.Streams[0].Mixed.TotalMolarflow), "Mass Balance Vapor Fraction");
            else
                AddEquationToEquationSystem(problem, (VF).IsEqualTo(0), "No Vapor Outlet connected");

            if (Vap.IsConnected)
                AddEquationToEquationSystem(problem, ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow + Q) / 1e4))
                   .IsEqualTo(Sym.Par(Vap.Streams[0].Mixed.SpecificEnthalpy * Vap.Streams[0].Mixed.TotalMolarflow + Liq1.Streams[0].Mixed.SpecificEnthalpy * Liq1.Streams[0].Mixed.TotalMolarflow + Liq2.Streams[0].Mixed.SpecificEnthalpy * Liq2.Streams[0].Mixed.TotalMolarflow) / 1e4), "Heat Balance");
            else
                AddEquationToEquationSystem(problem, ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow + Q) / 1e4))
                .IsEqualTo(Sym.Par(Liq1.Streams[0].Mixed.SpecificEnthalpy * Liq1.Streams[0].Mixed.TotalMolarflow + Liq2.Streams[0].Mixed.SpecificEnthalpy * Liq2.Streams[0].Mixed.TotalMolarflow) / 1e4), "Heat Balance");


            for (int i = 0; i < NC; i++)
            {
                if (Vap.IsConnected)
                {

                }

                System.EquationFactory.EquilibriumCoefficientLLE(System, KLL[i], T, p, Liq1.Streams[0].Mixed.ComponentMolarFraction, Liq2.Streams[0].Mixed.ComponentMolarFraction, i);
                AddEquationToEquationSystem(problem, Liq1.Streams[0].Mixed.ComponentMolarFraction[i].IsEqualTo(KLL[i] * Liq2.Streams[0].Mixed.ComponentMolarFraction[i]), "Equilibrium");
            }



            base.FillEquationSystem(problem);

        }


        public override ProcessUnit Initialize()
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Vap = FindMaterialPort("Vap");
            var Liq1 = FindMaterialPort("Liq1");
            var Liq2 = FindMaterialPort("Liq2");

            if (!Liq1.IsConnected || !Liq2.IsConnected)
                throw new InvalidOperationException("Decanter requires Liq1 and Liq2 port to be connected");

            var L1 = Liq1.Streams[0];
            var L2 = Liq2.Streams[0];

            if (p.IsFixed)
            {
                L1.Init("p", p.ValueInSI);
                L2.Init("p", p.ValueInSI);
            }
            else if (dp.IsFixed)
            {
                L1.Init("p", In.Streams[0].Mixed.Pressure.ValueInSI - dp.ValueInSI);
                L2.Init("p", In.Streams[0].Mixed.Pressure.ValueInSI - dp.ValueInSI);
            }

            if (T.IsFixed)
            {
                L1.Init("T", T.ValueInSI);
                L2.Init("T", T.ValueInSI);
            }
            else
            {
                L1.Init("T", In.Streams[0].Mixed.Temperature.ValueInSI);
                L2.Init("T", In.Streams[0].Mixed.Temperature.ValueInSI);
            }

            for (int i = 0; i < NC; i++)
            {
                L1.Mixed.ComponentMolarflow[i].ValueInSI = S[i].ValueInSI * In.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI;
                L2.Mixed.ComponentMolarflow[i].ValueInSI = (1 - S[i].ValueInSI) * In.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI;
            }

            L1.FlashPT();
            L2.FlashPT();

            return this;
        }
    }
}
