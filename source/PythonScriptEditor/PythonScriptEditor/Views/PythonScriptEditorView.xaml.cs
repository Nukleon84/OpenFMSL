using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenFMSL.Contracts.Documents;
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

namespace PythonScriptEditor.Views
{
    /// <summary>
    /// Interaktionslogik für PythonScriptEditorView.xaml
    /// </summary>
    public partial class PythonScriptEditorView : UserControl, IPythonScriptDocumentView
    {
        public PythonScriptEditorView()
        {
            InitializeComponent();
            PythonScriptTextBox.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(Environment.CurrentDirectory + "\\Resources\\Python.xshd"), HighlightingManager.Instance);
        }
    }
}
