using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentEditorControl.ViewModels
{
    public class TabViewModel: Screen
    {
        object _content;


        public TabViewModel(string title, object content)
        {
            DisplayName = title;
            _content = content;
        }

        public object Content
        {
            get
            {
                return _content;
            }

           
        }
    }
}
