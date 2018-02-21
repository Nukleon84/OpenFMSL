using OpenFMSL.Contracts.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class RestoreProjectFromRepositoryMessage:BaseMessage
    {
        Entity _restoredData;

        public Entity RestoredData
        {
            get
            {
                return _restoredData;
            }

            set
            {
                _restoredData = value;
            }
        }
    }
}
