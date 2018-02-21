using OpenFMSL.Contracts.Infrastructure.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class Chart:Entity
    {
        ChartModel _model=new ChartModel();

        public Chart(string name) : base()
        {
            Name = name;
            IconName = "ChartLine";

        }

        public ChartModel Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
            }
        }
    }
}
