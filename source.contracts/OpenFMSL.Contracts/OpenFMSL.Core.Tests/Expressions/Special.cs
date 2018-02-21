using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenFMSL.Core.Expressions;

namespace OpenFMSL.Core.Tests.Expressions
{
    [TestClass]
    public class Special
    {
        [TestMethod]
        public void Test_MinMax()
        {
            var x = new Variable { Name = "x", ValueInSI = 3 };
            var y = new Variable { Name = "y", ValueInSI = 6 };

            var eval = new Evaluator();
            Assert.AreEqual(3, Sym.Min(x, y).Eval(eval), 1e-5);
            Assert.AreEqual(6, Sym.Max(x, y).Eval(eval), 1e-5);


        }

    }
}
