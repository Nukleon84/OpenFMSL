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
    public class Splitter : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable SplitFactor;
        public Splitter(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "Splitter";

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out1", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out2", PortDirection.Out, 1));

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in splitter", PhysicalDimension.Pressure);
            SplitFactor = system.VariableFactory.CreateVariable("K", "Split factor (molar)", PhysicalDimension.MolarFraction);            
            dp.LowerBound = 0;
            AddVariable(dp);
            AddVariable(SplitFactor);
            AddVariable(p);
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out1 = FindMaterialPort("Out1");
            var Out2 = FindMaterialPort("Out2");

         
            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                   (SplitFactor * In.Streams[0].Mixed.ComponentMolarflow[cindex])
                        .IsEqualTo(Out1.Streams[0].Mixed.ComponentMolarflow[cindex]), "Mass Balance");

            }
            for (int i = 0; i < NC; i++)
            {
                var cindex = i;
                AddEquationToEquationSystem(problem,
                   (Sym.Par(1-SplitFactor) * In.Streams[0].Mixed.ComponentMolarflow[cindex])
                        .IsEqualTo(Out2.Streams[0].Mixed.ComponentMolarflow[cindex]), "Mass Balance");

            }

            AddEquationToEquationSystem(problem, (p / 1e4).IsEqualTo((In.Streams[0].Mixed.Pressure - dp) / 1e4), "Pressure drop");

            AddEquationToEquationSystem(problem, (Out1.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");            
            AddEquationToEquationSystem(problem, (Out1.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(Sym.Par(In.Streams[0].Mixed.Temperature) / 1e3), "Temperature Balance");
            AddEquationToEquationSystem(problem, (Out2.Streams[0].Mixed.Pressure / 1e4).IsEqualTo(Sym.Par(p) / 1e4), "Pressure Balance");
            AddEquationToEquationSystem(problem, (Out2.Streams[0].Mixed.Temperature / 1e3).IsEqualTo(Sym.Par(In.Streams[0].Mixed.Temperature) / 1e3), "Temperature Balance");
            
   
            base.FillEquationSystem(problem);

        }


        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out1 = FindMaterialPort("Out1");
            var Out2 = FindMaterialPort("Out2");
            int NC = System.Components.Count;


            p.ValueInSI = In.Streams[0].Mixed.Pressure.ValueInSI;
            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                Out1.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = ((SplitFactor)* Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[i])).Eval(eval);
                Out2.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = ((1 - SplitFactor) * Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[i])).Eval(eval);
            }

            Out1.Streams[0].Mixed.Temperature.ValueInSI =In.Streams[0].Mixed.Temperature.ValueInSI;
            Out2.Streams[0].Mixed.Temperature.ValueInSI = In.Streams[0].Mixed.Temperature.ValueInSI;
            Out1.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI;
            Out2.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI;

            var flash = new FlashRoutines(new Numerics.Solvers.Newton());
            flash.CalculateTP(Out1.Streams[0]);
            flash.CalculateTP(Out2.Streams[0]);
            return this;
        }
    }
}
