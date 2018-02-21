using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SnapshotEditor.ViewModels
{
    public class SnapshotEditorViewModel:ISnapshotDocumentViewModel
    {
        Snapshot _data;


        ListCollectionView _variables;
        ListCollectionView _expressions;
        ListCollectionView _equations;

        public Snapshot Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }

        public ListCollectionView Variables
        {
            get
            {
                return _variables;
            }

            set
            {
                _variables = value;
            }
        }

        public ListCollectionView Expressions
        {
            get
            {
                return _expressions;
            }

            set
            {
                _expressions = value;
            }
        }

        public ListCollectionView Equations
        {
            get
            {
                return _equations;
            }

            set
            {
                _equations = value;
            }
        }
    }
}
