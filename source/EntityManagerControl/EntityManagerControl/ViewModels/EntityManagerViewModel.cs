using Caliburn.Micro;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EntityManagerControl.ViewModels
{
    public class EntityManagerViewModel : PropertyChangedBase, IEntityManagerViewModel, IHandle<RestoreProjectFromRepositoryMessage>
    {
        IEventAggregator _aggregator;
        private EntityViewModel _root = new EntityViewModel(new Folder("Root"));

        private EntityViewModel _currentEntity;

        public EntityViewModel CurrentEntity
        {
            get { return _currentEntity; }
            set { _currentEntity = value; NotifyOfPropertyChange(() => CurrentEntity); }
        }

        public EntityViewModel Root
        {
            get { return _root; }
            set { _root = value; NotifyOfPropertyChange(() => Root); }
        }


        public EntityManagerViewModel(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.Subscribe(this);

            var test = new EntityViewModel(new Folder("Projects"));
            // test.AddChild(new EntityViewModel(new Script("Flash")));
            // test.AddChild(new EntityViewModel(new Script("Flowsheet")));
            Root.AddChild(test);
            Root.AddChild(new EntityViewModel(new Folder("Results")));
        }

        public void DragEnter(DragEventArgs args)
        {
            args.Handled = true;
        }

        public void Drop(ActionExecutionContext args)
        {
            var eventArgs = args.EventArgs as DragEventArgs;

            if (eventArgs != null)
            {
                if (eventArgs.Data.GetDataPresent("myDragDropFormat"))
                {
                    var droppedObject = eventArgs.Data.GetData("myDragDropFormat");

                    var dataSource = droppedObject as EntityViewModel;
                    if (dataSource != null)
                    {
                        var dropEntity = GetItemAtLocation(args.Source, eventArgs.GetPosition(args.Source));

                        if (dropEntity == null)
                            dropEntity = Root;

                        if (dropEntity != null && dropEntity != dataSource && !dataSource.Children.Contains(dropEntity) &&
                            dropEntity.IconName == "Folder")
                        {
                            dataSource.Parent.Children.Remove(dataSource);
                            dropEntity.AddChild(dataSource);
                        }
                        // AddDataSource(dataSource);
                        //Update();
                        return;
                    }
                }

                /* if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                 {
                     string[] files = (string[])eventArgs.Data.GetData(DataFormats.FileDrop);

                     var dropEntity = GetItemAtLocation(args.Source, eventArgs.GetPosition(args.Source));

                     if (dropEntity == null)
                         dropEntity = Root;

                     if (dropEntity != null && dropEntity is Folder && files != null && files.Length > 0)
                     {
                         foreach (var file in files)
                         {
                             var externalFile = new ExternalFile();
                             externalFile.Path = file;
                             externalFile.Name = System.IO.Path.GetFileName(file);
                             dropEntity.AddChild(externalFile);
                         }

                     }

                 }*/
            }
        }
        public void New()
        {
            Root.Data.Children.Clear();
            Root.Children.Clear();
            var test = new EntityViewModel(new Folder("Projects"));
            // test.AddChild(new EntityViewModel(new Script("Flash")));
            // test.AddChild(new EntityViewModel(new Script("Flowsheet")));
            Root.AddChild(test);
            Root.AddChild(new EntityViewModel(new Folder("Results")));
        }
        public void SelectItem(FrameworkElement sender)
        {
            var treeview = sender as TreeView;
            if (treeview != null)
            {
                CurrentEntity = treeview.SelectedItem as EntityViewModel;
            }
        }
        public void DeleteSelectedItem()
        {
            if (CurrentEntity != null)
            {
                if (CurrentEntity.Parent != null)
                {
                    CurrentEntity.Parent.Children.Remove(CurrentEntity);
                }
            }
        }

        EntityViewModel GetItemAtLocation(FrameworkElement treeView, Point point)
        {
            HitTestResult result = VisualTreeHelper.HitTest(treeView, point);

            FrameworkElement element = result.VisualHit as FrameworkElement;

            if (element != null)
                return element.DataContext as EntityViewModel;

            return null;
        }

        public void UnselectEntity()
        {
            if (_currentEntity != null)
                _currentEntity.IsRenaming = false;
        }


        public void ChangeActiveEntity(EntityViewModel sender)
        {
            if (_currentEntity != null)
                _currentEntity.IsRenaming = false;

            _currentEntity = sender;
        }

        public void RenameEntity()
        {
            if (_currentEntity != null)
                _currentEntity.IsRenaming = true;
        }


        public void CopyEntity()
        {
            if (_currentEntity != null)
            {

            }
            //    _currentEntity.IsRenaming = true;
        }

        public void CutEntity()
        {
            if (_currentEntity != null)
            {

            }
            //    _currentEntity.IsRenaming = true;
        }

        public void PasteEntity()
        {
            if (_currentEntity != null)
            {

            }
            //    _currentEntity.IsRenaming = true;
        }

        public void DeleteEntity()
        {
            if (_currentEntity != null)
            {
                if (CurrentEntity.Parent != null)
                {
                    CurrentEntity.Parent.Children.Remove(CurrentEntity);
                }
            }
            //    _currentEntity.IsRenaming = true;
        }


        public void AddFolder()
        {
            var folder = _currentEntity as EntityViewModel;
            if (folder != null && folder.IconName == "Folder")
            {
                folder.AddChild(new EntityViewModel(new Folder("New Folder")));

            }
        }
        public void AddScript()
        {
            var folder = _currentEntity as EntityViewModel;
            if (folder != null && folder.IconName == "Folder")
            {
                folder.AddChild(new EntityViewModel(new Script("New Script")));

            }
        }
        public void AddThermoSystem()
        {
            var folder = _currentEntity as EntityViewModel;
            if (folder != null && folder.IconName == "Folder")
            {
                folder.AddChild(new EntityViewModel(new ThermodynamicSystemEntity("New Thermo System")));

            }
        }
        public void RequestDetails(object args)
        {
            if (CurrentEntity != null)
            {
                _aggregator.PublishOnUIThread(new RequestEntityEditorMessage() { Target = CurrentEntity.Data });
            }
        }

        public Entity GetModelTree()
        {
            var node = _root.Data;
            //foreach (var child in _root.Children)
                RestoreHierarchyFromViewModel(_root.Data, Root.Children);
            return node;

        }
        void RestoreHierarchyFromViewModel(Entity node, IList<EntityViewModel> viewModelNode)
        {
            node.Children.Clear();
            foreach (var child in viewModelNode)
            {
                child.Data.Children.Clear();
                node.AddChild(child.Data);
                RestoreHierarchyFromViewModel(node.Children.Last(), child.Children);
            }
        }



        void RestoreHierarchy(EntityViewModel node, Entity data)
        {
            foreach (var child in data.Children.ToArray())
            {
                var childNode = new EntityViewModel(child);
                //childNode.Parent = node;
                //childNode.Data.Parent = node.Data;

                node.AddChild(childNode);
                RestoreHierarchy(childNode, child);
            }
        }
        public void Handle(RestoreProjectFromRepositoryMessage message)
        {
            Root.Data.Children.Clear();
            Root.Children.Clear();

            RestoreHierarchy(Root, message.RestoredData);

            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Information, MessageText = "Project restored " });
        }


        public EntityViewModel this[string path]
        {
            get
            {

                var route = SplitPath(path);

                var currentEntity = Root;

                while (route.Count > 0)
                {
                    var entityNameToFind = route.Dequeue();
                    var nextEntity = currentEntity.Children.FirstOrDefault(e => e.Name == entityNameToFind);

                    if (nextEntity != null)
                        currentEntity = nextEntity;
                    else
                    {
                        return null;
                    }
                }

                return currentEntity;
            }
        }

        public void Add(string path, Entity item)
        {
            var target = this[path];

            if (target != null)
            {
                Action<Entity> addMethod = target.AddChild;
                Application.Current.Dispatcher.Invoke(addMethod, item);
            }
        }


        Queue<string> SplitPath(string rawPath)
        {
            var result = new Queue<string>();

            if (String.IsNullOrEmpty(rawPath))
                return result;

            if (!rawPath.Contains("|"))
            {
                result.Enqueue(rawPath);
                return result;
            }

            var tokens = rawPath.Split('|');
            foreach (var token in tokens)
            {
                result.Enqueue(token);
            }
            return result;
        }

    }
}
