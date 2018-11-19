using OpenFMSL.Contracts.Entities;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Documents
{
    public interface IThermodynamicSystemViewModelFactory
    {
        IThermodynamicSystemViewModel Create(ThermodynamicSystemEntity source);
        IThermodynamicSystemViewModel Create(ThermodynamicSystem source);
    }
}
