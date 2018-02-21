using CSparse.Ordering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics.Solvers
{
    /// <summary>
    /// Wrapper class around the CSparse.NET DulmageMendelsohn
    /// Converts the jacobian of a NLP problem into Compressed Column Storage and processes it using CSparse Dulmage Mendelsohn Decomposition
    /// </summary>

    public class DulmageMendelsohnDecomposition
    {
        public DulmageMendelsohn Generate(EquationSystem problem)
        {
            var A = CSparseWrapper.ConvertSparsityJacobian(problem);

            var dm = DulmageMendelsohn.Generate(A, 1);

            A.PermuteRows(dm.p);
            A.PermuteColumns(dm.q);

            //foreach (var value in A.EnumerateIndexed())
            //{
            //    sw.WriteLine("{0},{1}", value.Item1, value.Item2);
            //}


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Coarse Structure");
            sb.AppendLine("Underdetermined : ");
            sb.AppendLine("Determined      : ");
            sb.AppendLine("Overdetermined  : ");

            sb.AppendLine("Fine Structure");
            for (int i = dm.Blocks - 1; i >= 0; i--)
            {

                sb.AppendLine(String.Format("Block {0}: V {1} - {2} E {3} - {4}", i, dm.s[i], dm.s[i + 1] - 1,
                    dm.r[i],
                    dm.r[i + 1] - 1));

                var varcount = dm.s[i + 1] - dm.s[i];

                for (int j = 0; j < varcount; j++)
                {
                    var vari = dm.q[dm.s[i] + j];
                    sb.Append(problem.Variables[vari] + ", ");

                }

                var eqcount = dm.r[i + 1] - dm.r[i];
                for (int j = 0; j < eqcount; j++)
                {
                    var vari = dm.p[dm.r[i] + j];
                    //  sb.Append(problem.Constraints[vari] + ", ");
                }

                sb.AppendLine("");
            }
            // Console.WriteLine((sb.ToString()));

            return dm;
        }
        

    }
}
