using OpenFMSL.Contracts.Entities;
using OpenFMSL.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Documents
{
    public interface IFlowsheetEntityEditorFactory
    {
        IFlowsheetEntityEditorViewModel Create(FlowsheetEntity model);
    }

    public interface IFlowsheetEntityEditorViewModel
    {
    
    }

    public interface IFlowsheetEntityEditorView
    {
    }
}
