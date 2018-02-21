using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Documents
{
    public interface IChartViewModelFactory
    {
        IChartViewModel Create(ChartModel model);
        IChartViewModel Create(Chart entity);
    }

    public interface IChartViewModel
    {
    }

    public interface IChartView
    {
    }
}
