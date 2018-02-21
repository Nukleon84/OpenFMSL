using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Infrastructure.Messaging
{
    public class ChangeStatusBarTextMessage:BaseMessage
    {
        string _statusBarText="";

        public string StatusBarText
        {
            get
            {
                return _statusBarText;
            }

            set
            {
                _statusBarText = value;
            }
        }
    }
}
