using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermodynamicSystemEditor.Factory;
using ThermodynamicSystemEditor.ViewModels;
using ThermodynamicSystemEditor.Views;

namespace thermodynamicsystemeditor.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
             Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IThermodynamicSystemViewModel>().ImplementedBy<ThermodynamicSystemEditorViewModel>().LifestyleTransient());
            container
                .Register(Component.For<IThermodynamicSystemView>().ImplementedBy<ThermodynamicSystemEditorView>().LifestyleTransient());
            container
              .Register(Component.For<IThermodynamicSystemViewModelFactory>().ImplementedBy<ThermodynamicSystemEditorViewModelFactory>().LifestyleSingleton());

        }
    }
}
