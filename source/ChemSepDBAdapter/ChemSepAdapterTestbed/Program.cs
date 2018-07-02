using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemSepAdapterTestbed
{
    class Program
    {
        static void Main(string[] args)
        {
            var adapter = new ChemSepDBAdapter.Adapter();
            var comp = adapter.FindComponent("Ethanol");
            Console.WriteLine(comp.Name + "["+comp.CasNumber+"]");
            Console.ReadLine();
        }
    }
}
