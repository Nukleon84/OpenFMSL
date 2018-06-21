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
    public class FeedStage : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable Q;

        private Variable[] K;
        public FeedStage(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Flash";
            Icon.IconType = IconTypes.FeedStage;

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("VIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("LIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("VOut", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("LOut", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in flash", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in flash", PhysicalDimension.Temperature);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);

            K = new Variable[system.Components.Count];
            for (int i = 0; i < system.Components.Count; i++)
            {
                K[i] = system.VariableFactory.CreateVariable("K", "Equilibrium partition coefficient", PhysicalDimension.Dimensionless);
                K[i].Subscript = system.Components[i].ID;
                K[i].ValueInSI = 1.2;
            }
            dp.LowerBound = -1e10;
            dp.ValueInSI = 0;

            AddVariable(p);
            AddVariable(T);
            AddVariable(Q);

            AddVariable(dp);
            AddVariables(K);
        }

        public override void FillEquationSystem(EquationSystem problem)
        {

            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var VIN = FindMaterialPort("VIn");
            var LIN = FindMaterialPort("LIn");

            var Vap = FindMaterialPort("VOut");
            var Liq = FindMaterialPort("LOut");

            Vap.Streams[0].State = PhaseState.DewPoint;
            Liq.Streams[0].State = PhaseState.BubblePoint;

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                   (In.Streams[0].Mixed.ComponentMolarflow[cindex] + VIN.Streams[0].Mixed.ComponentMolarflow[cindex] + LIN.Streams[0].Mixed.ComponentMolarflow[cindex])
                        .IsEqualTo(Vap.Streams[0].Mixed.ComponentMolarflow[cindex] + Liq.Streams[0].Mixed.ComponentMolarflow[cindex]), "Mass Balance");

            }

            AddEquationToEquationSystem(problem, (p / 1e4).IsEqualTo(Sym.Par(Sym.Min(In.Streams[0].Mixed.Pressure, VIN.Streams[0].Mixed.Pressure) - dp) / 1e4), "Pressure drop");

            AddEquationToEquationSystem(problem, (Vap.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Vap.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Temperature Balance");
            AddEquationToEquationSystem(problem, (Liq.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Liq.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Temperature Balance");


            AddEquationToEquationSystem(problem,
    ((In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow + LIN.Streams[0].Mixed.SpecificEnthalpy * LIN.Streams[0].Mixed.TotalMolarflow + VIN.Streams[0].Mixed.SpecificEnthalpy * VIN.Streams[0].Mixed.TotalMolarflow + Q) / 1e4)
    .IsEqualTo(Sym.Par(Vap.Streams[0].Mixed.SpecificEnthalpy * Vap.Streams[0].Mixed.TotalMolarflow + Liq.Streams[0].Mixed.SpecificEnthalpy * Liq.Streams[0].Mixed.TotalMolarflow) / 1e4), "Heat Balance");


            for (int i = 0; i < NC; i++)
            {
                System.EquationFactory.EquilibriumCoefficient(System, K[i], T, p, Liq.Streams[0].Mixed.ComponentMolarFraction, Vap.Streams[0].Mixed.ComponentMolarFraction, i);
                AddEquationToEquationSystem(problem, Vap.Streams[0].Mixed.ComponentMolarFraction[i].IsEqualTo(K[i] * Liq.Streams[0].Mixed.ComponentMolarFraction[i]), "Equilibrium");
            }



            base.FillEquationSystem(problem);

        }


        public override ProcessUnit Initialize()
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Vap = FindMaterialPort("VOut");
            var Liq = FindMaterialPort("LOut");

            var VIN = FindMaterialPort("VIn");
            var LIN = FindMaterialPort("LIn");

            var flash = new FlashRoutines(new Numerics.Solvers.Newton());

            var flashStream = new MaterialStream("FLASH", System);
            flashStream.CopyFrom(In.Streams[0]);
            for (int i = 0; i < NC; i++)
            {
                flashStream.Mixed.ComponentMolarflow[i].ValueInSI += LIN.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI + VIN.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI;
            }

            if (p.IsFixed)
                flashStream.Specify("p", p.ValueInSI);
            else if (dp.IsFixed)
                flashStream.Specify("p", In.Streams[0].Mixed.Pressure.ValueInSI - dp.ValueInSI);

            flashStream.Specify("T", In.Streams[0].Mixed.Temperature.ValueInSI);
            flash.CalculateTP(flashStream);       
            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                Vap.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = (flashStream.Vapor.ComponentMolarflow[i]).Eval(eval);
                Liq.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = (flashStream.Liquid.ComponentMolarflow[i]).Eval(eval);
            }

            if (T.IsFixed)
            {
                Vap.Streams[0].Mixed.Temperature.ValueInSI = T.ValueInSI;
                Liq.Streams[0].Mixed.Temperature.ValueInSI = T.ValueInSI;
            }
            else
            {
                Vap.Streams[0].Mixed.Temperature.ValueInSI = flashStream.Mixed.Temperature.ValueInSI;
                Liq.Streams[0].Mixed.Temperature.ValueInSI = flashStream.Mixed.Temperature.ValueInSI;
                T.ValueInSI = flashStream.Mixed.Temperature.ValueInSI;
            }

            if (p.IsFixed)
            {
                Vap.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI;
                Liq.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI;
            }
            else
            {
                Vap.Streams[0].Mixed.Pressure.ValueInSI = flashStream.Mixed.Pressure.ValueInSI;
                Liq.Streams[0].Mixed.Pressure.ValueInSI = flashStream.Mixed.Pressure.ValueInSI;
                p.ValueInSI = flashStream.Mixed.Pressure.ValueInSI;
            }

            if (!Q.IsFixed)
                Q.ValueInSI = -(In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Liq.Streams[0].Mixed.SpecificEnthalpy * Liq.Streams[0].Mixed.TotalMolarflow - Vap.Streams[0].Mixed.SpecificEnthalpy * Vap.Streams[0].Mixed.TotalMolarflow).Eval(eval);

            Vap.Streams[0].GetVariable("VF").SetValue(1);
            Liq.Streams[0].GetVariable("VF").SetValue(0);

            flash.CalculateTP(Vap.Streams[0]);
            flash.CalculateTP(Liq.Streams[0]);

            Vap.Streams[0].State = PhaseState.DewPoint;
            Liq.Streams[0].State = PhaseState.BubblePoint;

            return this;
        }
    }
}
