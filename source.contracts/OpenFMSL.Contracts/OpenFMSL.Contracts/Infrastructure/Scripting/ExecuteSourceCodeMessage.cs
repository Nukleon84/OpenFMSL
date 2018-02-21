using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Infrastructure.Scripting
{
    public class ExecuteSourceCodeMessage
    {
        string _sourceCode;

        public string SourceCode
        {
            get
            {
                return _sourceCode;
            }

            set
            {
                _sourceCode = value;
            }
        }
    }
}
