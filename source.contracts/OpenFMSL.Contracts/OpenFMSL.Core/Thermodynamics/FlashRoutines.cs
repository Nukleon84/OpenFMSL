using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Numerics.Solvers;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public class FlashRoutines
    {
        Newton _solver;

        public FlashRoutines(Newton solver)
        {
            _solver = solver;
        }

        public bool CalculateTP(MaterialStream stream)
        {
            PrecalculateTP(stream);

            return true;
        }

        public bool CalculateZP(MaterialStream stream)
        {
            PrecalculateZP(stream);

            return true;
        }

        public bool CalculatePQ(MaterialStream stream, double enthalpy)
        {
            var copy = new MaterialStream("copy", stream.System);
            copy.CopyFrom(stream);

            PrecalculateTP(copy);

            var problem2 = new EquationSystem() { Name = "PQ-Flash" };

            copy.GetVariable("p").IsFixed = true;
            copy.GetVariable("T").IsFixed = false;
            copy.GetVariable("VF").IsFixed = false;
            copy.Init("VF", stream.Vfmolar.ValueInSI);
            foreach (var comp in stream.System.Components)
            {
                copy.GetVariable("n[" + comp.ID + "]").IsFixed = true;
            }
            problem2.AddConstraints((copy.Mixed.SpecificEnthalpy * copy.Mixed.TotalMolarflow).IsEqualTo(enthalpy));
            copy.FillEquationSystem(problem2);

            var solver = new Decomposer();
            solver.Solve(problem2);

            performMassBalance(copy, copy.KValues);
            performDensityUpdate(copy);
            performEnthalpyUpdate(copy);

            stream.CopyFrom(copy);
            return true;

        }


        void PrecalculateZP(MaterialStream stream)
        {
            stream.Mixed.TotalMolarflow.ValueInSI = stream.Mixed.ComponentMolarflow.Sum(v => v.ValueInSI);
            var NC = stream.System.Components.Count;

            for (int i = 0; i < NC; i++)
            {
                stream.Mixed.ComponentMolarFraction[i].ValueInSI = stream.Mixed.ComponentMolarflow[i].ValueInSI / stream.Mixed.TotalMolarflow.ValueInSI;
                stream.Liquid.ComponentMolarFraction[i].ValueInSI = stream.Mixed.ComponentMolarFraction[i].ValueInSI;
                stream.Vapor.ComponentMolarFraction[i].ValueInSI = stream.Mixed.ComponentMolarFraction[i].ValueInSI;
            }

            var rachfordRice = Sym.Sum(0, NC, i => stream.Mixed.ComponentMolarFraction[i] * (1 - stream.KValues[i]) / (1 + stream.Vfmolar * (stream.KValues[i] - 1)));
            var rachfordRiceEq = rachfordRice.IsEqualTo(0);


            var problem2 = new EquationSystem() { Name = "Rachford-Rice" };

            if (stream.Vfmolar.ValueInSI <= 1e-10)
            {
                problem2.AddConstraints(Sym.Sum(0, NC, i => stream.Mixed.ComponentMolarFraction[i] * stream.KValues[i]).IsEqualTo(1));
            }
            else if (stream.Vfmolar.ValueInSI >= 1 - 1e-10)
            {
                problem2.AddConstraints(Sym.Sum(0, NC, i => stream.Mixed.ComponentMolarFraction[i] / stream.KValues[i]).IsEqualTo(1));
            }
            else
            {
                problem2.AddConstraints(rachfordRiceEq);
            }
            problem2.AddVariables(stream.Mixed.Temperature);
            _solver.Solve(problem2);

            performMassBalance(stream, stream.KValues);
            performDensityUpdate(stream);
            performEnthalpyUpdate(stream);

        }

        void PrecalculateTP(MaterialStream stream)
        {
            stream.Mixed.TotalMolarflow.ValueInSI = stream.Mixed.ComponentMolarflow.Sum(v => v.ValueInSI);
            var NC = stream.System.Components.Count;

            for (int i = 0; i < NC; i++)
            {
                stream.Mixed.ComponentMolarFraction[i].ValueInSI = stream.Mixed.ComponentMolarflow[i].ValueInSI / stream.Mixed.TotalMolarflow.ValueInSI;
                stream.Liquid.ComponentMolarFraction[i].ValueInSI = stream.Mixed.ComponentMolarFraction[i].ValueInSI;
                stream.Vapor.ComponentMolarFraction[i].ValueInSI = stream.Mixed.ComponentMolarFraction[i].ValueInSI;
            }

            //Solving Rachford Rice equation to get actual vapour fraction
            var x = stream.Mixed.ComponentMolarFraction;
            var K = stream.KValues;
            var a = stream.Vfmolar;
            var rachfordRice = Sym.Sum(0, NC, i => x[i] * (1 - K[i]) / (1 + a * (K[i] - 1)));
          
            //Evaluate Rachford-Rice at the edge cases of the VLE area
            stream.Vfmolar.ValueInSI = 0;
            var rrAt0 = rachfordRice.Eval(new Evaluator());
            stream.Vfmolar.ValueInSI = 1;
            var rrAt1 = rachfordRice.Eval(new Evaluator());

            stream.Vfmolar.ValueInSI = 0.5;

            if (rrAt0 > 0)
            {
                stream.Vfmolar.ValueInSI = 0;
            }
            else if (Math.Abs(rrAt0) < 1e-8)
            {
                stream.Vfmolar.ValueInSI = 0;
            }
            else if (Math.Abs(rrAt1) < 1e-8)
            {
                stream.Vfmolar.ValueInSI = 1;
            }
            else if (rrAt1 < 0)
            {
                stream.Vfmolar.ValueInSI = 1;
            }
                       

            if (stream.Vfmolar.ValueInSI == 0.5 || (rrAt0 == 0 && rrAt1 == 0))
            {
                for (int i = 0; i < NC; i++)
                {                
                    stream.Liquid.ComponentMolarFraction[i].ValueInSI = 0.95*stream.Mixed.ComponentMolarFraction[i].ValueInSI;
                    stream.Vapor.ComponentMolarFraction[i].ValueInSI = 1.05*stream.Mixed.ComponentMolarFraction[i].ValueInSI;
                }

                stream.Vfmolar.ValueInSI = 0.5;
                var problem2 = new EquationSystem() { Name = "Rachford-Rice" };
                stream.Vfmolar.LowerBound = -5;
                stream.Vfmolar.UpperBound = 5;
                problem2.AddConstraints(rachfordRice.IsEqualTo(0));
                problem2.AddVariables(stream.Vfmolar);               
                _solver.Solve(problem2);
              

            }

            if (Double.IsNaN(stream.Vfmolar.ValueInSI))
                stream.Vfmolar.ValueInSI = 0;

            stream.Vfmolar.LowerBound = 0;
            stream.Vfmolar.UpperBound = 1;
            if (stream.Vfmolar.ValueInSI > 1)
                stream.Vfmolar.ValueInSI = 1;
            if (stream.Vfmolar.ValueInSI < 0)
                stream.Vfmolar.ValueInSI = 0;
            
            performMassBalance(stream, stream.KValues);
            performDensityUpdate(stream);
            performEnthalpyUpdate(stream);
        }

        private void performMassBalance(MaterialStream stream, Variable[] K)
        {
            var NC = stream.System.Components.Count;

            stream.Vapor.TotalMolarflow.ValueInSI = stream.Vfmolar.ValueInSI * stream.Mixed.TotalMolarflow.ValueInSI;

            stream.Liquid.TotalMolarflow.ValueInSI = stream.Mixed.TotalMolarflow.ValueInSI - stream.Vapor.TotalMolarflow.ValueInSI;
            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                eval.Reset();
                stream.Liquid.ComponentMolarFraction[i].ValueInSI = (stream.Mixed.ComponentMolarFraction[i] / (1 + stream.Vfmolar * (K[i] - 1))).Eval(eval);
                stream.Vapor.ComponentMolarFraction[i].ValueInSI = (stream.Liquid.ComponentMolarFraction[i] * K[i]).Eval(eval);

                stream.Liquid.ComponentMolarflow[i].ValueInSI = stream.Liquid.ComponentMolarFraction[i].ValueInSI * stream.Liquid.TotalMolarflow.ValueInSI;
                stream.Vapor.ComponentMolarflow[i].ValueInSI = stream.Vapor.ComponentMolarFraction[i].ValueInSI * stream.Vapor.TotalMolarflow.ValueInSI;

                stream.Liquid.ComponentMassflow[i].ValueInSI = (stream.Liquid.ComponentMolarflow[i] * Sym.Convert(stream.System.Components[i].GetConstant(ConstantProperties.MolarWeight), stream.System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Mass] / stream.System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Mole])).Eval(eval);
                stream.Vapor.ComponentMassflow[i].ValueInSI = (stream.Vapor.ComponentMolarflow[i] * Sym.Convert(stream.System.Components[i].GetConstant(ConstantProperties.MolarWeight), stream.System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Mass] / stream.System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Mole])).Eval(eval);
                stream.Mixed.ComponentMassflow[i].ValueInSI = (stream.Mixed.ComponentMolarflow[i] * Sym.Convert(stream.System.Components[i].GetConstant(ConstantProperties.MolarWeight), stream.System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Mass] / stream.System.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Mole])).Eval(eval);

            }

            stream.Mixed.TotalMassflow.ValueInSI = stream.Mixed.ComponentMassflow.Sum(v => v.ValueInSI);
            stream.Vapor.TotalMassflow.ValueInSI = stream.Vapor.ComponentMassflow.Sum(v => v.ValueInSI);
            stream.Liquid.TotalMassflow.ValueInSI = stream.Liquid.ComponentMassflow.Sum(v => v.ValueInSI);

            for (int i = 0; i < NC; i++)
            {
                eval.Reset();

                if (stream.Liquid.TotalMassflow.ValueInSI <= 1e-8)
                    stream.Liquid.ComponentMassFraction[i].ValueInSI = 0;
                else
                    stream.Liquid.ComponentMassFraction[i].ValueInSI = (stream.Liquid.ComponentMassflow[i] / stream.Liquid.TotalMassflow).Eval(eval);

                if (stream.Vapor.TotalMassflow.ValueInSI <= 1e-8)
                    stream.Vapor.ComponentMassFraction[i].ValueInSI = 0;
                else
                    stream.Vapor.ComponentMassFraction[i].ValueInSI = (stream.Vapor.ComponentMassflow[i] / stream.Vapor.TotalMassflow).Eval(eval);

                stream.Mixed.ComponentMassFraction[i].ValueInSI = (stream.Mixed.ComponentMassflow[i] / stream.Mixed.TotalMassflow.ValueInSI).Eval(eval);
            }
        }


        private void performEnthalpyUpdate(MaterialStream stream)
        {
            var NC = stream.System.Components.Count;
            var eval = new Evaluator();

            stream.Vapor.SpecificEnthalpy.ValueInSI = (Sym.Par(Sym.Sum(0, NC, (idx) => stream.Vapor.ComponentMolarFraction[idx] * stream.System.EquationFactory.GetVaporEnthalpyExpression(stream.System, idx, stream.Mixed.Temperature)))).Eval(eval);
            stream.Liquid.SpecificEnthalpy.ValueInSI = (Sym.Par(Sym.Sum(0, NC, (idx) => stream.Liquid.ComponentMolarFraction[idx] * stream.System.EquationFactory.GetLiquidEnthalpyExpression(stream.System, idx, stream.Mixed.Temperature)))).Eval(eval);

            if (Double.IsNaN(stream.Liquid.SpecificEnthalpy.ValueInSI))
                stream.Liquid.SpecificEnthalpy.ValueInSI = 0;
            if (Double.IsNaN(stream.Vapor.SpecificEnthalpy.ValueInSI))
                stream.Vapor.SpecificEnthalpy.ValueInSI = 0;

            stream.Mixed.SpecificEnthalpy.ValueInSI = ((stream.Vapor.TotalMolarflow * stream.Vapor.SpecificEnthalpy + stream.Liquid.TotalMolarflow * stream.Liquid.SpecificEnthalpy) / stream.Mixed.TotalMolarflow).Eval(eval);
        }

        private void performDensityUpdate(MaterialStream stream)
        {
            var NC = stream.System.Components.Count;
            var eval = new Evaluator();

            for (int i = 0; i < NC; i++)
            {
                stream.Liquid.ComponentMolarVolume[i].ValueInSI = 1.0 / stream.System.EquationFactory.GetLiquidDensityExpression(stream.System,
                                                             stream.System.Components[i], stream.Mixed.Temperature, stream.Mixed.Pressure).Eval(eval);
                stream.Vapor.ComponentMolarVolume[i].ValueInSI = 1.0 / stream.System.EquationFactory.GetVaporDensityExpression(stream.System,
                                                             stream.System.Components[i], stream.Mixed.Temperature, stream.Mixed.Pressure).Eval(eval);

                stream.Mixed.ComponentMolarVolume[i].ValueInSI = (stream.Liquid.ComponentMolarVolume[i] + stream.Vapor.ComponentMolarVolume[i]).Eval(eval);
            }
        }




    }
}
