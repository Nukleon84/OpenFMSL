using CSparse.Double.Factorization;
using CSparse.Storage;
using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics.Solvers
{

    /// <summary>
    /// Wrapper class around CSparse.NET LU solve
    /// </summary>
    public class CSparseWrapper
    {
        public static Vector SolveLinearSystem(CompressedColumnStorage<double> A, Vector x, Vector b, out bool status, out string algorithm)
        {
            status = false;
            algorithm = "NONE";

            var result = SolveLU(A, x, b, out status, out algorithm);

            for (int i = 0; i < result.Size; i++)
            {
                if (Double.IsNaN(result[i]))
                {
                    result[i] = 0;
                }
            }

            if (!status)
            {

            }
            return result;
        }

        public static Vector SolveLU(CompressedColumnStorage<double> A, Vector x, Vector b, out bool status, out string algorithm)
        {
            var orderings = new[] { CSparse.ColumnOrdering.MinimumDegreeAtPlusA, CSparse.ColumnOrdering.MinimumDegreeAtA, CSparse.ColumnOrdering.MinimumDegreeStS, CSparse.ColumnOrdering.Natural };

            status = false;
            algorithm = "LU";
           
                try
                {
                    status = true;
                    if (A.RowCount == A.ColumnCount)
                    {
                        var lu = new SparseLU(A, CSparse.ColumnOrdering.MinimumDegreeAtA, 1.0);
                        var xc = x.Clone();
                        var bc = b.Clone();
                        lu.Solve(bc.ToDouble(), xc.ToDouble());
                        algorithm = "LU/" + CSparse.ColumnOrdering.MinimumDegreeAtPlusA;
                        status = true;
                        return xc;
                    }
                }
                catch (Exception e)
                {
                    status = false;
                }
           
            return x;

        }
        public static Vector SolveChol(CompressedColumnStorage<double> A, Vector x, Vector b, out bool status, out string algorithm)
        {
            var orderings = new[] { CSparse.ColumnOrdering.MinimumDegreeAtPlusA, CSparse.ColumnOrdering.MinimumDegreeAtA, CSparse.ColumnOrdering.MinimumDegreeStS, CSparse.ColumnOrdering.Natural };

            status = false;
            algorithm = "LU";
            foreach (var ordering in orderings)
            {
                try
                {
                    status = true;
                    if (A.RowCount == A.ColumnCount)
                    {
                        var lu = new SparseCholesky(A, ordering);
                        var xc = x.Clone();
                        var bc = b.Clone();
                        lu.Solve(bc.ToDouble(), xc.ToDouble());
                        algorithm = "CHOL/" + ordering;
                        status = true;
                        return xc;
                    }
                }
                catch (Exception e)
                {
                    status = false;
                }
            }
            return x;

        }
        public static Vector SolveQR(CompressedColumnStorage<double> A, Vector x, Vector b, out bool status, out string algorithm)
        {
            var orderings = new[] { CSparse.ColumnOrdering.MinimumDegreeAtA, CSparse.ColumnOrdering.MinimumDegreeAtPlusA, CSparse.ColumnOrdering.MinimumDegreeStS, CSparse.ColumnOrdering.Natural };
            status = false;
            algorithm = "QR";
            foreach (var ordering in orderings)
            {
                try
                {

                    if (A.RowCount == A.ColumnCount)
                    {
                        var qr = new SparseQR(A, ordering);
                        var xc = x.Clone();
                        var bc = b.Clone();
                        qr.Solve(bc.ToDouble(), xc.ToDouble());
                        algorithm = "QR/" + ordering;
                        status = true;
                        return xc;
                    }
                }
                catch (Exception e)
                {
                    status = false;
                }
            }
            return x;

        }

        public static Vector FillResiduals(EquationSystem system, Evaluator evaluator)
        {
            var b = new Vector(system.Equations.Count);
            for (int i = 0; i < system.Equations.Count; i++)
            {
                //system.Equations[i].ResetIncidenceVector();
                var val = system.Equations[i].Residual(evaluator);
                if (Double.IsNaN(val) || Double.IsInfinity(val))
                    continue;
                b[i] = val;
            }
            return b;
        }

        public static CompressedColumnStorage<double> FillJacobian(EquationSystem system, Evaluator evaluator)
        {
            var sparseJacobian = new CoordinateStorage<double>(system.NumberOfEquations, system.NumberOfVariables, system.Jacobian.Count);
            

            foreach (var entry in system.Jacobian)
            {
                var value = system.Equations[entry.EquationIndex].Diff(evaluator, system.Variables[entry.VariableIndex]);
                if (Double.IsNaN(value))
                {
                    var eq = system.Equations[entry.EquationIndex];
                    var vari = system.Variables[entry.VariableIndex];
                    var value2 = eq.Diff(evaluator, vari);
                }
                sparseJacobian.At(entry.EquationIndex, entry.VariableIndex, value);
            }
           /* foreach (var equation in system.Equations)
            {
                foreach (var variable in equation.Incidence(evaluator))
                {
                    if (!variable.IsConstant && variable.DefiningExpression == null)
                    {
                        int j = system.Variables.IndexOf(variable);
                        if (j >= 0)
                        {
                            var value = equation.Diff(evaluator, variable);
                            sparseJacobian.At(i, j, value);
                        }
                    }
                }
                i++;
            }*/
            var compJacobian = CSparse.Converter.ToCompressedColumnStorage(sparseJacobian);
            return compJacobian;
        }

        public static CompressedColumnStorage<double> ConvertSparsityJacobian(EquationSystem problem)
        {
            var sparseJacobian = new CoordinateStorage<double>(problem.NumberOfEquations, problem.NumberOfVariables, problem.Jacobian.Count);
            var eval = new Evaluator();
            foreach (var entry in problem.Jacobian)
            {

               /* var value = problem.Equations[entry.EquationIndex].Diff(eval, problem.Variables[entry.VariableIndex]);
                if(Double.IsNaN(value))
                {
                    var eq = problem.Equations[entry.EquationIndex];
                    var vari = problem.Variables[entry.VariableIndex];
                }*/

                sparseJacobian.At(entry.EquationIndex, entry.VariableIndex, 1);
            }


            /*  int i = 0;
              foreach (var equation in problem.Equations)
              {
                  foreach (var variable in equation.Incidence(new Evaluator()))
                  {
                      if (!variable.IsConstant && variable.DefiningExpression == null)
                      {
                          int j = problem.Variables.IndexOf(variable);
                          if (j >= 0)
                          {                           
                              sparseJacobian.At(i, j, 1);
                          }
                      }
                  }
                  i++;
              }*/


            var compJacobian = CSparse.Converter.ToCompressedColumnStorage(sparseJacobian);
            return compJacobian;
        }


        public static CompressedColumnStorage<double> CreateIdentity(int dim, double diagonal)
        {
            var identityMatrix = new CoordinateStorage<double>(dim, dim, dim);

            for (int i = 0; i < dim; i++)
            {
                identityMatrix.At(i, i, diagonal);
            }
            var compidentityMatrix = CSparse.Converter.ToCompressedColumnStorage(identityMatrix);
            return compidentityMatrix;
        }


        public static CompressedColumnStorage<double> CreateDiagonal(int dim, double[] diagonal)
        {
            var identityMatrix = new CoordinateStorage<double>(dim, dim, dim);

            for (int i = 0; i < dim; i++)
            {
                identityMatrix.At(i, i, diagonal[i]);
            }
            var compidentityMatrix = CSparse.Converter.ToCompressedColumnStorage(identityMatrix);
            return compidentityMatrix;
        }


        public static CompressedColumnStorage<double> CreateDiagonalInverse(int dim, double[] diagonal)
        {
            var identityMatrix = new CoordinateStorage<double>(dim, dim, dim);

            for (int i = 0; i < dim; i++)
            {
                identityMatrix.At(i, i, 1.0 / diagonal[i]);
            }
            var compidentityMatrix = CSparse.Converter.ToCompressedColumnStorage(identityMatrix);
            return compidentityMatrix;
        }

    }

}
