using Castle.MicroKernel.Registration;
using ChartEditor.Factory;
using ChartEditor.Views;
using ChartEditor.ViewModels;
using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
             Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IChartViewModel>().ImplementedBy<ChartEditorViewModel>().LifestyleTransient());
            container
                .Register(Component.For<IChartView>().ImplementedBy<ChartEditorView>().LifestyleTransient());
            container
              .Register(Component.For<IChartViewModelFactory>().ImplementedBy<ChartEditorFactory>().LifestyleSingleton());

        }
    }
}
