using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenFMSL.Core.Expressions;

namespace OpenFMSL.Core.Tests.Expressions
{
    [TestClass]
    public class Arithmetics
    {
        [TestMethod]
        public void Can_Add_3to6()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var evaluator = new Evaluator();
            Assert.AreEqual(9, (x + y).Eval(evaluator));

        }
        [TestMethod]
        public void Can_Add_3plus3to6()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var evaluator = new Evaluator();
            Assert.AreEqual(12, (y + (3 + x)).Eval(evaluator));
        }


        [TestMethod]
        public void Can_Subtract_Literals()
        {
            var evaluator = new Evaluator();
            Assert.AreEqual(3, ((Expression)(6 - 3)).Eval(evaluator));
        }

        [TestMethod]
        public void Can_Subtract_Variables()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var evaluator = new Evaluator();
            Assert.AreEqual(3, (y - x).Eval(evaluator));
            Assert.AreEqual(-3, (x - y).Eval(evaluator));
        }


        [TestMethod]
        public void Can_Subtract_VariablesAndLiterals()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var evaluator = new Evaluator();
            Assert.AreEqual(3, (9 - y).Eval(evaluator));
            Assert.AreEqual(-3, (y - 9).Eval(evaluator));

        }

        [TestMethod]
        public void Test_Expression_Multiplication()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };


            var evaluator = new Evaluator();
            Assert.AreEqual(9, (3 * x).Eval(evaluator));
            Assert.AreEqual(9, (x * 3).Eval(evaluator));
            Assert.AreEqual(18, (x * y).Eval(evaluator));
            Assert.AreEqual(18, (y * x).Eval(evaluator));

        }

        [TestMethod]
        public void Test_Expression_Division()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };


            var evaluator = new Evaluator();
            Assert.AreEqual(2, (12 / y).Eval(evaluator));
            Assert.AreEqual(3, (y / 2).Eval(evaluator));
            Assert.AreEqual(2, (y / x).Eval(evaluator));
            Assert.AreEqual(0.5, (x / y).Eval(evaluator));

        }

        [TestMethod]
        public void Test_Expression_Precedence()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var evaluator = new Evaluator();
            Assert.AreEqual(11, (3 * x + 2).Eval(evaluator));
            Assert.AreEqual(5, (y / 2 + 2).Eval(evaluator));
            Assert.AreEqual(5, (y / 2 + 2 * 3 / x).Eval(evaluator));

        }
        [TestMethod]
        public void Test_Expression_Negation()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var evaluator = new Evaluator();
            Assert.AreEqual(-3, (-x).Eval(evaluator));
            Assert.AreEqual(3, (-x + 6).Eval(evaluator));
            Assert.AreEqual(3, (-x + y).Eval(evaluator));

        }
    }
}
