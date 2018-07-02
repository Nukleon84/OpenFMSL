using OpenFMSL.Core.Thermodynamics;
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
            var sys = new ThermodynamicSystem("Test", "NRTL", "default");

            var adapter = new ChemSepDBAdapter.Adapter();
            adapter.SetLogCallback(Console.Write);

            var comp1 = adapter.FindComponent("Ethanol");
            var comp2 = adapter.FindComponent("Water");

            sys.AddComponent(comp1);
            sys.AddComponent(comp2);

            adapter.FillBIPs(sys);

            Console.WriteLine(comp1.Name + "["+comp1.CasNumber+"]");
            Console.WriteLine(comp2.Name + "[" + comp2.CasNumber + "]");
            Console.ReadLine();
        }
    }
}
