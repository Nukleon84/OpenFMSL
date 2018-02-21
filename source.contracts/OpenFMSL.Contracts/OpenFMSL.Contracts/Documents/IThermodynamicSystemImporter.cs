using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Documents
{
    public interface IThermodynamicSystemImporter
    {
        ThermodynamicSystem ImportPPDX(string source);
        ThermodynamicSystem ImportNeutralFile(string source);
    }
}
