using OpenFMSL.Core.Flowsheeting;
using System;

namespace FlowsheetEditorControl.Items
{
 
    public class ToolboxItem
    {
        double _width = 50;
        private string _fillColor = "White";
        private string _borderColor = "Black";

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }
        double _height = 50;

        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }              

        IconTypes _iconType;

        public IconTypes DisplayIcon
        {
            get { return _iconType; }
            set { _iconType = value; }
        }

        public string FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        public string BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        public ToolboxItem(IconTypes icon, double width, double height)
        {
          
            DisplayIcon = icon;
            Height = height;
            Width = width;
        }
    }
}
