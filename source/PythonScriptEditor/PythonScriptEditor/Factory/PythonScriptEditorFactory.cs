using Caliburn.Micro;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using PythonScriptEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonScriptEditor.Factory
{
    public class PythonScriptEditorFactory : IPythonScriptViewModelFactory
    {
        IEventAggregator _aggregator;

        public PythonScriptEditorFactory(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
        }
        public IPythonScriptDocumentViewModel Create(Script source)
        {
            return new PythonScriptEditorViewModel(_aggregator, source);
        }
    }
}
