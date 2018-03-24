using System;

namespace FlowsheetEditorControl.Items
{
    /// <summary>
    /// Class for serialization of connectors. 
    /// </summary>    
    public class Connector : DrawableItem
    {
        #region Fields
        VisualUnit _owner;
        bool _isConnected = false;
        ConnectorIntent _intent = ConnectorIntent.Unspecified;
        ConnectorDirection _direction = ConnectorDirection.Right;
        ConnectorIconTypes _iconType = ConnectorIconTypes.Box;

        #endregion

        #region Properties
        public virtual bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; NotifyOfPropertyChange(() => IsConnected);  }
        }

        public virtual VisualUnit Owner
        {
            get { return _owner; }
            set { _owner = value; NotifyOfPropertyChange(() => Owner);  }
        }

        public ConnectorIconTypes IconType
        {
            get
            {
                return _iconType;
            }

            set
            {
                _iconType = value;
            }
        }

        public ConnectorDirection Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value; NotifyOfPropertyChange(() => Direction); 
            }
        }

        public ConnectorIntent Intent
        {
            get
            {
                return _intent;
            }

            set
            {
                _intent = value; NotifyOfPropertyChange(() => Intent);
            }
        }
        #endregion

        public Connector()
        {
            this.Width = 10;
            this.Height = 10;
        }
    }
}
