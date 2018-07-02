using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Shell.Views
{
    /// <summary>
    /// Interaktionslogik für TestDocumentView.xaml
    /// </summary>
    public partial class TestDocumentView : UserControl
    {
        public TestDocumentView()
        {
            InitializeComponent();

          

        }

      

        void link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            //http://stackoverflow.com/questions/2288999/how-can-i-get-a-flowdocument-hyperlink-to-launch-browser-and-go-to-url-in-a-wpf
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }


    }
}
