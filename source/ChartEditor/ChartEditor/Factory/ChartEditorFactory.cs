using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Reporting;
using ChartEditor.ViewModels;

namespace ChartEditor.Factory
{
    public class ChartEditorFactory : IChartViewModelFactory
    {
        public IChartViewModel Create(Chart entity)
        {
            return Create(entity.Model);
        }

        public IChartViewModel Create(ChartModel model)
        {
            return new ChartEditorViewModel(model);
        }
    }
}
