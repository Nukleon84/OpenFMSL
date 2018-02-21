using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Numerics.Solvers;

namespace OpenFMSL.Core.Tests.Numerics
{
    [TestClass]
    public class NewtonSolver
    {
        [TestMethod]
        public void Can_Solve_Nonlinear_System_with_3_Variables()
        {
            var problem = new EquationSystem();
            var x1 = new Variable("x1", 1);
            var x2 = new Variable("x2", 1);
            var x3 = new Variable("x3", 1);
            problem.AddVariables(x1, x2, x3);
            problem.AddConstraints(new Equation((3 * x1 - Sym.Cos(x2 * x3) - 3.0 / 2.0)));
            problem.AddConstraints(new Equation(4 * Sym.Pow(x1, 2) - 625 * Sym.Pow(x2, 2) + 2 * x2 - 1));
            problem.AddConstraints(new Equation(Sym.Exp(-x1 * x2) + 20 * x3 + (10 * Math.PI - 3.0) / 3.0));

            var solver = new Newton();            
            solver.Solve(problem);

            Assert.AreEqual(true, solver.IsConverged);
            Assert.AreEqual(8, solver.Iterations);
        }

        [TestMethod]
        public void CanSolveMinMaxProblem()
        {
            var problem = new EquationSystem();
            var x1 = new Variable("x1", 0);
            var x2 = new Variable("x2", 6);
            var x3 = new Variable("x3", 25);
            problem.AddVariables(x2);
            problem.AddConstraints((Sym.Max(x1, Sym.Min(x2, x3))).IsEqualTo(5));
            var solver = new Newton();
            solver.Solve(problem);
            Assert.AreEqual(5, x2.ValueInSI, 1e-5);
            Assert.AreEqual(true, solver.IsConverged);
        }

        [TestMethod]
        public void CanSolveMaxProblem()
        {
            var problem = new EquationSystem();
            var x1 = new Variable("x1", 10);            
            problem.AddVariables(x1);
            problem.AddConstraints((Sym.Max(x1, 5)).IsEqualTo(25));
            var solver = new Newton();
            solver.Solve(problem);
            Assert.AreEqual(25, x1.ValueInSI, 1e-5);
            Assert.AreEqual(true, solver.IsConverged);
        }

        [TestMethod]
        public void CanSolveMinProblem()
        {
            var problem = new EquationSystem();
            var x1 = new Variable("x1", 6);            
            problem.AddVariables(x1);
            problem.AddConstraints((Sym.Min(x1, 25)).IsEqualTo(5));
            var solver = new Newton();
            solver.Solve(problem);
            Assert.AreEqual(5, x1.ValueInSI, 1e-5);
            Assert.AreEqual(true, solver.IsConverged);
        }
    }


}
