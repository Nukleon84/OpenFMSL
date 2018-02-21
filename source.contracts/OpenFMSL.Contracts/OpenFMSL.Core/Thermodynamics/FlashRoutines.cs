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

            //  var tempStream = new MaterialStream("temp", stream.System);
            // tempStream.CopyFrom(stream);
            copy.GetVariable("p").IsFixed = true;
            copy.GetVariable("T").IsFixed = false;
            copy.GetVariable("VF").IsFixed = false;
            copy.Init("VF", stream.Vfmolar.ValueInSI);
            foreach (var comp in stream.System.Components)
            {
                copy.GetVariable("n["+comp.ID+"]").IsFixed = true;
            }
            problem2.AddConstraints((copy.Mixed.SpecificEnthalpy * copy.Mixed.TotalMolarflow).IsEqualTo(enthalpy));
            copy.FillEquationSystem(problem2);

            var solver = new Decomposer();
            solver.Solve(problem2);
                  
            performMassBalance(copy, copy.KValues);
            performDensityUpdate(copy);
            performEnthalpyUpdate(copy);

            stream.CopyFrom(copy);
           // performMassBalance(stream, copy.KValues);
           // performDensityUpdate(stream);
           // performEnthalpyUpdate(stream);


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

            //Calculate Dew Point and Bubble Point

            /*  var KD = new Variable[stream.System.Components.Count];
              var KB = new Variable[stream.System.Components.Count];
              var TD = stream.System.VariableFactory.CreateVariable("TD", "Dew-point temperature", PhysicalDimension.Temperature);
              var TB = stream.System.VariableFactory.CreateVariable("TB", "Bubble-point temperature", PhysicalDimension.Temperature);

              for (int i = 0; i < NC; i++)
              {
                  KD[i] = stream.System.VariableFactory.CreateVariable("KD", "Equilibrium distribution coefficient", PhysicalDimension.Dimensionless);
                  KD[i].Subscript = stream.System.Components[i].ID;
                  KD[i].Group = "Dew-Point";
                  KD[i].ValueInSI = 1.4;

                  KB[i] = stream.System.VariableFactory.CreateVariable("KB", "Equilibrium distribution coefficient", PhysicalDimension.Dimensionless);
                  KB[i].Subscript = stream.System.Components[i].ID;
                  KB[i].Group = "Bubble-Point";
                  KB[i].ValueInSI = 1.4;
              }
              var problem = new EquationSystem() { Name = "Phase boundary" };
              for (int i = 0; i < NC; i++)
              {
                  stream.System.EquationFactory.EquilibriumCoefficient(stream.System, KD[i], TD, stream.Mixed.Pressure, stream.Liquid.ComponentMolarFraction, stream.Vapor.ComponentMolarFraction, i);
                  stream.System.EquationFactory.EquilibriumCoefficient(stream.System, KB[i], TB, stream.Mixed.Pressure, stream.Liquid.ComponentMolarFraction, stream.Vapor.ComponentMolarFraction, i);
                  stream.System.EquationFactory.EquilibriumCoefficient(stream.System, stream.KValues[i], stream.Mixed.Temperature, stream.Mixed.Pressure, stream.Liquid.ComponentMolarFraction, stream.Vapor.ComponentMolarFraction, i);

              }
              problem.AddConstraints((Sym.Sum(0, NC, i => stream.Mixed.ComponentMolarFraction[i] * KB[i])).IsEqualTo(1));
              problem.AddConstraints((Sym.Sum(0, NC, i => stream.Mixed.ComponentMolarFraction[i] / KD[i])).IsEqualTo(1));

              problem.AddDefinedVariables(KD);
              problem.AddDefinedVariables(KB);

              problem.AddVariables(TD);
              problem.AddVariables(TB);
              _solver.Solve(problem);*/


            //Solving Rachford Rice equation to get actual vapour fraction

            var rachfordRice = Sym.Sum(0, NC, i => stream.Mixed.ComponentMolarFraction[i] * (1 - stream.KValues[i]) / (1 + stream.Vfmolar * (stream.KValues[i] - 1)));
            var rachfordRiceEq = rachfordRice.IsEqualTo(0);

            stream.Vfmolar.ValueInSI = 0;
            var rrAt0 = rachfordRice.Eval(new Evaluator());
            stream.Vfmolar.ValueInSI = 1;
            var rrAt1 = rachfordRice.Eval(new Evaluator());

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
            else
            {
                stream.Vfmolar.ValueInSI = 0.5;
            }

            if (stream.Vfmolar.ValueInSI == 0.5)
            {
                var problem2 = new EquationSystem() { Name = "Rachford-Rice" };

                stream.Vfmolar.LowerBound = -5;
                stream.Vfmolar.UpperBound = 5;
                problem2.AddConstraints(rachfordRiceEq);
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
            //  stream.GetVariable("d").ValueInSI = 0;
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


        #region Flash Calculation
        /*
          override bool Initialize()
          {
              int NC = ComponentSet.Count;

              //T,P Flash
              if (Mixed.Temperature.IsFixed && Mixed.Pressure.IsFixed)
              {
                  performTPFlash();
              }
              //x,P Flash
              else if (_vf.IsFixed && Mixed.Pressure.IsFixed)
              {
                  performXPFlash();
              }
              else
              {
                  performTPFlash();
              }

              performMassBalance();


              performDensityUpdate();


              performEnthalpyUpdate(NC);


              return true;
          }

          private void performXPFlash()
          {
              int NC = System.Components.Count;

              //if (_vf.Value == 0)
              //{
              //    State = PhaseState.BubblePoint;
              //}
              //else if (_vf.Value == 1)
              //{
              //    State = PhaseState.DewPoint;
              //}
              //else
              //{
              //    State = PhaseState.LiquidVapour;
              //}

              var problem = new NLP();
              problem.Name = "Flash: " + Name;
              // problem.AddVariables(_k);
              //Console.WriteLine(Mixed.Temperature.WriteReport());


              for (int i = 0; i < ComponentSet.Count; i++)
              {
                  System.EquationFactory.CreateKValueExpression(problem, System, _k[i], Mixed.Temperature, Mixed.Pressure, Liquid.GetMolarFractionArray(), Vapour.GetMolarFractionArray(), i);
              }

              var solver = new Newton();
              problem.AddVariables(Mixed.Temperature);
              if (_vf.Value <= 1e-10)
              {
                  problem.AddConstraints(Sym.Sum(0, NC, i => Mixed.ComponentMolarFraction[i] * _k[i]).IsEqualTo(1));
              }
              else if (_vf.Value >= 1 - 1e-10)
              {
                  problem.AddConstraints(Sym.Sum(0, NC, i => Mixed.Components[i].MolarFraction / _k[i]).IsEqualTo(1));
              }
              else
              {
                  var rachfordRice = Sym.Sum(0, NC, i => Mixed.Components[i].MolarFraction * Sym.Par(_k[i] - 1) / (_vf + Sym.Par(1 - _vf) * _k[i]));
                  var rachfordRiceEq = rachfordRice.IsEqualTo(0);
                  problem.AddConstraints(rachfordRiceEq);
              }
              solver.Solve(problem, 50, 0.95, 1e-6, true, true);
          }

          private void performEnthalpyUpdate(int NC)
          {
              Vapour.Enthalpy.Value = (Sym.Par(Sym.Sum(0, NC, (idx) => Vapour.Components[idx].MolarFraction * System.EquationFactory.GetVaporEnthalpyExpression(System, Mixed.Temperature, idx)))).Eval();
              Liquid.Enthalpy.Value = (Sym.Par(Sym.Sum(0, NC, (idx) => Liquid.Components[idx].MolarFraction * System.EquationFactory.GetLiquidEnthalpyExpression(System, Mixed.Temperature, idx)))).Eval();

              if (Double.IsNaN(Liquid.Enthalpy.Value))
                  Liquid.Enthalpy.Value = 0;
              if (Double.IsNaN(Vapour.Enthalpy.Value))
                  Vapour.Enthalpy.Value = 0;

              Mixed.Enthalpy.Value =
                  ((Vapour.Molarflow * Vapour.Enthalpy + Liquid.Molarflow * Liquid.Enthalpy) / Mixed.Molarflow).Eval();
          }

          private void performDensityUpdate()
          {
              for (int i = 0; i < ComponentSet.Count; i++)
              {
                  Liquid.Components[i].MolarVolume.Value = 1.0 /
                                                           System.EquationFactory.GetLiquidDensityExpression(System,
                                                               Liquid.Temperature, i).Eval();
                  Vapour.Components[i].MolarVolume.Value = 1.0 /
                                                           System.EquationFactory.GetVapourDensityExpression(System,
                                                               Liquid.Temperature, Vapour.Pressure, i).Eval();

                  Mixed.Components[i].MolarVolume.Value =
                      (Liquid.Components[i].MolarVolume + Vapour.Components[i].MolarVolume).Eval();
              }
          }


          private void performMassBalance()
          {
              Vapour.Molarflow.Value = _vf.Value * Mixed.Molarflow.Value;

              Liquid.Molarflow.Value = Mixed.Molarflow.Value - Vapour.Molarflow.Value;

              for (int i = 0; i < ComponentSet.Count; i++)
              {
                  Liquid.Components[i].MolarFraction.Value = (Mixed.Components[i].MolarFraction / (1 + _vf * (_k[i] - 1))).Eval();
                  Vapour.Components[i].MolarFraction.Value = (Liquid.Components[i].MolarFraction * _k[i]).Eval();

                  Liquid.Components[i].MolarFlow.Value = Liquid.Components[i].MolarFraction.Value * Liquid.Molarflow.Value;
                  Vapour.Components[i].MolarFlow.Value = Vapour.Components[i].MolarFraction.Value * Vapour.Molarflow.Value;

                  Liquid.Components[i].MassFlow.Value =
                      (Liquid.Components[i].MolarFlow *
                       Sym.Convert(System.Components[i].PURE.MolarWeight,
                           System.VariableFactory.Internal.MassUnit / System.VariableFactory.Internal.MoleUnit)).Eval();
                  Vapour.Components[i].MassFlow.Value =
                      (Vapour.Components[i].MolarFlow *
                       Sym.Convert(System.Components[i].PURE.MolarWeight,
                           System.VariableFactory.Internal.MassUnit / System.VariableFactory.Internal.MoleUnit)).Eval();
                  Mixed.Components[i].MassFlow.Value =
                      (Mixed.Components[i].MolarFlow *
                       Sym.Convert(System.Components[i].PURE.MolarWeight,
                           System.VariableFactory.Internal.MassUnit / System.VariableFactory.Internal.MoleUnit)).Eval();
              }

              Mixed.Massflow.Value = Mixed.GetMassFlowArray().Sum(v => v.Value);
              Vapour.Massflow.Value = Vapour.GetMassFlowArray().Sum(v => v.Value);
              Liquid.Massflow.Value = Liquid.GetMassFlowArray().Sum(v => v.Value);

              for (int i = 0; i < ComponentSet.Count; i++)
              {
                  if (Liquid.Massflow.Value <= 1e-8)
                      Liquid.Components[i].MassFraction.Value = 0;
                  else
                      Liquid.Components[i].MassFraction.Value = (Liquid.Components[i].MassFlow / Liquid.Massflow.Value).Eval();

                  if (Vapour.Massflow.Value <= 1e-8)
                      Vapour.Components[i].MassFraction.Value = 0;
                  else
                      Vapour.Components[i].MassFraction.Value = (Vapour.Components[i].MassFlow / Vapour.Massflow.Value).Eval();
                  Mixed.Components[i].MassFraction.Value = (Mixed.Components[i].MassFlow / Mixed.Massflow.Value).Eval();
              }
          }*/
        #endregion

    }
}
