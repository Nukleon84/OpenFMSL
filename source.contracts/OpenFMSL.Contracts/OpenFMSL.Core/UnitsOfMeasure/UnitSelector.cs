using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.UnitsOfMeasure
{
    public static class UnitSelector
    {
        public static Dictionary<PhysicalDimension, List<Unit>> AvailableUnitsDictionary = new Dictionary<PhysicalDimension, List<Unit>>
        {
            { PhysicalDimension.Temperature,new List<Unit>{SI.K, METRIC.C, USENG.F} },
            { PhysicalDimension.Pressure,   new List<Unit>{SI.Pa, SI.kPa, METRIC.bar, METRIC.mbar, USENG.psi} },
            { PhysicalDimension.MassFlow,   new List<Unit>{SI.kg/SI.s, SI.kg / SI.min, SI.kg / SI.h, METRIC.ton / SI.s, METRIC.ton / SI.min, METRIC.ton / SI.h } },
            { PhysicalDimension.MolarFlow,  new List<Unit>{SI.mol/SI.s, SI.kmol / SI.min, SI.kmol / SI.h } },
            { PhysicalDimension.HeatFlow,   new List<Unit>{SI.J/SI.s, SI.W, SI.kW, SI.MW } }
        };
    }
}
