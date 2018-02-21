using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Infrastructure.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pythonenvironment.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
            Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IPythonEnvironment>().ImplementedBy<PythonEnvironment.PythonEnvironmentModule>().LifestyleSingleton());
           

        }

    }
}
