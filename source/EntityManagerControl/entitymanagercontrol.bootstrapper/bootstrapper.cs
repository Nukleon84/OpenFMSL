using Castle.MicroKernel.Registration;
using EntityManagerControl.ViewModels;
using EntityManagerControl.Views;
using OpenFMSL.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entitymanagercontrol.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
            Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IEntityManagerViewModel>().ImplementedBy<EntityManagerViewModel>().LifestyleSingleton());
            container
                .Register(Component.For<IEntityManagerView>().ImplementedBy<EntityManagerView>().LifestyleSingleton());

        }

    }

}
