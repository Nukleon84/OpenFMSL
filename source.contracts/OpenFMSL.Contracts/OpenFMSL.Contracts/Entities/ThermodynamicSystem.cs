using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class ThermodynamicSystemEntity:Entity
    {
        string _sourceCode = "# IKCAPE Neutral Input File";
        public ThermodynamicSystemEntity(string name):base()
        {
            Name = name;
            IconName = "FlaskOutline";
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
