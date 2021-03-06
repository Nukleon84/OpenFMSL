﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using MessageLog.ViewModels;
using MessageLog.Views;

namespace messagelogcontrol.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
            Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IMessageLogView>().ImplementedBy<MessageLogView>().LifestyleSingleton());
            container
                .Register(Component.For<IMessageLogViewModel>().ImplementedBy<MessageLogViewModel>().LifestyleSingleton());

        }

    }
}
