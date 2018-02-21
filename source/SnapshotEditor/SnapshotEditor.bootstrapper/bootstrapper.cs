using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Documents;
using SnapshotEditor.Factory;
using SnapshotEditor.ViewModels;
using SnapshotEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotEditor.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
             Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<ISnapshotDocumentViewModel>().ImplementedBy<SnapshotEditorViewModel>().LifestyleTransient());
            container
                .Register(Component.For<ISnapshotDocumentView>().ImplementedBy<SnapshotEditorView>().LifestyleTransient());
            container
              .Register(Component.For<ISnapshotDocumentViewModelFactory>().ImplementedBy<SnapshotEditorViewModelFactory>().LifestyleSingleton());

        }
    }
}
