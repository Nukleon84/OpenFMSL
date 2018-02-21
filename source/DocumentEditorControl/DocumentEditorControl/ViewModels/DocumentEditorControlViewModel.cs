using Caliburn.Micro;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentEditorControl.ViewModels
{
    public class DocumentEditorControlViewModel : Conductor<IScreen>.Collection.OneActive, IDocumentEditorViewModel, IHandle<AddNewDocumentMessage>, IHandle<RunCurrentScriptMessage>
    {
        private readonly IEventAggregator _aggregator;

        public DocumentEditorControlViewModel(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.Subscribe(this);
        }

        public void Clear()
        {
            foreach (var item in Items.ToArray())
                DeactivateItem(item, true);
        }
        public void Handle(AddNewDocumentMessage message)
        {
            if (Items.OfType<TabViewModel>().Any(i => i.Content == message.Parameter))
            {
                ActivateItem(Items.OfType<TabViewModel>().First(i => i.Content == message.Parameter));
                return;
            }

            ActivateItem(new TabViewModel(message.Title, message.Parameter));

        }




        public void CloseItem(IScreen context)
        {
            DeactivateItem(context, true);
        }

        public void Handle(RunCurrentScriptMessage message)
        {
            var tab = ActiveItem as TabViewModel;
            if (tab != null && tab.Content is IPythonScriptDocumentViewModel)
            {
                var editor = tab.Content as IPythonScriptDocumentViewModel;
                var code = editor.SourceCode;
                _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = "Execution started for script "+ tab.DisplayName });

                _aggregator.PublishOnUIThread(new ShowConsoleMessage());
                _aggregator.PublishOnUIThread(new ExecuteSourceCodeMessage() { SourceCode = code });

            }
        }
    }
}
