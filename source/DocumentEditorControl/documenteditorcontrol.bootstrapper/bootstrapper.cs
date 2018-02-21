using Castle.MicroKernel.Registration;
using DocumentEditorControl.ViewModels;
using DocumentEditorControl.Views;
using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace documenteditorcontrol.bootstrapper
{
   
        public class bootstrapper : IWindsorInstaller
        {
            public void Install(Castle.Windsor.IWindsorContainer container,
                Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
            {
                container
                    .Register(Component.For<IDocumentEditorViewModel>().ImplementedBy<DocumentEditorControlViewModel>().LifestyleSingleton());
                container
                    .Register(Component.For<IDocumentEditorView>().ImplementedBy<DocumentEditorControlView>().LifestyleSingleton());

            }

        }
    
}
