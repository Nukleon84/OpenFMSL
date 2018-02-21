using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenFMSL.Core.Expressions;

namespace OpenFMSL.Core.Tests.Expressions
{
    [TestClass]
    public class Functions
    {
        [TestMethod]
        public void Can_Raise_VariabletoPower2()
        {
            var x = new Variable { Name = "x", ValueInSI = 2 };

            var evaluator = new Evaluator();
            Assert.AreEqual(4, Sym.Pow(x, 2).Eval(evaluator));

        }

        [TestMethod]
        public void Can_Sqrt_4()
        {
            var x = new Variable { Name = "x", ValueInSI = 4 };
            var evaluator = new Evaluator();
            Assert.AreEqual(2, Sym.Sqrt(x).Eval(evaluator));

        }


        [TestMethod]
        public void Can_Log6()
        {
            var x = new Variable { Name = "x", ValueInSI = 6 };
            var evaluator = new Evaluator();
            Assert.AreEqual(Math.Log(6), Sym.Ln(x).Eval(evaluator));

        }

        [TestMethod]
        public void Can_Abs_Minus4()
        {
            var x = new Variable { Name = "x", ValueInSI = -4 };
            var evaluator = new Evaluator();
            Assert.AreEqual(4, Sym.Abs(x).Eval(evaluator));

        }
    }
}
