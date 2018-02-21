using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonScriptEditor.ViewModels
{
    public class PythonScriptEditorViewModel : PropertyChangedBase, IPythonScriptDocumentViewModel, IHandle<PersistChangesMessage>
    {
        private readonly IEventAggregator _aggregator;
        private readonly Script _source;
        TextDocument _scriptDocument;
        public PythonScriptEditorViewModel(IEventAggregator aggregator, Script source)
        {
            _aggregator = aggregator;
            _source = source;
            _aggregator.Subscribe(this);
            ScriptDocument = new TextDocument(_source.SourceCode);
        }

        public Script Source
        {
            get
            {
                return _source;
            }
        }

        public string SourceCode
        {
            get
            {
                return ScriptDocument.Text;
            }


        }

        public TextDocument ScriptDocument
        {
            get
            {
                return _scriptDocument;
            }

            set
            {
                _scriptDocument = value;
                NotifyOfPropertyChange(() => ScriptDocument);
            }
        }

        public void Handle(PersistChangesMessage message)
        {
            Source.SourceCode = ScriptDocument.Text;
        }
    }
}
