using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Contracts.Entities;
using SnapshotEditor.ViewModels;
using System.Windows.Data;

namespace SnapshotEditor.Factory
{
    public class SnapshotEditorViewModelFactory : ISnapshotDocumentViewModelFactory
    {
        public ISnapshotDocumentViewModel Create(Snapshot source)
        {
            var vm = new SnapshotEditorViewModel();

      

            vm.Variables = new ListCollectionView(source.Variables);
            vm.Variables.GroupDescriptions.Add(new PropertyGroupDescription("ModelName"));

            vm.Expressions = new ListCollectionView(source.Expressions);
            vm.Expressions.GroupDescriptions.Add(new PropertyGroupDescription("ModelName"));

            vm.Equations = new ListCollectionView(source.Equations);
            vm.Equations.GroupDescriptions.Add(new PropertyGroupDescription("ModelName"));
            
               vm.Data = source;
            return vm;

        }
    }
}
