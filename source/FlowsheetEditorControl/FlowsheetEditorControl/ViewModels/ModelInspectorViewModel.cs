using Caliburn.Micro;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FlowsheetEditorControl.ViewModels
{
    public class ModelInspectorViewModel : PropertyChangedBase
    {
        FlowsheetObject _model;

        public FlowsheetObject Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
                NotifyOfPropertyChange(() => Variables);
                NotifyOfPropertyChange(() => RawVariables);
            }
        }
        public List<Variable> RawVariables
        {
            get
            {
                if (Model != null)
                {
                    var collection = Model.Variables;                 
                    return collection;
                }
                else
                    return new List<Variable>();
            }
        }

        public ListCollectionView Variables
        {
            get
            {
                if (Model != null)
                {
                    var collection = new ListCollectionView(Model.Variables);
                   // collection.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
                    
                    return collection;
                }
                else
                    return new ListCollectionView(new List<Variable>());
            }
        }

        public ModelInspectorViewModel(FlowsheetObject data)
        {
            Model = data;
        }


    }
}
