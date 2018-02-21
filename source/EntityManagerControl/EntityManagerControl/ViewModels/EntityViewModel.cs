using Caliburn.Micro;
using OpenFMSL.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityManagerControl.ViewModels
{
    public class EntityViewModel : PropertyChangedBase
    {
        private readonly Entity _data;
        EntityViewModel _parent;
        private IList<EntityViewModel> _children = new ObservableCollection<EntityViewModel>();

        public EntityViewModel(Entity data)
        {
            _data = data;
        }

        public Guid Id
        {
            get
            {
                return Data.Id;
            }
        }

        public string Name
        {
            get
            {
                return Data.Name;
            }

            set
            {
                Data.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public IList<EntityViewModel> Children
        {
            get
            {
                return _children;
            }

            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }

        public bool IsExpanded
        {
            get
            {
                return Data.IsExpanded;
            }

            set
            {
                Data.IsExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded);
            }
        }

        public string IconName
        {
            get
            {
                return Data.IconName;
            }

            set
            {
                Data.IconName = value;
                NotifyOfPropertyChange(() => IconName);
            }
        }

        public bool IsRenaming
        {
            get
            {
                return Data.IsRenaming;
            }

            set
            {
                Data.IsRenaming = value;
                NotifyOfPropertyChange(()=>IsRenaming);
            }
        }



        public EntityViewModel Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                _parent = value;
               /* if (value != null && value.Data != null)
                    Data.Parent = value.Data.Parent;*/
                NotifyOfPropertyChange(() => Parent);
            }
        }

        public Entity Data
        {
            get
            {
                return _data;
            }
        }

        public void AddChild(Entity data)
        {
            var child = new EntityViewModel(data);
            child.Parent = this;
            // Data.AddChild(child.Data);
            Children.Add(child);

        }


        public void AddChild(EntityViewModel child)
        {
            child.Parent = this;
           // Data.AddChild(child.Data);
            Children.Add(child);

        }

       
    }
}
