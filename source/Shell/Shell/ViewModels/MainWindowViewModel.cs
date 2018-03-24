using Caliburn.Micro;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Persistence;
using OpenFMSL.Contracts.Infrastructure.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Shell.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase, IHandle<ChangeStatusBarTextMessage>, IHandle<RequestEntityEditorMessage>, IHandle<ShowConsoleMessage>
    {
        private readonly IEventAggregator _aggregator;
        private readonly IMessageLogViewModel _messageLog;
        private readonly IDocumentEditorViewModel _documents;
        private readonly IEntityManagerViewModel _entityManager;
        private readonly IInteractiveConsoleViewModel _console;
        private readonly IPythonScriptViewModelFactory _scriptEditorFactory;
        private readonly IThermodynamicSystemViewModelFactory _thermoEditorFactory;
        private readonly ISnapshotDocumentViewModelFactory _snapshotEditorFactory;
        private readonly IChartViewModelFactory _chartFactory;
        private readonly IFlowsheetEntityEditorFactory _flowsheetFactory;

        private readonly IProjectStorage _projectStorage;
        string _currentFilename = "";
        string _statusbarText = "Application idle..";

        public IMessageLogViewModel MessageLog
        {
            get
            {
                return _messageLog;
            }


        }

        public IDocumentEditorViewModel Documents
        {
            get
            {
                return _documents;
            }


        }

        public IEntityManagerViewModel EntityManager
        {
            get
            {
                return _entityManager;
            }


        }

        public string StatusbarText
        {
            get
            {
                return _statusbarText;
            }

            set
            {
                _statusbarText = value;
                NotifyOfPropertyChange(() => StatusbarText);
            }
        }

        public string CurrentFilename
        {
            get
            {
                return _currentFilename;
            }

            set
            {
                _currentFilename = value;
                NotifyOfPropertyChange(() => CurrentFilename);
            }
        }

        public MainWindowViewModel(IEventAggregator aggregator,
            IMessageLogViewModel messageLog,
            IDocumentEditorViewModel documents,
            IEntityManagerViewModel entityManager,
            IInteractiveConsoleViewModel console,
            IPythonScriptViewModelFactory scriptEditorFactory,
            IThermodynamicSystemViewModelFactory thermoEditorFactory,
            ISnapshotDocumentViewModelFactory snapshotEditorFactory,
             IChartViewModelFactory chartFactory,
              IFlowsheetEntityEditorFactory flowsheetFactory,
            IProjectStorage projectStorage)
        {
            _console = console;
            _aggregator = aggregator;
            _messageLog = messageLog;
            _documents = documents;
            _entityManager = entityManager;
            _scriptEditorFactory = scriptEditorFactory;
            _thermoEditorFactory = thermoEditorFactory;
            _snapshotEditorFactory = snapshotEditorFactory;
            _chartFactory = chartFactory;
            _projectStorage = projectStorage;
            _flowsheetFactory = flowsheetFactory;
            _aggregator.Subscribe(this);
            _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = "Welcome to Open FMSL", Parameter = new TestDocumentViewModel() });
        }

        public void Save()
        {
            _aggregator.PublishOnUIThread(new PersistChangesMessage { TimeStamp = DateTime.Now, Sender = this });


            if (!String.IsNullOrEmpty(CurrentFilename))
            {
                _aggregator.PublishOnUIThread(new StoreEntityInRepositoryMessage { TimeStamp = DateTime.Now, Sender = this, Data = _entityManager.GetModelTree(), Filename = CurrentFilename });
                _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Information, MessageText = "Project stored in file " + CurrentFilename });
            }
            else
            {
                _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = "No filename specified" });
            }
        }

        public void SaveAs()
        {

            var filename = "";
            var result = Helper.DialogHelper.ShowSaveFileDialog(".project", "OpenFMSL Project| *.project", out filename);
            if (result)
            {
                CurrentFilename = filename;
                Save();
            }

        }

        public void Open()
        {
            var filename = "";
            var result = Helper.DialogHelper.ShowOpenFileDialog(".project", "OpenFMSL Project| *.project", out filename);
            if (result)
            {
                _documents.Clear();
                _entityManager.New();
                CurrentFilename = filename;
                _aggregator.PublishOnUIThread(new OpenRepositoryMessage { TimeStamp = DateTime.Now, Sender = this, Filename = filename });
            }
        }

        public void New()
        {
            CurrentFilename = "";
            _documents.Clear();
            _entityManager.New();
            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Information, MessageText = "New project created" });

            _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = "Welcome to Open FMSL", Parameter = new TestDocumentViewModel() });
        }

        public void StopCurrentTask()
        {
            _aggregator.PublishOnUIThread(new StopCurrentScriptMessage { TimeStamp = DateTime.Now, Sender = this });

        }

        public void ClearConsole()
        {
            _console.Clear();
        }
        public void ShowConsole()
        {
            _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = "Console", Parameter = _console });
        }
        public void Quit()
        {
            Application.Current.Shutdown();
        }

        public void Handle(ChangeStatusBarTextMessage message)
        {
            if (message != null)
            {
                StatusbarText = message.StatusBarText;
            }
        }

        public void Handle(RequestEntityEditorMessage message)
        {

            if (message.Target is Script)
            {
                var vm = _scriptEditorFactory.Create(message.Target as Script);
                _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = message.Target.Name, Parameter = vm });
            }

            if (message.Target is ThermodynamicSystemEntity)
            {
                var vm = _thermoEditorFactory.Create(message.Target as ThermodynamicSystemEntity);
                _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = message.Target.Name, Parameter = vm });
            }

            if (message.Target is Snapshot)
            {
                var vm = _snapshotEditorFactory.Create(message.Target as Snapshot);
                _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = message.Target.Name, Parameter = vm });
            }
            if (message.Target is Chart)
            {
                var vm = _chartFactory.Create(message.Target as Chart);
                _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = message.Target.Name, Parameter = vm });
            }
            if (message.Target is FlowsheetEntity)
            {
                var vm = _flowsheetFactory.Create(message.Target as FlowsheetEntity);
                _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = message.Target.Name, Parameter = vm });
            }
        }
        public void ExecuteCurrentDocument()
        {
            _aggregator.PublishOnUIThread(new RunCurrentScriptMessage { TimeStamp = DateTime.Now, Sender = this });
        }

        public void Handle(ShowConsoleMessage message)
        {
            ShowConsole();
        }
    }
}
