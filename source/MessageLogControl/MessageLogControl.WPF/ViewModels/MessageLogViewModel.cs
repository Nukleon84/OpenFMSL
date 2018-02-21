using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using Caliburn.Micro;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using System;

namespace MessageLog.ViewModels
{
    public class MessageLogViewModel : PropertyChangedBase, IMessageLogViewModel, IHandle<LogMessage>
    {
        private readonly ObservableCollection<LogMessage> _log = new ObservableCollection<LogMessage>();
        private int _bufferSize = 50;
        private readonly IEventAggregator _aggregator;

        public MessageLogViewModel(IEventAggregator aggregator)
        {
            _aggregator = aggregator;

            _aggregator.Subscribe(this);
        }
        
        public void Clear()
        {
            _log.Clear();
        }

       
        public void Show()
        {
          //.Register<LogMessage>(LogHandler);
            //Register Load Save New Messages
            //MessageDispatcher.Register<SelectedObjectChangedMessage>(OnSelectedObjectChanged);
        }

        public void Hide()
        {
            //Unregister Load Save New Messages
            // MessageDispatcher.Unregister<SelectedObjectChangedMessage>(OnSelectedObjectChanged);
            //_aggregator.Unregister<LogMessage>(LogHandler);

        }

        #region Properties
        public IList<LogMessage> Log
        {
            get { return _log; }

        }

        public bool ShowErrors
        {
            get;
            set;
        }

        public bool ShowInformation
        {
            get;
            set;
        }

        public bool ShowWarnings
        {
            get;
            set;
        }

        public int BufferSize
        {
            get { return _bufferSize; }
            set { _bufferSize = value;  NotifyOfPropertyChange(()=>BufferSize); }
        }
        #endregion
        
        public void Handle(LogMessage message)
        {
            _log.Insert(0, message);

            if (_log.Count >= BufferSize)
            {
                _log.Remove((_log.Last()));
            }
            Trace.WriteLine("Message received");
        }
    }
}
