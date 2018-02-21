using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermodynamicsImporter;

namespace thermodynamicsimporter.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
            Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IThermodynamicSystemImporter>().ImplementedBy<Importer>().LifestyleSingleton());


        }

    }
}
