using Castle.MicroKernel.Registration;
using OpenFMSL.Contracts.Documents;
using PythonScriptEditor.Factory;
using PythonScriptEditor.ViewModels;
using PythonScriptEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pythonscripteditor.bootstrapper
{
    public class bootstrapper : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container,
              Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container
                .Register(Component.For<IPythonScriptDocumentViewModel>().ImplementedBy<PythonScriptEditorViewModel>().LifestyleTransient());
            container
                .Register(Component.For<IPythonScriptDocumentView>().ImplementedBy<PythonScriptEditorView>().LifestyleTransient());
            container
              .Register(Component.For<IPythonScriptViewModelFactory>().ImplementedBy<PythonScriptEditorFactory>().LifestyleSingleton());

        }
    }
}
