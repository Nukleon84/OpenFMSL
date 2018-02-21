using Castle.MicroKernel.Registration;
using InteractiveConsoleControl.ViewModels;
using InteractiveConsoleControl.Views;
using OpenFMSL.Contracts.Infrastructure.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interactiveconsolecontrol.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
            Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IInteractiveConsoleViewModel>().ImplementedBy<InteractiveConsoleViewModel>().LifestyleSingleton());
            container
                .Register(Component.For<IInteractiveConsoleView>().ImplementedBy<InteractiveConsoleView>().LifestyleSingleton());

        }

    }
}
