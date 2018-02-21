using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenFMSL.Core.Expressions;

namespace OpenFMSL.Core.Tests.Expressions
{
    [TestClass]
    public class Differentiation
    {
        [TestMethod]
        public void Can_Differentiate_Sums()
        {

            var x = new Variable { Name = "x", ValueInSI = 3 };
            var evaluator = new Evaluator();
            Assert.AreEqual(1, (3 + x).Diff(evaluator, x));
        }

        [TestMethod]
        public void Can_Differentiate_Differences()
        {

            var x = new Variable { Name = "x", ValueInSI = 3 };
            var evaluator = new Evaluator();
            Assert.AreEqual(-1, (3 - x).Diff(evaluator, x));
        }

        [TestMethod]
        public void Can_Differentiate_Products()
        {

            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };
            var evaluator = new Evaluator();
            Assert.AreEqual(6, (x * y).Diff(evaluator, x));
            Assert.AreEqual(3, (x * y).Diff(evaluator, y));
        }

        [TestMethod]
        public void Can_Differentiate_Negative_Var()
        {
            var x = new Variable { Name = "x", ValueInSI = -3 };
            var evaluator = new Evaluator();
            Assert.AreEqual(1, x.Diff(evaluator, x));
        }

        [TestMethod]
        public void Can_Differentiate_Abs()
        {

            var x = new Variable { Name = "x", ValueInSI = -3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };
            var evaluator = new Evaluator();
            Assert.AreEqual(6, Sym.Abs(x*y).Diff(evaluator, x));
            Assert.AreEqual(3, Sym.Abs(x * y).Diff(evaluator, y));
        }
    }
}
