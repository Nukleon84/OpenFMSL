﻿using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public class CorrelationFactory
    {

        public void EnsureCoefficients(List<Variable> coeff, int newNumber)
        {
            while (coeff.Count < newNumber)
            {
                coeff.Add(new Variable("0", 0));
            }
        }

        public Expression CreateExpression(FunctionType typeToCreate, PropertyFunction func, Variable T, Variable TC, Variable PC)
        {
            Expression expr = null;
            switch (typeToCreate)
            {
                case FunctionType.Polynomial:

                    //if (func.NumberOfCoefficients < 1)
                    //   throw new InvalidOperationException("Not enough coefficients to create an expression");
                    EnsureCoefficients(func.Coefficients, 1);

                    expr = func.Coefficients[0];

                    for (int i = 1; i < func.NumberOfCoefficients; i++)
                    {
                        expr += func.Coefficients[i] * Sym.Pow(T, i);
                    }
                    break;
                case FunctionType.PolynomialIntegrated:

                    EnsureCoefficients(func.Coefficients, 1);
                    expr = func.Coefficients[0] * T;

                    for (int i = 1; i < func.NumberOfCoefficients; i++)
                    {
                        expr += 1.0 / (double)(i + 1) * func.Coefficients[i] * Sym.Pow(T, i + 1);
                    }
                    break;

                case FunctionType.Dippr106:
                    if (System.Math.Abs(TC.ValueInSI - func.Coefficients[0].ValueInSI) < 1e-6)
                    {
                        EnsureCoefficients(func.Coefficients, 6);
                        var TR = Sym.Par(T / func.Coefficients[0]);
                        var h = func.Coefficients[2] + func.Coefficients[3] * TR + func.Coefficients[4] * Sym.Pow(TR, 2) + func.Coefficients[5] * Sym.Pow(TR, 3);
                        expr = func.Coefficients[1] * Sym.Pow(Sym.Par(1 - TR), h);
                    }
                    else
                    {
                        EnsureCoefficients(func.Coefficients, 6);
                        var TR = Sym.Par(T / TC);
                        var h = func.Coefficients[2] + func.Coefficients[3] * TR + func.Coefficients[4] * Sym.Pow(TR, 2) + func.Coefficients[5] * Sym.Pow(TR, 3);
                        expr = func.Coefficients[1] * Sym.Pow(Sym.Par(1 - TR), h);
                    }
                    break;
                case FunctionType.Dippr117:
                    {
                        EnsureCoefficients(func.Coefficients, 5);

                        var Tcon = Sym.Convert(T, func.XUnit);
                        expr = func.Coefficients[0] * Tcon + func.Coefficients[1] * func.Coefficients[2] * Sym.Coth(func.Coefficients[2] / Tcon) - func.Coefficients[3] * func.Coefficients[4] * Sym.Tanh(func.Coefficients[4] / Tcon);
                        break;
                    }
                case FunctionType.AlyLee:
                    {
                        EnsureCoefficients(func.Coefficients, 5);

                        Expression Tcon = T;
                        if (!Unit.AreEquivalent(SI.K, func.XUnit))
                            Tcon = Sym.Convert(T, func.XUnit);

                        expr = func.Coefficients[0] + func.Coefficients[1] * Sym.Pow(Sym.Par((func.Coefficients[2] / Tcon) / Sym.Sinh(func.Coefficients[2] / Tcon)), 2) + func.Coefficients[3] * Sym.Pow(Sym.Par((func.Coefficients[4] / Tcon) / Sym.Cosh(func.Coefficients[4] / Tcon)), 2);
                        break;
                    }
                case FunctionType.Antoine:
                    {
                        EnsureCoefficients(func.Coefficients, 3);
                        Expression CT = T;
                        if (!Unit.AreEquivalent(SI.K, func.XUnit))
                            CT = Sym.Convert(T, func.XUnit);


                        expr = Sym.Exp(func.Coefficients[0] - func.Coefficients[1] / Sym.Par(func.Coefficients[2] + CT));
                        break;
                    }

                case FunctionType.ExtendedAntoine:
                    {
                        EnsureCoefficients(func.Coefficients, 7);
                        Expression CT = T;
                        if (!Unit.AreEquivalent(SI.K, func.XUnit))
                            CT = Sym.Convert(T, func.XUnit);

                        expr = (Sym.Exp(func.Coefficients[0] + func.Coefficients[1] / Sym.Par(func.Coefficients[2] + CT) + func.Coefficients[3] * CT + func.Coefficients[4] * Sym.Ln(CT) + func.Coefficients[5] * Sym.Pow(CT, func.Coefficients[6])));

                        break;
                    }

                case FunctionType.Rackett:
                    {
                        EnsureCoefficients(func.Coefficients, 4);
                        var TR = Sym.Convert(T, func.XUnit) / func.Coefficients[2];
                        expr = func.Coefficients[0] / (Sym.Pow(func.Coefficients[1], 1 + Sym.Pow(Sym.Par(1 - TR), func.Coefficients[3])));

                        break;
                    }
                case FunctionType.Wagner:
                    {
                        EnsureCoefficients(func.Coefficients, 6);

                        if (TC == null || PC == null)
                            throw new InvalidOperationException("Not enough coefficients to create an expression");
                        var TR = Sym.Convert(T, func.XUnit) / TC;
                        var tau = Sym.Par(1 - TR);
                        expr = Sym.Exp(Sym.Ln(PC) + 1 / TR * Sym.Par(func.Coefficients[2] * tau + func.Coefficients[3] * Sym.Pow(tau, 1.5) + func.Coefficients[4] * Sym.Pow(tau, 3) + func.Coefficients[5] * Sym.Pow(tau, 6)));

                        break;
                    }
                case FunctionType.Watson:
                    {
                        EnsureCoefficients(func.Coefficients, 4);

                        expr = func.Coefficients[0] * Sym.Pow(func.Coefficients[2] - T, func.Coefficients[1]) + func.Coefficients[3];
                        break;
                    }
                case FunctionType.Dippr102:
                    {
                        EnsureCoefficients(func.Coefficients, 4);

                        expr = func.Coefficients[0] * Sym.Pow(T, func.Coefficients[1]) / (1 + func.Coefficients[2] / T + func.Coefficients[3] / T);
                        break;
                    }
                case FunctionType.Kirchhoff:
                    {
                        EnsureCoefficients(func.Coefficients, 3);

                        expr = (Sym.Exp(func.Coefficients[0] - func.Coefficients[1] / T + func.Coefficients[2] * Sym.Ln(T)));
                        break;
                    }
                case FunctionType.Sutherland:
                    {
                        EnsureCoefficients(func.Coefficients, 2);

                        expr = (func.Coefficients[0] * Sym.Sqrt(T)) / (1 + func.Coefficients[1] / T);
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown function type" + typeToCreate);
            }
            return expr;
        }


        public string GetVariableNameForProperty(EvaluatedProperties id)
        {
            switch (id)
            {
                case EvaluatedProperties.HeatOfVaporization:
                    return "HVAP";
                case EvaluatedProperties.IdealGasHeatCapacity:
                    return "CPID";
                case EvaluatedProperties.LiquidDensity:
                    return "DENL";
                case EvaluatedProperties.LiquidHeatCapacity:
                    return "CL";
                case EvaluatedProperties.LiquidHeatConductivity:
                    return "KLIQ";
                case EvaluatedProperties.SurfaceTension:
                    return "ST";
                case EvaluatedProperties.VaporHeatConductivity:
                    return "KVAP";
                case EvaluatedProperties.VaporPressure:
                    return "VP";
                default:
                    throw new InvalidOperationException("Unknown property type");
            }
        }
    }
}
