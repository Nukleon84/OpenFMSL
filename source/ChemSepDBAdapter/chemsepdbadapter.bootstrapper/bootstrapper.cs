using Castle.MicroKernel.Registration;
using ChemSepDBAdapter;
using OpenFMSL.Contracts.Infrastructure.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chemsepdbadapter.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
          Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IPureComponentPropertyDatabase>().ImplementedBy<Adapter>().LifestyleSingleton());


        }
    }
}
