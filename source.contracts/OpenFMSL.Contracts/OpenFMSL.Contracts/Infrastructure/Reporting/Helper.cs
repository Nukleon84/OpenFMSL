using System.Collections.Generic;

namespace OpenFMSL.Contracts.Infrastructure.Reporting
{
    public static class Helper
    {
        public static List<double> GetListFromTo(double from, double to, int steps)
        {
            var list = new List<double>();

            for (int i = 0; i < steps; i++)
            {
                list.Add(from + i / ((double)steps - 1) * (to - from));
            }

            return list;

        }


        public static List<double> GetListByCount(int steps, bool startAtOne = false)
        {
            var list = new List<double>();

            for (int i = startAtOne ? 1 : 0; i < steps + (startAtOne ? 1 : 0); i++)
            {
                list.Add(i);
            }

            return list;

        }
    }
}
