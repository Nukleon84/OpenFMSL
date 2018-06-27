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
using OpenFMSL.Core.ThermodynamicModels;

namespace OpenFMSL.Core.ModelLibrary
{
    public class Heater : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable VF;
        private Variable Q;
        private Variable[] r;
        public Heater(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Heater";
            Icon.IconType = IconTypes.Heater;

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1));
            HeatPorts.Add(new Port<HeatStream>("Duty", PortDirection.In, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in heater outlet", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in heater outlet", PhysicalDimension.Temperature);
            VF = system.VariableFactory.CreateVariable("VF", "Vapor fraction in heater outlet", PhysicalDimension.MolarFraction);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            dp.LowerBound = 0;
            dp.ValueInSI = 0;




            AddVariable(dp);
            AddVariable(p);
            AddVariable(T);
            AddVariable(VF);
            AddVariable(Q);

        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            var Duty = FindHeatPort("Duty");

            if (Duty.IsConnected)
            {
                Q.IsFixed = true;
                Q.ValueInSI = 0;
            }


            for (int i = 0; i < NC; i++)
            {
                var cindex = i;

                Expression reactingMoles = 0;

                if (ChemistryBlock != null)
                {
                    reactingMoles = ChemistryBlock.GetReactingMolesExpression(r, System.Components[i]);
                }


                AddEquationToEquationSystem(problem,
                    (Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[cindex]) + reactingMoles)
                        .IsEqualTo(Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Mixed.ComponentMolarflow[cindex])), "Mass Balance");



            }


            if (ChemistryBlock != null)
            {
                foreach (var reac in ChemistryBlock.Reactions)
                {
                    AddEquationToEquationSystem(problem, reac.GetDefiningEquation(Out.Streams[0]), "Reaction rate equation");
                }
            }
            
            AddEquationToEquationSystem(problem, (p / 1e4).IsEqualTo(Sym.Par(In.Streams[0].Mixed.Pressure - dp) / 1e4), "Pressure Balance");

            if (!VF.IsFixed)
                AddEquationToEquationSystem(problem, (VF).IsEqualTo(Out.Streams[0].Vfmolar), "Vapor Fraction");


            foreach (var outlet in Out.Streams)
            {
                AddEquationToEquationSystem(problem, (outlet.Mixed.Pressure / 1e4).IsEqualTo(p / 1e4), "Pressure drop");
                AddEquationToEquationSystem(problem, (outlet.Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Heat Balance");
            }

            if (Duty.IsConnected)
            {
                if (Duty.Direction == PortDirection.In)
                {
                    AddEquationToEquationSystem(problem,
                ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow + Duty.Streams[0].Q) / 1e4))
                .IsEqualTo(Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Mixed.SpecificEnthalpy * Out.Streams[i].Mixed.TotalMolarflow)) / 1e4), "Heat Balance");
                }
                else
                {
                    AddEquationToEquationSystem(problem,
             ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow) / 1e4))
             .IsEqualTo(Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Mixed.SpecificEnthalpy * Out.Streams[i].Mixed.TotalMolarflow) + Duty.Streams[0].Q) / 1e4), "Heat Balance");
                }
            }
            else
            {
                AddEquationToEquationSystem(problem,
              ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow + Q) / 1e4))
              .IsEqualTo(Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Mixed.SpecificEnthalpy * Out.Streams[i].Mixed.TotalMolarflow)) / 1e4), "Heat Balance");
            }
            base.FillEquationSystem(problem);

        }

        public override ProcessUnit EnableChemistry(Chemistry chem)
        {
            base.EnableChemistry(chem);

            if (ChemistryBlock != null)
            {
                r = new Variable[ChemistryBlock.Reactions.Count];
                for (int i = 0; i < ChemistryBlock.Reactions.Count; i++)
                {
                    r[i] = System.VariableFactory.CreateVariable("r", "Reacting molar flow", PhysicalDimension.MolarFlow);
                    r[i].Subscript = (i + 1).ToString();
                    r[i].LowerBound = -1e6;
                }
                AddVariables(r);
            }

            return this;
        }

        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            var Duty = FindHeatPort("Duty");
            int NC = System.Components.Count;

            if (!p.IsFixed)
                p.ValueInSI = In.Streams[0].Mixed.Pressure.ValueInSI;

            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[i]).Eval(eval);
            }


            if (ChemistryBlock != null)
            {
                foreach (var reac in ChemistryBlock.Reactions)
                {
                    foreach (var comp in reac.Stoichiometry)
                    {
                        if (Math.Abs(Out.Streams[0].Mixed.ComponentMolarflow[comp.Index].ValueInSI) < 1e-10 && Math.Abs(comp.StoichiometricFactor) > 1e-6)
                            Out.Streams[0].Mixed.ComponentMolarflow[comp.Index].ValueInSI = 1e-6;
                    }
                }
            }

            Out.Streams[0].Mixed.Temperature.ValueInSI = T.ValueInSI;
            Out.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI - dp.ValueInSI;
            Out.Streams[0].Vfmolar.ValueInSI = In.Streams[0].Vfmolar.ValueInSI;


            var flash = new FlashRoutines(new Numerics.Solvers.Newton());
            if (T.IsFixed)
                flash.CalculateTP(Out.Streams[0]);

            if (VF.IsFixed)
            {
                Out.Streams[0].Vfmolar.ValueInSI = VF.ValueInSI;
                flash.CalculateZP(Out.Streams[0]);
                Out.Streams[0].Vfmolar.FixValue(VF.ValueInSI);
                if (VF.ValueInSI == 0)
                    Out.Streams[0].FixBubblePoint();
                if (VF.ValueInSI == 1)
                    Out.Streams[0].FixDewPoint();
            }

            if (Duty.IsConnected)
            {
                if (Duty.Direction == PortDirection.In)
                    Duty.Streams[0].Q.ValueInSI = -(In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Out.Streams[0].Mixed.SpecificEnthalpy * Out.Streams[0].Mixed.TotalMolarflow).Eval(eval);
                else
                    Duty.Streams[0].Q.ValueInSI = (In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Out.Streams[0].Mixed.SpecificEnthalpy * Out.Streams[0].Mixed.TotalMolarflow).Eval(eval);


            }
            else
            {
                Q.ValueInSI = -(In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Out.Streams[0].Mixed.SpecificEnthalpy * Out.Streams[0].Mixed.TotalMolarflow).Eval(eval);
            }





            return this;
        }
    }
}
