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
    public class BlackBoxReactor : ProcessUnit
    {
        private Variable dp;
        private Variable p;
        private Variable T;
        private Variable Q;
        private Variable[] R;
        double[,] _stochiometry;

        int _numberOfReactions = 1;

        public BlackBoxReactor(string name, ThermodynamicSystem system, int numberOfReactions) : base(name, system)
        {
            Class = "Reactor";
            _numberOfReactions = numberOfReactions;

            MaterialPorts.Add(new Port<MaterialStream>("In", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Out", PortDirection.Out, 1));

            R = new Variable[_numberOfReactions];
            _stochiometry = new double[_numberOfReactions, System.Components.Count];

            for (int i = 0; i < _numberOfReactions; i++)
            {
                R[i] = system.VariableFactory.CreateVariable("R", (i + 1).ToString(), "Converted Molar Flow for reaction " + (i + 1).ToString(), PhysicalDimension.MolarFlow);
            }

            dp = system.VariableFactory.CreateVariable("DP", "Pressure Drop", PhysicalDimension.Pressure);
            p = system.VariableFactory.CreateVariable("P", "Pressure in heater outlet", PhysicalDimension.Pressure);
            T = system.VariableFactory.CreateVariable("T", "Temperature in heater outlet", PhysicalDimension.Temperature);
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            dp.LowerBound = -1e9;
            dp.ValueInSI = 0;
            AddVariable(dp);
            AddVariable(p);
            AddVariable(T);
            AddVariables(R);
            AddVariable(Q);
        }
        public BlackBoxReactor DefineRateEquation(int reactionNumber, Expression rate)
        {
            if(reactionNumber>0 && reactionNumber< _numberOfReactions+1)
            {
                R[reactionNumber - 1].BindTo(rate);
            }
            return this;
        }
        public BlackBoxReactor AddStochiometry(int reactionNumber, string compID, double factor)
        {
            var compIndex = System.Components.FindIndex(c => c.ID == compID);

            if (compIndex >= 0 && reactionNumber > 0)
            {
                _stochiometry[reactionNumber - 1, compIndex] = factor;
            }

            return this;
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");

            for (int i = 0; i < NC; i++)
            {
                var cindex = i;

                Expression reactingMoles = 0;
                for (int j = 0; j < _numberOfReactions; j++)
                {
                    if (Math.Abs(_stochiometry[j, i]) > 1e-16)
                        reactingMoles += _stochiometry[j, i] * R[j];
                }

                AddEquationToEquationSystem(problem,
                    Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[cindex]+ reactingMoles)
                        .IsEqualTo(Sym.Sum(0, Out.NumberOfStreams, (j) => Out.Streams[j].Mixed.ComponentMolarflow[cindex])), "Mass Balance");

            }

            AddEquationToEquationSystem(problem, (p / 1e4).IsEqualTo(Sym.Par(In.Streams[0].Mixed.Pressure - dp) / 1e4), "Pressure Balance");


            foreach (var outlet in Out.Streams)
            {
                AddEquationToEquationSystem(problem, (outlet.Mixed.Pressure / 1e4).IsEqualTo(p / 1e4), "Pressure drop");
                AddEquationToEquationSystem(problem, (outlet.Mixed.Temperature / 1e3).IsEqualTo(T / 1e3), "Heat Balance");
            }

            AddEquationToEquationSystem(problem,
          ((Sym.Sum(0, In.NumberOfStreams, (i) => In.Streams[i].Mixed.SpecificEnthalpy * In.Streams[i].Mixed.TotalMolarflow + Q) / 1e4))
          .IsEqualTo(Sym.Par(Sym.Sum(0, Out.NumberOfStreams, (i) => Out.Streams[i].Mixed.SpecificEnthalpy * Out.Streams[i].Mixed.TotalMolarflow)) / 1e4), "Heat Balance");

            base.FillEquationSystem(problem);
        }

        public override ProcessUnit Initialize()
        {
            var In = FindMaterialPort("In");
            var Out = FindMaterialPort("Out");
            int NC = System.Components.Count;

            if (!p.IsFixed)
                p.ValueInSI = In.Streams[0].Mixed.Pressure.ValueInSI;

            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                Out.Streams[0].Mixed.ComponentMolarflow[i].ValueInSI = Sym.Sum(0, In.NumberOfStreams, (j) => In.Streams[j].Mixed.ComponentMolarflow[i]).Eval(eval);
            }

            Out.Streams[0].Mixed.Temperature.ValueInSI = T.ValueInSI;
            Out.Streams[0].Mixed.Pressure.ValueInSI = p.ValueInSI - dp.ValueInSI;
            Out.Streams[0].Vfmolar.ValueInSI = In.Streams[0].Vfmolar.ValueInSI;


            var flash = new FlashRoutines(new Numerics.Solvers.Newton());
            if (T.IsFixed)
                flash.CalculateTP(Out.Streams[0]);

            Q.ValueInSI = -(In.Streams[0].Mixed.SpecificEnthalpy * In.Streams[0].Mixed.TotalMolarflow - Out.Streams[0].Mixed.SpecificEnthalpy * Out.Streams[0].Mixed.TotalMolarflow).Eval(eval);
            return this;
        }
    }
}
