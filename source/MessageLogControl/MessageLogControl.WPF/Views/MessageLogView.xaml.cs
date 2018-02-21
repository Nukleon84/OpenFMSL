using OpenFMSL.Contracts.Infrastructure.Messaging;
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

namespace MessageLog.Views
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class MessageLogView : UserControl, IMessageLogView
    {
       
        public MessageLogView()
        {           
            InitializeComponent();
            //Trace.WriteLine(("MessageLogViewModel created"));
        }
    }
}
