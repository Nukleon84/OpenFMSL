using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InteractiveConsoleControl.ViewModels
{


    public class InteractiveConsoleViewModel : PropertyChangedBase, IInteractiveConsoleViewModel, IHandle<StopCurrentScriptMessage>, IHandle<ExecuteSourceCodeMessage>
    {
        private readonly IPythonEnvironment _pythonEnvironment;
        private readonly IEventAggregator _aggregator;

        private CancellationTokenSource _tokenSource;

        string _currentCommand = "";
        TextDocument _history;
        System.Action _onSimulationCompleted;
        System.Action _onSimulationAborted;

        public string CurrentCommand
        {
            get
            {
                return _currentCommand;
            }

            set
            {
                _currentCommand = value;
                NotifyOfPropertyChange(() => CurrentCommand);
            }
        }

        public TextDocument History
        {
            get
            {
                return _history;
            }

            set
            {
                _history = value;
                NotifyOfPropertyChange(() => History);
            }
        }

        public System.Action OnSimulationCompleted
        {
            get
            {
                return _onSimulationCompleted;
            }

            set
            {
                _onSimulationCompleted = value;
            }
        }

        public System.Action OnSimulationAborted
        {
            get
            {
                return _onSimulationAborted;
            }

            set
            {
                _onSimulationAborted = value;
            }
        }

        public InteractiveConsoleViewModel(IEventAggregator aggregator, IPythonEnvironment pythonEnvironment)
        {
            _aggregator = aggregator;
            _pythonEnvironment = pythonEnvironment;

            _pythonEnvironment.OnWrite += AddToHistory;
            History = new TextDocument();
            _aggregator.Subscribe(this);
        }

        public void Clear()
        {
            History = new TextDocument();
        }

        public void AbortSimulation()
        {
            if (_tokenSource != null)
                _tokenSource.Cancel();
        }

        public void ExecuteCurrentCommand()
        {
            //CommandHistory.Add(CurrentCommand);
            ExecuteString(CurrentCommand);
            CurrentCommand = "";
        }


        public void ExecuteString(string sourceCode)
        {
            // CommandHistory.Add(sourceCode);

            _tokenSource = new CancellationTokenSource();

            var task = Task.Factory.StartNew(() =>
            SimulateInBackground(_tokenSource.Token, sourceCode), _tokenSource.Token)
            .ContinueWith((t) => TaskAborted(t), TaskContinuationOptions.OnlyOnCanceled)
            .ContinueWith(t => TaskFinished(t));


        }
        public object GetObject(string name)
        {
            return _pythonEnvironment.GetObject(name);

        }

        public void InjectObject(string name, object instance)
        {
            _pythonEnvironment.InjectObject(name, instance);
        }

        public void TextInputCommand(KeyEventArgs args)
        {
            if (args != null && args.Key == Key.Enter)
            {
                ExecuteCurrentCommand();
            }
            if (args != null && args.Key == Key.Back)
            {
                if (CurrentCommand.Length > 0)
                    CurrentCommand = CurrentCommand.Substring(0, CurrentCommand.Length - 1);
            }
        }
        public void TextInputHistory(TextCompositionEventArgs args)
        {
            if (args != null)
            {
                CurrentCommand += args.Text;
            }
        }

        void AddToHistory(string message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                var formattedLine = message;
                Application.Current.Dispatcher.BeginInvoke(
                    new System.Action(() => History.Insert(History.TextLength, formattedLine)));
                NotifyOfPropertyChange(() => History);
            }

        }

        void TaskAborted(Task t)
        {
            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = "Script execution was aborted by user" });
            if (OnSimulationAborted != null)
                OnSimulationAborted();
        }
        void TaskFinished(Task t)
        {

            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = "Script execution finished successfully" });

            if (OnSimulationCompleted != null)
                OnSimulationCompleted();
        }
        void SimulateInBackground(CancellationToken token, string sourceCode)
        {
            Func<bool> check = ()=> token.IsCancellationRequested;

            InjectObject("IsAbortRequested", check);
            _pythonEnvironment.Run(sourceCode);
        }
        public void Handle(StopCurrentScriptMessage message)
        {
            if (_tokenSource != null)
                _tokenSource.Cancel();
        }

        public void Handle(ExecuteSourceCodeMessage message)
        {
            ExecuteString(message.SourceCode);
        }
    }
}
