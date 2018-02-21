using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Infrastructure.Scripting
{
    public interface IPythonEnvironment
    {
        void Run(string sourceCode);
        void InjectObject(string name, object instance);
        object GetObject(string name);

        Action<string> OnWrite { get; set; }
    }
}
