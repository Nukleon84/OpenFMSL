using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Flowsheeting
{
    public class BaseFlowsheetObject
    {
        string _name;
        FlowsheetIcon _icon = new FlowsheetIcon();


        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public FlowsheetIcon Icon
        {
            get
            {
                return _icon;
            }

            set
            {
                _icon = value;
            }
        }

        public BaseFlowsheetObject SetIcon(IconTypes type, double x, double y)
        {
            Icon.IconType = type;
            Icon.X = x;
            Icon.Y = y;
            return this;
        }
        
        public BaseFlowsheetObject SetPosition(double x, double y)
        {
            Icon.X = x;
            Icon.Y = y;
            return this;
        }
        public BaseFlowsheetObject SetSize(double width, double height)
        {
            Icon.Width = width;
            Icon.Height = height;
            return this;
        }
        public BaseFlowsheetObject SetColors(string border, string fill)
        {
            Icon.BorderColor = border;
            Icon.FillColor = fill;
            return this;
        }
    }
}
