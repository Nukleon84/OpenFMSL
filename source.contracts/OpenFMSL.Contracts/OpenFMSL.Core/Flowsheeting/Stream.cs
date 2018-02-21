using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Flowsheeting
{
    public class Stream:FlowsheetObject
    {
        public Stream():base()
        {

        }

        public Stream(string name, Thermodynamics.ThermodynamicSystem system):base(name, system)
        {

        }

    }
}
