using Caliburn.Micro;
using Castle.Core;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.IO;
using Shell.ViewModels;

namespace Shell
{
    class CastleBootstrapper : BootstrapperBase
    {
        private ApplicationContainer _container;

        public CastleBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }

        protected override void Configure()
        {
            _container = new ApplicationContainer();
        }
        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key)
                       ? _container.Resolve(service)
                       : _container.Resolve(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return (IEnumerable<object>)_container.ResolveAll(service);
        }

        protected override void BuildUp(object instance)
        {
            instance.GetType().GetProperties()
                .Where(property => property.CanWrite && property.PropertyType.IsPublic)
                .Where(property => _container.Kernel.HasComponent(property.PropertyType))
                .ForEach(property => property.SetValue(instance, _container.Resolve(property.PropertyType), null));
        }

        private IEnumerable<Assembly> LoadAssemblies(string folder)
        {
            var directory = new DirectoryInfo(folder);
            FileInfo[] files = directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in files)
            {
                if (file.Name.Contains("Controls.") || file.Name.Contains(".Views") || file.Name.Contains(".ViewModels"))
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                    Assembly assembly = AppDomain.CurrentDomain.Load(assemblyName);//Assembly.Load(assemblyName);
                    yield return assembly;
                }
            }

            yield break;
        }


        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            IEnumerable<Assembly> viewsAsm = LoadAssemblies(Environment.CurrentDirectory);
            assemblies.AddRange(viewsAsm);
            return assemblies;

        }
    }

    public class ApplicationContainer : WindsorContainer
    {
        public ApplicationContainer()
        {
            AddFacility<TypedFactoryFacility>();
            
            Register(
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifeStyle.Is(LifestyleType.Singleton),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifeStyle.Is(LifestyleType.Singleton)
                );
                     
            Install( FromAssembly.InDirectory(new AssemblyFilter(Environment.CurrentDirectory)));
          
            Register(Component.For<MainWindowViewModel>().LifestyleSingleton());
            // ViewLocator.AddSubNamespaceMapping("Controls.InteractiveMediaConsole.WPF.ViewModels", "Controls.InteractiveMediaConsole.WPF.Views");
        }        
    }

    public static class ForEachExtension
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
        }
    }



}
