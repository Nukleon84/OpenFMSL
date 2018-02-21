using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Core.Thermodynamics;
using Caliburn.Micro;

namespace ThermodynamicsImporter
{
    public class Importer : IThermodynamicSystemImporter
    {
        private readonly IEventAggregator _aggregator;

        public Importer(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        public ThermodynamicSystem ImportNeutralFile(string source)
        {
            return ImportNeutralFile(source, 0);
        }

        public ThermodynamicSystem ImportNeutralFile(string source, int index)
        {
            FileImporter importer = new FileImporter(_aggregator);
            return importer.ImportSystem(source, 0);
            
        }

        public ThermodynamicSystem ImportPPDX(string source)
        {
            throw new NotImplementedException();
        }
    }
}
