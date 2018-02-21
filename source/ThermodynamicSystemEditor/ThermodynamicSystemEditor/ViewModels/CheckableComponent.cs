using Caliburn.Micro;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicSystemEditor.ViewModels
{
    public class CheckableComponent: PropertyChangedBase
    {
        bool _isChecked = false;
        MolecularComponent _data;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }

            set
            {
                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }

        public MolecularComponent Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
                NotifyOfPropertyChange(() => Data);
            }
        }
    }
}
