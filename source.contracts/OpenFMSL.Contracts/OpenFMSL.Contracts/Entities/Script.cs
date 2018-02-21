using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class Script:Entity
    {
        string _sourceCode = "# Python Scripting Language Input File";
        public Script(string name):base()
        {
            Name = name;
            IconName = "LanguagePythonText";
        }

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
