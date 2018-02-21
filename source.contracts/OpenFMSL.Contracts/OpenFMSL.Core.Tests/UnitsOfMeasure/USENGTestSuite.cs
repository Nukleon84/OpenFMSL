using System;
using OpenFMSL.Core.UnitsOfMeasure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenFMSL.Core.Tests.UnitsOfMeasure
{
    [TestClass]
    public class USENGTestSuite
    {
        [TestMethod]
        public void Can_Convert_F_in_K()
        {
            var TinK = Unit.Convert(USENG.F, SI.K, 32);
            Assert.AreEqual(273.15, TinK, 1e-6);
        }

        [TestMethod]
        public void Can_Convert_K_in_F()
        {
            var TinF = Unit.Convert(SI.K, USENG.F, 273.15);
            Assert.AreEqual(32, TinF, 1e-6);
        }


        [TestMethod]
        public void Can_Convert_psi_in_bar()
        {
            var PinBar = Unit.Convert(USENG.psi, METRIC.bar, 1);
            Assert.AreEqual(0.0689476, PinBar, 1e-4);
        }

        [TestMethod]
        public void Can_Convert_bar_in_psi()
        {
            var PinPSI = Unit.Convert(METRIC.bar, USENG.psi, 1);
            Assert.AreEqual(14.50377946784675, PinPSI, 1e-4);

        }
        [TestMethod]
        public void Can_Convert_gal_in_cum()
        {
            var Vincum = Unit.Convert(USENG.USGallon, (SI.m ^ 3), 1);
            Assert.AreEqual(0.00378541, Vincum, 1e-3);            
        }

        [TestMethod]
        public void Can_Convert_cum_in_gal()
        {       
            var Vingal = Unit.Convert((SI.m ^ 3), USENG.USGallon, 1);
            Assert.AreEqual(264.171897710447354, Vingal, 1e-3);           

        }
    }
}
