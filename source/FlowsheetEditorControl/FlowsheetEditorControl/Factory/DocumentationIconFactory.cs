using FlowsheetEditorControl.Items;
using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Flowsheeting.Documentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowsheetEditorControl.Factory
{
    public class DocumentationIconFactory
    {
        public static VisualUnit Create(DocumentationElement element, FlowsheetIcon icon)
        {
            switch (icon.IconType)
            {
                case IconTypes.Text:
                    {

                        var newItem = new VisualUnit();
                        newItem.Name = "Label01";
                        newItem.Type = "Text";
                        newItem.X = icon.X;
                        newItem.Y = icon.Y;
                        newItem.Height = icon.Height;
                        newItem.Width = icon.Width;
                        newItem.DisplayIcon = IconTypes.Text;                        
                        newItem.FillColor =icon.FillColor;
                        newItem.BorderColor = icon.BorderColor;
                        newItem.Model = element;
                        return newItem;
                    }
                case IconTypes.StreamTable:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "Table01";
                        newItem.Type = "StreamTable";
                        newItem.X = icon.X;
                        newItem.Y = icon.Y;
                        newItem.Height = icon.Height;
                        newItem.Width = icon.Width;
                        newItem.DisplayIcon = IconTypes.StreamTable;
                        newItem.FillColor = icon.FillColor;
                        newItem.BorderColor = icon.BorderColor;
                        newItem.Model = element;
                        return newItem;
                    }
                default:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "U01";
                        newItem.X = icon.X;
                        newItem.Y = icon.Y;
                        newItem.Height = 60;
                        newItem.Width = 60;
                        newItem.DisplayIcon = IconTypes.Block;
                        return newItem;
                    }
            }
        }
    }
}
