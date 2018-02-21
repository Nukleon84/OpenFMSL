using OpenFMSL.Contracts.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class StoreEntityInRepositoryMessage:BaseMessage
    {
        Entity _data;
        string _filename;

        public Entity Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                _filename = value;
            }
        }
    }
}
