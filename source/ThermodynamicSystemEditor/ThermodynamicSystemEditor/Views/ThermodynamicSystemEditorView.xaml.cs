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

namespace ThermodynamicSystemEditor.Views
{
    /// <summary>
    /// Interaktionslogik für ThermodynamicSystemEditorView.xaml
    /// </summary>
    public partial class ThermodynamicSystemEditorView : UserControl, IThermodynamicSystemView
    {
        public ThermodynamicSystemEditorView()
        {
            InitializeComponent();
        }
    }
}
