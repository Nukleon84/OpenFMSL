using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using OpenFMSL.Contracts.Infrastructure.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace InteractiveConsoleControl.Views
{
    /// <summary>
    /// Interaktionslogik für InteractiveConsoleView.xaml
    /// </summary>
    public partial class InteractiveConsoleView : UserControl, IInteractiveConsoleView
    {
        public InteractiveConsoleView()
        {
            InitializeComponent();

            //   History.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(History.TextArea));
            SearchPanel.Install(History.TextArea);
            History.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(Environment.CurrentDirectory + "\\Resources\\Log.xshd"), HighlightingManager.Instance);
        }

        private void History_TextChanged(object sender, EventArgs e)
        {
            History.ScrollToEnd();
        }
    }
}
