using OpenFMSL.Core.UnitsOfMeasure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenFMSL.Core.UnitsOfMeasurements.Test
{
    [TestClass]
    public class UnitOfMeasureTest
    {
       
        [TestMethod]
        public void Test_UOM_ObjectCreation()
        {
            Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
            Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
            Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
            Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
            Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });
            Unit dollar = new Unit("$", "US-Dollar", new double[] { 0, 0, 0, 0, 0, 0, 0, 1 });

            Assert.AreEqual("Meter", m.Name);
            Assert.AreEqual("Kilogram", kg.Name);
            Assert.AreEqual("Second", s.Name);
            Assert.AreEqual("Kelvin", K.Name);
            Assert.AreEqual("Mol", mol.Name);
            Assert.AreEqual("US-Dollar", dollar.Name);

            Assert.AreEqual("m", m.Symbol);
            Assert.AreEqual("kg", kg.Symbol);
            Assert.AreEqual("s", s.Symbol);
            Assert.AreEqual("K", K.Symbol);
            Assert.AreEqual("mol", mol.Symbol);
            Assert.AreEqual("$", dollar.Symbol);
        }

        [TestMethod]
        public void Test_UOM_ObjectCreation_Derived()
        {
            Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
            Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
            Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
            Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
            Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });
            Unit dollar = new Unit("$", "US-Dollar", new double[] { 0, 0, 0, 0, 0, 0, 0, 1 });

            Unit min = new Unit("min", "Minutes", s, 60, 0);
            Unit h = new Unit("h", "Hour", min, 60, 0);

            Assert.AreEqual(60, min.Factor);
            Assert.AreEqual(60 * 60, h.Factor);
        }

        [TestMethod]
        public void Test_UOM_ObjectCreation_Complex()
        {
            Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
            Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
            Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
            Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
            Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });
            Unit dollar = new Unit("$", "US-Dollar", new double[] { 0, 0, 0, 0, 0, 0, 0, 1 });

            Unit min = new Unit("min", "Minutes", s, 60, 0);
            Unit h = new Unit("h", "Hour", min, 60, 0);

            Unit N = new Unit("NumberOfVariables", "Newton", kg * m / (s ^ 2));

            CollectionAssert.AreEqual(new double[] { 1, 1, -2, 0, 0, 0, 0, 0 }, N.Dimensions);

        }

        [TestMethod]
        public void Test_UOM_ObjectCreation_Conversion()
        {
            Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
            Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
            Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
            Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
            Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });
            Unit dollar = new Unit("$", "US-Dollar", new double[] { 0, 0, 0, 0, 0, 0, 0, 1 });


            Unit g = new Unit("g", "Gram", kg, 1e-3, 0);
            Unit min = new Unit("min", "Minutes", s, 60, 0);
            Unit h = new Unit("h", "Hour", min, 60, 0);
            Unit N = new Unit("NumberOfVariables", "Newton", kg * m / (s ^ 2));
            Unit C = new Unit("°C", "Centigrade", K, 1, 273.15);

            Assert.AreEqual(373.15, Unit.Convert(C, K, 100));
            Assert.AreEqual(100, Unit.Convert(K, C, 373.15));



        }

        [TestMethod]
        public void Test_UOM_ObjectCreation_CompareIntermediateUnits()
        {
            Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
            Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
            Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
            Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
            Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });
            Unit dollar = new Unit("$", "US-Dollar", new double[] { 0, 0, 0, 0, 0, 0, 0, 1 });


            Unit g = new Unit("g", "Gram", kg, 1e-3, 0);
            Unit min = new Unit("min", "Minutes", s, 60, 0);
            Unit h = new Unit("h", "Hour", min, 60, 0);
            Unit N = new Unit("NumberOfVariables", "Newton", kg * m / (s ^ 2));
            Unit C = new Unit("°C", "Centigrade", K, 1, 273.15);

            //R is the result of a calculation
            Unit r = new Unit("?", "Result", g * m / (s ^ 2));


            Assert.AreEqual(1e-3, Unit.Convert(r, N, 1));
            Assert.IsTrue(Unit.AreSameDimension(N, r));
            Assert.IsFalse(Unit.AreEquivalent(N, r));
        }

        [TestMethod]
        public void Test_UOM_ObjectCreation_DerivedUnitsFromSI()
        {

            Unit g = new Unit("g", "Gram", SI.kg, 1e-3, 0);
            Unit min = new Unit("min", "Minutes", SI.s, 60, 0);
            Unit h = new Unit("h", "Hour", min, 60, 0);
            Unit N = new Unit("NumberOfVariables", "Newton", SI.kg * SI.m / (SI.s ^ 2));
            Unit C = new Unit("°C", "Centigrade", SI.K, 1, 273.15);

            //R is the result of a calculation
            Unit r = g * SI.m / (SI.s ^ 2);

            Assert.AreEqual("g*m/s^2", (g * SI.m / (SI.s ^ 2)).Symbol);

            Assert.AreEqual(1e-3, Unit.Convert(r, N, 1));
            Assert.IsTrue(Unit.AreSameDimension(N, r));
            Assert.IsFalse(Unit.AreEquivalent(N, r));


        }

        [TestMethod]
        public void Test_UOM_ObjectCreation_DerivedUnitsWithPrefix()
        {
            Unit g = new Unit("g", "Gram", SI.kg, 1e-3, 0);

            Unit milli = new Unit("m", "Centi", 1e-3);
            Unit centi = new Unit("c", "Centi", 1e2);
            Unit kilo = new Unit("k", "Kilo", 1e3);
            Unit mega = new Unit("NumberOfEquations", "Mega", 1e6);
            //R is the result of a calculation

            Assert.AreEqual(1e-3, Unit.Convert(g, SI.kg, 1));

            var temp = kilo * g;
            Assert.AreEqual("kg", temp.Symbol);



            Assert.IsTrue(Unit.AreSameDimension(g, SI.kg));
            Assert.IsTrue(Unit.AreSameDimension(g, kilo * g));
            Assert.IsTrue(Unit.AreEquivalent(SI.kg, kilo * g));
            Assert.IsTrue(Unit.AreEquivalent(milli * SI.kg, g));
            Assert.IsFalse(Unit.AreEquivalent(SI.kg, g));
        }


        [TestMethod]
        public void Test_UOM_PrintDimensions()
        {
            Unit m = new Unit("m", "Meter", new double[] { 1, 0, 0, 0, 0, 0, 0, 0 });
            Unit kg = new Unit("kg", "Kilogram", new double[] { 0, 1, 0, 0, 0, 0, 0, 0 });
            Unit s = new Unit("s", "Second", new double[] { 0, 0, 1, 0, 0, 0, 0, 0 });
            Unit K = new Unit("K", "Kelvin", new double[] { 0, 0, 0, 0, 1, 0, 0, 0 });
            Unit mol = new Unit("mol", "Mol", new double[] { 0, 0, 0, 0, 0, 1, 0, 0 });
            Unit dollar = new Unit("$", "US-Dollar", new double[] { 0, 0, 0, 0, 0, 0, 0, 1 });

            Unit min = new Unit("min", "Minutes", s, 60, 0);
            Unit h = new Unit("h", "Hour", min, 60, 0);

            Unit N = new Unit("N", "Newton", kg * m / (s ^ 2));

            Assert.AreEqual("L M t^-2", N.PrintDimensions());
            Assert.AreEqual("m kg / (s^2)", N.PrintBaseUnits());

        }

    }
}
