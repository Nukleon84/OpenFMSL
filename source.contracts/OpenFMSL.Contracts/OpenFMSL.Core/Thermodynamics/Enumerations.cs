using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public enum FunctionType
    {
        Antoine,
        ExtendedAntoine,
        AlyLee,
        Polynomial,
        Dippr106,
        Rackett,
        Wagner,
        Watson,
        Dippr117,
        PolynomialIntegrated,
        Dippr102,
        Kirchhoff,
        Sutherland
    }


    public enum ConstantProperties
    {
        MolarWeight,
        CriticalPressure,
        CriticalTemperature,
        CriticalVolume,
        CriticalDensity,
        AcentricFactor,
        UniquacQ,
        UniquacQP,
        UniquacR,
        HeatOfFormation,
        RKSA,
        RKSB
    }

    public enum EvaluatedProperties
    {
        VaporPressure,
        IdealGasHeatCapacity,
        LiquidHeatCapacity,
        HeatOfVaporization,
        LiquidDensity,
        SurfaceTension,
        LiquidHeatConductivity,
        VaporHeatConductivity,
        LiquidViscosity,
        VaporViscosity

    }


    public enum EquationOfState
    {
        Ideal,
        RedlichKwong,
        SoaveRedlichKwong,
        PengRobinson
    }

    public enum EquilibriumApproach
    {
        GammaPhi,
        PhiPhi
    }

    public enum FugacityMethod
    {
        Ideal,
        RedlichKwong,
        SoaveRedlichKwong,
        PengRobinson
    }

    public enum ActivityMethod
    {
        Ideal,
        Wilson,
        NRTL,
        UNIQUAC,
        MODUNIQUAC,
        UNIFAC

    }

    public enum ExcessEnthalpyMethod
    {
        Ideal,
        NRTL,
        UNIQUAC,
        MODUNIFAC
    }

    public enum AllowedPhases
    {
        V,
        L,
        VLE,
        LLE,
        VLLE,
        SLE,
        SLLE        
    }

    public enum PhaseState
    {
        Liquid,
        BubblePoint,
        LiquidVapor,
        DewPoint,
        Vapour
    };
}
