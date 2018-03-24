using OpenFMSL.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{


    public class FlowsheetEntity : Entity
    {
        Flowsheet _flowsheet = new Flowsheet("Empty");

        public FlowsheetEntity(string name) : base()
        {
            Name = name;
            IconName = "Settings";
        }

        public FlowsheetEntity(string name, Flowsheet source) : this(name)
        {
            Flowsheet = source;
        }

        public Flowsheet Flowsheet
        {
            get
            {
                return _flowsheet;
            }

            set
            {
                _flowsheet = value;
            }
        }
    }
}
