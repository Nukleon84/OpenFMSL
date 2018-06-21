using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.ThermodynamicModels;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public class PropertyFunctionFactory
    {
        public Variable ActivityCoefficient(ThermodynamicSystem system, Variable g, Variable T, List<Variable> x, int index)
        {
            Expression liquidPart = null;

            switch (system.EquilibriumMethod.EquilibriumApproach)
            {
                case EquilibriumApproach.GammaPhi:
                    switch (system.EquilibriumMethod.Activity)
                    {

                        case ActivityMethod.NRTL:
                            {
                                var gamma = new ActivityCoefficientNRTL(system, T, x, index);
                                liquidPart = gamma;
                                break;
                            }
                        case ActivityMethod.Wilson:
                            {
                                var gamma = new ActivityCoefficientWilson(system, T, x, index);
                                liquidPart = gamma;
                                break;
                            }
                        default:
                            liquidPart = 1.0;
                            break;
                    }

                    break;
                case EquilibriumApproach.PhiPhi:
                    throw new NotSupportedException("Only Gamma-Phi allowed");
            }
            g.Subscript = system.Components[index].ID;
            g.BindTo(liquidPart);
            return g;
        }


        public Variable EquilibriumCoefficient(ThermodynamicSystem system, Variable K, Variable T, Variable p, List<Variable> x, List<Variable> y, int index)
        {
            Expression liquidPart = null;
            Expression vaporPart = p;



            var currentComponent = system.Components[index];

            switch (system.EquilibriumMethod.EquilibriumApproach)
            {
                case EquilibriumApproach.GammaPhi:
                    switch (system.EquilibriumMethod.Activity)
                    {

                        case ActivityMethod.NRTL:
                            {
                                var gamma = new ActivityCoefficientNRTL(system, T, x, index);

                                if (currentComponent.IsInert)
                                    liquidPart = new MixtureHenryCoefficient(system, T, x, index);
                                else
                                    liquidPart = gamma * GetVaporPressure(system, currentComponent, T);
                                break;
                            }
                        case ActivityMethod.Wilson:
                            {
                                var gamma = new ActivityCoefficientWilson(system, T, x, index);
                                liquidPart = gamma * GetVaporPressure(system, currentComponent, T);
                                break;
                            }
                        default:
                            liquidPart = GetVaporPressure(system, currentComponent, T);
                            break;
                    }

                    break;
                case EquilibriumApproach.PhiPhi:
                    throw new NotSupportedException("Only Gamma-Phi allowed");
            }
            if (String.IsNullOrEmpty(K.Subscript))
                K.Subscript = system.Components[index].ID;
            K.BindTo(liquidPart / vaporPart);
            return K;
        }

        public Variable GetAverageVaporDensityExpression(ThermodynamicSystem system, Variable[] y, Variable T, Variable p)
        {
            var NC = system.Components.Count;

            var nu = Sym.Sum(0, NC, j => 1 / GetVaporDensityExpression(system, system.Components[j], T, p) * y[j]);

            Variable prop = new Variable("DENV" + "(" + T.FullName + ")", 1);
            prop.Subscript = "avg";
            prop.BindTo(1 / nu);
            return prop;
        }

        public Variable GetAverageMolarWeightExpression(ThermodynamicSystem system, Variable[] z)
        {
            var NC = system.Components.Count;
            var molw = Sym.Sum(0, NC, j => system.Components[j].MolarWeight * z[j]);
            Variable prop = new Variable("MOLW", 1);
            prop.Subscript = "avg";
            prop.BindTo(molw);
            return prop;
        }

        public Variable GetAverageVaporViscosityExpression(ThermodynamicSystem system, Variable[] y, Variable T, Variable p)
        {
            var NC = system.Components.Count;
            var visv = Sym.Sum(0, NC, j => y[j] * Sym.Sqrt(system.Components[j].MolarWeight/1000) * GetVaporViscosityExpression(system, system.Components[j], T, p)) / Sym.Sum(0, NC, j => y[j] * Sym.Sqrt(system.Components[j].MolarWeight / 1000));
            Variable prop = new Variable("VISV" + "(" + T.FullName + ")", 1);
            prop.Subscript = "avg";
            prop.BindTo(visv);
            return prop;
        }






        public Variable GetVaporDensityExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T, Variable p)
        {
            var R = new Variable("R", 8.3144621, SI.J / SI.mol / SI.K);
            var expression = Sym.Convert(p, SI.Pa) / (R * T);
            expression *= Unit.GetConversionFactor(SI.mol / (SI.m ^ 3), system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarDensity]);
            Variable prop = new Variable("DENV" + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }

        public Variable GetLiquidDensityExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T, Variable p)
        {
            var func = comp.GetFunction(EvaluatedProperties.LiquidDensity);
            var expression = system.CorrelationFactory.CreateExpression(func.Type, func, T, null, null);
            expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.MolarDensity]);

            var expresssionDENV = GetVaporDensityExpression(system, comp, T, p);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 0;
            prop.Subscript = comp.ID;
            prop.BindTo(new SafeLiquidDensity(expression, expresssionDENV));
            return prop;


        }


        public Variable GetVaporViscosityExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T, Variable p)
        {
            var func = comp.GetFunction(EvaluatedProperties.VaporViscosity);
            var expression = system.CorrelationFactory.CreateExpression(func.Type, func, T, null, null);
            expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.DynamicViscosity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 0;
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }

        public Variable GetVaporPressure(ThermodynamicSystem system, MolecularComponent comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.VaporPressure);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.Pressure]);

            //var exprmax = system.CorrelationFactory.CreateExpression(func.Type, func, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            //var maxVal = exprmax.Eval(new Evaluator());


            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            // prop.UpperBound = maxVal;
            prop.BindTo(expr);
            return prop;
        }





        public Expression GetVaporEnthalpyExpression(ThermodynamicSystem sys, int idx, Variable T)
        {
            Variable Tref = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Tref;
            Expression expr = null;
            var comp = sys.Components[idx];


            if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].PhaseChangeAtSystemTemperature)
            {
                if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].ReferenceState == PhaseState.Vapour)
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, Tref));
                else
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, Tref))
                        + sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, T);
            }
            else
            {
                if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].ReferenceState == PhaseState.Vapour)
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, Tref));
                else
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange) - sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, Tref))
                        + sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange)
                         + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange));


            }
            Variable prop = new Variable("hV" + "(" + T.FullName + ")", 1);
            prop.Subscript = sys.Components[idx].ID;
            prop.BindTo(expr);
            return expr;
        }

        public Expression GetLiquidEnthalpyExpression(ThermodynamicSystem sys, int idx, Variable T)
        {
            Variable Tref = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Tref;
            Expression expr = null;
            var comp = sys.Components[idx];
            if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].PhaseChangeAtSystemTemperature)
            {
                if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].ReferenceState == PhaseState.Vapour)
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, Tref))
                        - sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, T);
                else
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, Tref));


            }
            else

            {
                if (sys.EnthalpyMethod.PureComponentEnthalpies[idx].ReferenceState == PhaseState.Vapour)
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange) - sys.EquationFactory.GetIdealGasHeatCapacityIntegralExpression(sys, comp, Tref))
                        - sys.EquationFactory.GetEnthalpyOfVaporizationExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange)
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, sys.EnthalpyMethod.PureComponentEnthalpies[idx].TPhaseChange))
                        ;
                else
                    expr = sys.EnthalpyMethod.PureComponentEnthalpies[idx].Href
                        + Sym.Par(sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, T) - sys.EquationFactory.GetLiquidHeatCapacityIntegralExpression(sys, comp, Tref));
            }


            Variable prop = new Variable("hL" + "(" + T.FullName + ")", 1);
            prop.Subscript = sys.Components[idx].ID;
            prop.BindTo(expr);
            return expr;
        }


        public Expression GetEnthalpyOfVaporizationExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.HeatOfVaporization);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.SpecificMolarEnthalpy]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.LowerBound = 0;
            prop.Subscript = comp.ID;
            prop.BindTo(expr);
            return prop;


        }


        public Expression GetIdealGasHeatCapacityExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.IdealGasHeatCapacity);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expr);
            return prop;


        }

        public Expression GetLiquidHeatCapacityExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T)
        {
            var func = comp.GetFunction(EvaluatedProperties.LiquidHeatCapacity);

            var expr = system.CorrelationFactory.CreateExpression(func.Type, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
            expr *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);

            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expr);
            return prop;
        }


        public Expression GetIdealGasHeatCapacityIntegralExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T)
        {
            Expression expression = null;
            var func = comp.GetFunction(EvaluatedProperties.IdealGasHeatCapacity);

            switch (func.Type)
            {
                case FunctionType.Polynomial:
                    expression = system.CorrelationFactory.CreateExpression(FunctionType.PolynomialIntegrated, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;

                case FunctionType.AlyLee:
                    expression = system.CorrelationFactory.CreateExpression(FunctionType.Dippr117, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;

            }
            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "_INT" + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;


        }
        public Expression GetLiquidHeatCapacityIntegralExpression(ThermodynamicSystem system, MolecularComponent comp, Variable T)
        {
            Expression expression = null;
            var func = comp.GetFunction(EvaluatedProperties.LiquidHeatCapacity);

            switch (func.Type)
            {
                case FunctionType.Polynomial:
                    expression = system.CorrelationFactory.CreateExpression(FunctionType.PolynomialIntegrated, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;

                case FunctionType.AlyLee:
                    expression = system.CorrelationFactory.CreateExpression(FunctionType.Dippr117, func, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
                    expression *= Unit.GetConversionFactor(func.YUnit, system.VariableFactory.Internal.UnitDictionary[PhysicalDimension.HeatCapacity]);
                    break;

            }
            Variable prop = new Variable(system.CorrelationFactory.GetVariableNameForProperty(func.Property) + "_INT" + "(" + T.FullName + ")", 1);
            prop.Subscript = comp.ID;
            prop.BindTo(expression);
            return prop;
        }

    }
}
