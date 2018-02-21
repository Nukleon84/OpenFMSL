using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.UnitsOfMeasure;

namespace OpenFMSL.Core.ModelLibrary
{
    public class Mixer : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        public Mixer(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Mixer";

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, -1));
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in mixer", PhysicalDimension.Pressure);
            dp.LowerBound = 0;
            AddVariable(dp);
            AddVariable(p);
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                    Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[cindex])
                        .IsEqualTo(Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Mixed.ComponentMolarflow[cindex])),"Mass Balance");

            }


            AddEquationToEquationSystem(problem, (p / 1e4).IsEqualTo((Sym.Min(In.Streams[0].Mixed.Pressure, In.Streams[1].Mixed.Pressure) - dp) / 1e4),"Pressure Balance");

            foreach (var outlet in Out.Streams)
            {
                AddEquationToEquationSystem(problem, (outlet.Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure drop");
            }

            AddEquationToEquationSystem(problem,
                ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow)/1e4))
                .IsEqualTo(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Mixed.SpecificEnthalpy * Out.Streams[i].Mixed.TotalMolarflow) / 1e4),"Heat Balance");

            base.FillEquationSystem(problem);

        }


        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            int NC = System.Components.Count;


            p.ValueInSI = Math.Min(In.Streams[0].Mixed.Pressure.ValueInSI, In.Streams[1].Mixed.Pressure.ValueInSI);
            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[i]).Eval(eval);
            }

            Out.Streams[0].Mixed.Temperature.ValueInSI = (Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.Temperature) / In.NumberOfStreams).Eval(eval);
            Out.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI;

            var flash = new FlashRoutines(new Numerics.Solvers.Newton());
            flash.CalculateTP(Out.Streams[0]);
            return this;
        }
    }
}
