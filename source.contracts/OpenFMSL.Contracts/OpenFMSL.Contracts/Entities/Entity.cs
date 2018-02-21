using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Contracts.Entities
{
    public class Entity
    {
        private Guid _id;
        private string _name;
        Entity _parent = null;
        private IList<Entity> _children = new List<Entity>();        
        private bool _isExpanded = true;
        private bool _isRenaming = false;
        private string _iconName = "Entity";

        public Entity()
        {
            Id = new Guid();
        }

        public Guid Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public IList<Entity> Children
        {
            get
            {
                return _children;
            }

            set
            {
                _children = value;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }

            set
            {
                _isExpanded = value;
            }
        }

        public string IconName
        {
            get
            {
                return _iconName;
            }

            set
            {
                _iconName = value;
            }
        }

        public bool IsRenaming
        {
            get
            {
                return _isRenaming;
            }

            set
            {
                _isRenaming = value;
            }
        }

        public Entity Parent
        {
            get
            {
                return _parent;
            }

            set
            {
                _parent = value;
            }
        }

        public void AddChild(Entity child)
        {
            child.Parent = this;
            Children.Add(child);

        }
    }
}
