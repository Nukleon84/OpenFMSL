using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class RequestEntityEditorMessage
    {
        Entity _target;

        public Entity Target
        {
            get
            {
                return _target;
            }

            set
            {
                _target = value;
            }
        }
    }
}
