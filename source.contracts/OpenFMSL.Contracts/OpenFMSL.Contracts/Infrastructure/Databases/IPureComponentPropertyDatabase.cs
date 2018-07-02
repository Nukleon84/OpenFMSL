using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Infrastructure.Databases
{
    public interface IPureComponentPropertyDatabase
    {
        
        MolecularComponent FindComponent(string name);
    }
}
