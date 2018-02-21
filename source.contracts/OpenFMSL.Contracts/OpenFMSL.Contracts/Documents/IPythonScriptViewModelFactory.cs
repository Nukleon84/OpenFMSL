using OpenFMSL.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Documents
{
    public interface IPythonScriptViewModelFactory
    {
        IPythonScriptDocumentViewModel Create(Script source );
    }
}
