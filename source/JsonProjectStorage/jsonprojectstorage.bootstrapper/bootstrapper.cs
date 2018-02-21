using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsonprojectstorage.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
            Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IProjectStorage>().ImplementedBy<JsonProjectStorage.JsonProjectStorage>().LifestyleSingleton());
            

        }

    }
}
