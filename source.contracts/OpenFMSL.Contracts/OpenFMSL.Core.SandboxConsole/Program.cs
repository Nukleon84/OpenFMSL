using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Numerics.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.SandboxConsole
{
    class Program
    {
        /*Jacobian Density = 88,889%
Hessian Density = 0,000%
Solving NLP Problem(3 Variables, 3 Equations)
ITER SCALE          D_NORM INF_PR ALG
0001               1             1.6                  620  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0002               1            0.48             153.0758  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0003               1            0.12            38.338978  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0004               1           0.055            9.1587665  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0005               1            0.02            1.9173079  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0006               1          0.0035           0.24865722  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0007               1         0.00011         0.0076250521  ___(NEWTON, LU/MinimumDegreeAtPlusA)
0008               1         1.2E-07         8.137733E-06  ___(NEWTON, LU/MinimumDegreeAtPlusA)
[NewtonSolver]
        Problem NLP Problem was successfully solved because constraint violation is below tolerance(8 iterations, 0,04 seconds, problem size: 3)
### Printing debugging information NLP Problem
x1                             =              0,833196581863439
x2                             =             0,0549436583091183
x3                             =             -0,521361434378159

                                Generic               0,0000 ( ((4*(x1)^(2) - 625*(x2)^(2)) + 2*x2 - 1) == 0 )
                                Generic               0,0000 ( exp(-(x1)*x2) + 20*x3 + 9.471976 == 0 )
                                Generic               0,0000 ( ((3*x1 - cos(x2* x3)) - 1.5) == 0 )*/
        static void Test1()
        {
            Console.WriteLine();
            Console.WriteLine("### Test Problem:  Difficult Function");
            
            var problem = new EquationSystem();
            var x1 = new Variable("x1", 1);
            var x2 = new Variable("x2", 1);
            var x3 = new Variable("x3", 1);
            problem.AddVariables(x1, x2, x3);
            problem.AddConstraints(new Equation((3 * x1 - Sym.Cos(x2 * x3) - 3.0 / 2.0)));
            problem.AddConstraints(new Equation(4 * Sym.Pow(x1, 2) - 625 * Sym.Pow(x2, 2) + 2 * x2 - 1));
            problem.AddConstraints(new Equation(Sym.Exp(-x1 * x2) + 20 * x3 + (10 * Math.PI - 3.0) / 3.0));
                     
            var solver = new Newton();
            solver.OnLog += Console.WriteLine;
            solver.OnLogError += Console.WriteLine;
            solver.OnLogSuccess += Console.WriteLine;                
            solver.Solve(problem);
        }

        static void Test2()
        {
            Console.WriteLine();
            Console.WriteLine("### Test Problem:  Ill-conditioned at solution");

            var problem = new EquationSystem();
            var x1 = new Variable("x", 2);

            problem.AddVariables(x1);
            problem.AddConstraints(new Equation(Sym.Pow(x1, 3)));

            //problem.OnLog += Console.WriteLine;
            var solver = new Newton();
            solver.OnLog += Console.WriteLine;
            solver.OnLogDebug += Console.WriteLine;
            solver.OnLogError += Console.WriteLine;
            solver.OnLogSuccess += Console.WriteLine;

            solver.Solve(problem);
                        

        }
        static void Test3()
        {
            Console.WriteLine();
            Console.WriteLine("### Test Problem:  Well-conditioned at solution");

            var problem = new EquationSystem();
            var x1 = new Variable("x", 2);

            problem.AddVariables(x1);
            problem.AddConstraints(new Equation(Sym.Pow(x1, 3)-1));

            //problem.OnLog += Console.WriteLine;
            var solver = new Newton();
            solver.OnLog += Console.WriteLine;
            solver.OnLogDebug += Console.WriteLine;
            solver.OnLogError += Console.WriteLine;
            solver.OnLogSuccess += Console.WriteLine;

            solver.Solve(problem);


        }

        static void Test4()
        {
            Console.WriteLine();
            Console.WriteLine("### Test Problem: Powell Badly Scaled Function");
            var problem = new EquationSystem();
            var x1 = new Variable("x1", 2);
            var x2 = new Variable("x2", 1);

            problem.AddVariables(x1, x2);
            problem.AddConstraints(new Equation(10000 * x1 * x2 - 1));
            problem.AddConstraints(new Equation(Sym.Exp(-x1) + Sym.Exp(-x2) - 1.0001));

            //   
            var solver = new Newton();
            solver.OnLog += Console.WriteLine;
            solver.OnLogDebug += Console.WriteLine;
            solver.OnLogError += Console.WriteLine;
            solver.OnLogSuccess += Console.WriteLine;
            solver.Solve(problem);      

        }


         static void TestHS71()
        {
            Console.WriteLine();
            Console.WriteLine("### Test Problem: Hock-Schittkowski #71");
            var problem = new OptimizationProblem();
            var x1 = new Variable("x1", 1,1,5);
            var x2 = new Variable("x2", 2, 1, 5);
            var x3 = new Variable("x3", 5, 1, 5);
            var x4 = new Variable("x4", 1, 1, 5);

            problem.AddVariables(x1, x2,x3,x4);

            problem.ObjectiveFunction = x1 * x4 * Sym.Par(x1 + x2 + x3) + x3;
            problem.AddInequalityConstraints(new Constraint(x1*x2*x3*x4, ConstraintComparisonOperator.GreaterThanOrEqual,25));
            problem.AddConstraints((Sym.Pow(x1,2)+ Sym.Pow(x2, 2)+ Sym.Pow(x3, 2)+ Sym.Pow(x4, 2)).IsEqualTo(40) ) ;

            //   
            var solver = new IpoptSolver();
            solver.OnLog += Console.WriteLine;
           
            solver.Solve(problem);

            Console.WriteLine(x1.WriteReport());
            Console.WriteLine(x2.WriteReport());
            Console.WriteLine(x3.WriteReport());
            Console.WriteLine(x4.WriteReport());
        }
        static void Main(string[] args)
        {
            /*Test1();
            Test2();
            Test3();
            Test4();*/

            TestHS71();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
