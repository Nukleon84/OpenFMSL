using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Documents
{
    public interface IPythonScriptDocumentViewModel
    {
        string SourceCode { get; }
    }
    public interface IPythonScriptDocumentView
    {
    }
}
