using Castle.MicroKernel.Registration;
using FlowsheetEditorControl.Factory;
using FlowsheetEditorControl.ViewModels;
using FlowsheetEditorControl.Views;
using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowsheetEditor.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
             Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IFlowsheetEntityEditorViewModel>().ImplementedBy<FlowsheetEditorViewModel>().LifestyleTransient());
            container
                .Register(Component.For<IFlowsheetEntityEditorView>().ImplementedBy<FlowsheetEditorView>().LifestyleTransient());
            container
              .Register(Component.For<IFlowsheetEntityEditorFactory>().ImplementedBy<FlowsheetEditorControlFactory>().LifestyleSingleton());

        }
    }
}
