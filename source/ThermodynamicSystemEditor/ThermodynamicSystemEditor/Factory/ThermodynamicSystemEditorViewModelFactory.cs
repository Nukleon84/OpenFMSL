using Caliburn.Micro;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermodynamicSystemEditor.ViewModels;

namespace ThermodynamicSystemEditor.Factory
{

    public class ThermodynamicSystemEditorViewModelFactory : IThermodynamicSystemViewModelFactory
    {
        private readonly IEventAggregator _aggregator;
        private readonly IThermodynamicSystemImporter _importer;
        private readonly IChartViewModelFactory _chartFactory;
        public ThermodynamicSystemEditorViewModelFactory(IEventAggregator aggregator, IThermodynamicSystemImporter importer, IChartViewModelFactory chartFactory)
        {
            _aggregator = aggregator;
            _importer = importer;
            _chartFactory = chartFactory;
        }
        public IThermodynamicSystemViewModel Create(ThermodynamicSystemEntity source)
        {
            return new ThermodynamicSystemEditorViewModel(_aggregator, source, _importer, _chartFactory);
        }

        public IThermodynamicSystemViewModel Create(ThermodynamicSystem source)
        {
            return new ThermodynamicSystemEditorViewModel(_aggregator, source, _importer, _chartFactory);
        }
    }
}
