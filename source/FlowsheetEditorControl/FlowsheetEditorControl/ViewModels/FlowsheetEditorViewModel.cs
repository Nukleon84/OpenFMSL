using Caliburn.Micro;
using FlowsheetEditor.Factory;
using FlowsheetEditorControl.Factory;
using FlowsheetEditorControl.Items;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowsheetEditorControl.ViewModels
{
    public class FlowsheetEditorViewModel : PropertyChangedBase, IFlowsheetEntityEditorViewModel
    {
        [NonSerialized]
        Connector newSource;
        [NonSerialized]
        Connector newSink;
        [NonSerialized]
        Connector currentConnector;
        [NonSerialized]
        Connection currentConnection;
        [NonSerialized]
        DrawableItem currentObject;

        private VisualFlowsheet _flowsheet = new VisualFlowsheet();

        private FlowsheetEntity _owner = null;
        private IEventAggregator _aggregator;

        #region Properties

        public VisualUnit GetUnitByName(string name)
        {
            foreach (var unit in Items)
                if (unit.Name == name)
                    return unit;

            return null;
        }

        public DrawableItem CurrentObject
        {
            get { return currentObject; }
            set
            {
                currentObject = value;
                NotifyOfPropertyChange(() => CurrentObject);
            }
        }
        public double FlowsheetHeight
        {
            get { return _flowsheet.FlowsheetHeight; }
            set
            {
                _flowsheet.FlowsheetHeight = value;
                NotifyOfPropertyChange(() => FlowsheetHeight);
            }
        }
        public double FlowsheetWidth
        {
            get { return _flowsheet.FlowsheetWidth; }
            set
            {
                _flowsheet.FlowsheetWidth = value;
                NotifyOfPropertyChange(() => FlowsheetWidth);
            }
        }
        public IList<VisualUnit> Items
        {
            get { return _flowsheet.Items; }
            set { _flowsheet.Items = value; NotifyOfPropertyChange(() => Items); }
        }
        public IList<Connection> Connections
        {
            get { return _flowsheet.Connections; }
            set { _flowsheet.Connections = value; NotifyOfPropertyChange(() => Connections); }
        }


        public bool SnapToGrid
        {
            get
            {
                return Flowsheet.SnapToGrid;
            }

            set
            {
                Flowsheet.SnapToGrid = value;
                NotifyOfPropertyChange(() => SnapToGrid);

            }
        }

        public bool ShowGrid
        {
            get
            {
                return Flowsheet.ShowGrid;
            }

            set
            {
                Flowsheet.ShowGrid = value;

                NotifyOfPropertyChange(() => ShowGrid);
            }
        }

        public bool ShowTemperature
        {
            get
            {
                return Flowsheet.ShowTemperature;
            }

            set
            {
                Flowsheet.ShowTemperature = value;
                NotifyOfPropertyChange(() => ShowTemperature);
            }
        }

        public bool ShowPressure
        {
            get
            {
                return Flowsheet.ShowPressure;
            }

            set
            {
                Flowsheet.ShowPressure = value;

                NotifyOfPropertyChange(() => ShowPressure);
            }
        }

        public bool ShowMassflow
        {
            get
            {
                return Flowsheet.ShowMassflow;
            }

            set
            {
                Flowsheet.ShowMassflow = value;

                NotifyOfPropertyChange(() => ShowMassflow);
            }
        }

        public bool ShowVapourFraction
        {
            get
            {
                return Flowsheet.ShowVapourFraction;
            }

            set
            {
                Flowsheet.ShowVapourFraction = value;

                NotifyOfPropertyChange(() => ShowVapourFraction);
            }
        }

        public string CanvasColor
        {
            get
            {
                return Flowsheet.CanvasColor;
            }

            set
            {
                Flowsheet.CanvasColor = value;
                NotifyOfPropertyChange(() => CanvasColor);
            }
        }

        public Connector NewSource
        {
            get { return newSource; }
            set { newSource = value; NotifyOfPropertyChange(() => NewSource); }
        }
        public Connector NewSink
        {
            get { return newSink; }
            set { newSink = value; NotifyOfPropertyChange(() => NewSink); }
        }
        public Connector CurrentConnector
        {
            get { return currentConnector; }
            set { currentConnector = value; NotifyOfPropertyChange(() => CurrentConnector); }
        }
        public Connection CurrentConnection
        {
            get { return currentConnection; }
            set { currentConnection = value; NotifyOfPropertyChange(() => CurrentConnection); }
        }

        public VisualFlowsheet Flowsheet
        {
            get { return _flowsheet; }
            set
            {
                _flowsheet = value;
                NotifyOfPropertyChange(() => Flowsheet);
                NotifyOfPropertyChange(() => SnapToGrid);
                NotifyOfPropertyChange(() => ShowGrid);
                NotifyOfPropertyChange(() => CanvasColor);
                NotifyOfPropertyChange(() => ShowTemperature);
                NotifyOfPropertyChange(() => ShowPressure);
                NotifyOfPropertyChange(() => ShowMassflow);
                NotifyOfPropertyChange(() => ShowVapourFraction);
                NotifyOfPropertyChange(() => CanvasColor);


            }
        }

        public IList<VisualUnit> SelectedItems
        {
            get { return Items.Where(i => i.IsSelected).ToList(); }
        }
        public IList<Connection> SelectedConnections
        {
            get { return Connections.Where(i => i.IsSelected).ToList(); }
        }









        #endregion


        public FlowsheetEditorViewModel(IEventAggregator aggregator) : this(aggregator, null)
        {
        }



        public FlowsheetEditorViewModel(IEventAggregator aggregator, FlowsheetEntity entity)
        {
            _aggregator = aggregator;
            _owner = entity;

            if (entity != null)
                CreateViewModelFromEntity();
            /*  var feed = ItemFactory.Create(IconTypes.Feed, 100, 140);
             var flash = ItemFactory.Create(IconTypes.TwoPhaseFlash, 300, 140);
             var vapo = ItemFactory.Create(IconTypes.Product, 500, 80);
             var liqu = ItemFactory.Create(IconTypes.Product, 500, 200);
             feed.Name = "FEED";
             flash.Name = "FLASH";
             vapo.Name = "VAPOR";
             liqu.Name = "LIQUID";

             Items.Add(feed);
             Items.Add(flash);
              Items.Add(vapo);
              Items.Add(liqu);

           Connect(feed.GetConnectorByName("Mixed"), flash.GetConnectorByName("Inlet[0]"));
           Connect(flash.GetConnectorByName("Outlet[0]"), vapo.GetConnectorByName("Mixed"));
           Connect(flash.GetConnectorByName("Outlet[1]"), liqu.GetConnectorByName("Mixed"));*/


            // Items.Add(ItemFactory.Create(ModelTypes.MaterialStream, 300, 110, null));
            //Items.Add(ItemFactory.Create(ModelTypes.MaterialStream, 300, 170, null));

            //var col = ItemFactory.Create(ModelTypes.Column, 400, 100, null);

            // col.Width = 50;
            // col.Height = 200;
            // Items.Add(col);

        }

        void CreateViewModelFromEntity()
        {
            foreach (var unit in _owner.Flowsheet.Units)
            {
                var visualUnit = ItemFactory.Create(unit.Icon.IconType, unit.Icon.X, unit.Icon.Y);
                visualUnit.Name = unit.Name;
                visualUnit.BorderColor = unit.Icon.BorderColor;
                visualUnit.FillColor = unit.Icon.FillColor;
                visualUnit.Model = unit;
                Items.Add(visualUnit);
            }

            foreach (var unit in _owner.Flowsheet.Documentation)
            {
                var visualUnit = DocumentationIconFactory.Create(unit, unit.Icon);
                visualUnit.Name = unit.Name;                
                Items.Add(visualUnit);
            }


            foreach (var stream in _owner.Flowsheet.MaterialStreams)
            {
                var source = _owner.Flowsheet.Units.Where(u => u.MaterialPorts.Any(p => p.Streams.Contains(stream) && p.Direction == PortDirection.Out)).FirstOrDefault();
                var sink = _owner.Flowsheet.Units.Where(u => u.MaterialPorts.Any(p => p.Streams.Contains(stream) && p.Direction == PortDirection.In)).FirstOrDefault();

                if (source == null && sink != null)
                {
                    var visualUnit = ItemFactory.Create(IconTypes.Feed, stream.Icon.X, stream.Icon.Y);
                    visualUnit.Name = stream.Name;
                    visualUnit.BorderColor = stream.Icon.BorderColor;
                    visualUnit.FillColor = stream.Icon.FillColor;
                    visualUnit.Model = stream;
                    Items.Add(visualUnit);

                    var sinkVisual = GetUnitByName(sink.Name);
                    var sinkVisualCon = sinkVisual?.GetConnectorByName(sink.MaterialPorts.Where(p => p.Streams.Contains(stream)).FirstOrDefault().Name);
                    if (sinkVisualCon != null)
                    {
                        var connection = Connect(visualUnit.GetConnectorByName("Stream"), sinkVisualCon);
                        connection.Model = stream;
                        connection.Name = stream.Name;
                        connection.Report = WriteStreamReport(stream);
                    }
                }


                if (source != null && sink == null)
                {
                    var visualUnit = ItemFactory.Create(IconTypes.Product, stream.Icon.X, stream.Icon.Y);
                    visualUnit.Name =  stream.Name;
                    visualUnit.BorderColor = stream.Icon.BorderColor;
                    visualUnit.FillColor = stream.Icon.FillColor;
                    visualUnit.Model = stream;
                    Items.Add(visualUnit);


                    var sourceVisual = GetUnitByName(source.Name);
                    var sourceVisualCon = sourceVisual?.GetConnectorByName(source.MaterialPorts.Where(p => p.Streams.Contains(stream)).FirstOrDefault().Name);
                    if (sourceVisualCon != null)
                    {
                        var connection = Connect(sourceVisualCon, visualUnit.GetConnectorByName("Stream"));
                        connection.Model = stream;
                        connection.Name = stream.Name;
                        connection.Report = WriteStreamReport(stream);
                    }
                }

                if (source != null && sink != null)
                {
                    var sinkVisual = GetUnitByName(sink.Name);
                    var sinkVisualCon = sinkVisual?.GetConnectorByName(sink.MaterialPorts.Where(p => p.Streams.Contains(stream)).FirstOrDefault().Name);
                    var sourceVisual = GetUnitByName(source.Name);
                    var sourceVisualCon = sourceVisual?.GetConnectorByName(source.MaterialPorts.Where(p => p.Streams.Contains(stream)).FirstOrDefault().Name);
                    if (sourceVisualCon != null && sinkVisualCon != null)
                    {
                        var connection = Connect(sourceVisualCon, sinkVisualCon);
                        connection.Model = stream;
                        connection.Name = stream.Name;
                        connection.Report = WriteStreamReport(stream);
                    }
                }
            }

            FlowsheetWidth = Items.Max(i => i.X + i.Width + 100);
            FlowsheetHeight = Items.Max(i => i.Y + i.Height + 100);

        }


        string WriteStreamReport(MaterialStream stream)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Material Stream: " + stream.Name);
            sb.AppendLine();
            sb.AppendLine(stream.Vfmolar.WriteReport());
            sb.AppendLine(stream.Mixed.Temperature.WriteReport());           
            sb.AppendLine(stream.Mixed.Pressure.WriteReport());
            sb.AppendLine(stream.Mixed.SpecificEnthalpy.WriteReport());
            sb.AppendLine(stream.Mixed.TotalEnthalpy.WriteReport());
            //sb.AppendLine(stream.Mixed.TotalMolarflow.WriteReport());
            //sb.AppendLine(stream.Mixed.TotalMassflow.WriteReport());
            sb.AppendLine(stream.Mixed.TotalVolumeflow.WriteReport());
            sb.AppendLine(String.Format("{0,-25} = {1, 12}", "Phase", stream.State));
            sb.AppendLine("");
            sb.AppendLine("Mixed");
            sb.Append(String.Format("{0,-12}", "Component"));
            sb.Append(String.Format("{0,15}", "Mole Flow"));
            sb.Append(String.Format("{0,15}", "Mole Fraction"));
            sb.Append(String.Format("{0,15}", "Mass Flow"));
            sb.AppendLine(String.Format("{0,15}", "Mass Fraction"));

            sb.Append(String.Format("{0,-12}", ""));
            sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MolarFlow].Symbol));
            sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MolarFraction].Symbol));
            sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MassFlow].Symbol));
            sb.AppendLine(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MassFraction].Symbol));


            for (int i = 0; i < stream.Mixed.ComponentMassflow.Count; i++)
            {
                if (stream.Mixed.ComponentMolarflow[i].ValueInOutputUnit < 1e-10)
                    continue;
                
                sb.Append(String.Format("  {0,-10}", stream.System.Components[i].ID));
                sb.Append(String.Format("{0,15}", stream.Mixed.ComponentMolarflow[i].ValueInOutputUnit.ToString("0.0000")));
                sb.Append(String.Format("{0,15}", stream.Mixed.ComponentMolarFraction[i].ValueInOutputUnit.ToString("0.0000")));
                sb.Append(String.Format("{0,15}", stream.Mixed.ComponentMassflow[i].ValueInOutputUnit.ToString("0.0000")));
                sb.AppendLine(String.Format("{0,15}", stream.Mixed.ComponentMassFraction[i].ValueInOutputUnit.ToString("0.0000")));
            }
            sb.Append(String.Format("  {0,-10}", "Sum"));
            sb.Append(String.Format("{0,15}", stream.Mixed.TotalMolarflow.ValueInOutputUnit.ToString("0.0000")));
            sb.Append(String.Format("{0,15}", ""));
            sb.AppendLine(String.Format("{0,15}", stream.Mixed.TotalMassflow.ValueInOutputUnit.ToString("0.0000")));

            if (stream.Vapor.TotalMolarflow.ValueInSI > 1e-6)
            {
                sb.AppendLine("");
                sb.AppendLine("Vapor");
                sb.Append(String.Format("{0,-12}", "Component"));
                sb.Append(String.Format("{0,15}", "Mole Flow"));
                sb.Append(String.Format("{0,15}", "Mole Fraction"));
                sb.Append(String.Format("{0,15}", "Mass Flow"));
                sb.AppendLine(String.Format("{0,15}", "Mass Fraction"));

                sb.Append(String.Format("{0,-12}", ""));
                sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MolarFlow].Symbol));
                sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MolarFraction].Symbol));
                sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MassFlow].Symbol));
                sb.AppendLine(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MassFraction].Symbol));


                for (int i = 0; i < stream.Mixed.ComponentMassflow.Count; i++)
                {
                    if (stream.Vapor.ComponentMolarflow[i].ValueInOutputUnit < 1e-10)
                        continue;

                    sb.Append(String.Format("  {0,-10}", stream.System.Components[i].ID));
                    sb.Append(String.Format("{0,15}", stream.Vapor.ComponentMolarflow[i].ValueInOutputUnit.ToString("0.0000")));
                    sb.Append(String.Format("{0,15}", stream.Vapor.ComponentMolarFraction[i].ValueInOutputUnit.ToString("0.0000")));
                    sb.Append(String.Format("{0,15}", stream.Vapor.ComponentMassflow[i].ValueInOutputUnit.ToString("0.0000")));
                    sb.AppendLine(String.Format("{0,15}", stream.Vapor.ComponentMassFraction[i].ValueInOutputUnit.ToString("0.0000")));                 
                }
                sb.Append(String.Format("  {0,-10}", "Sum"));
                sb.Append(String.Format("{0,15}", stream.Vapor.TotalMolarflow.ValueInOutputUnit.ToString("0.0000")));
                sb.Append(String.Format("{0,15}", ""));
                sb.AppendLine(String.Format("{0,15}", stream.Vapor.TotalMassflow.ValueInOutputUnit.ToString("0.0000")));

            }

            if (stream.Liquid.TotalMolarflow.ValueInSI > 1e-6)
            {
                sb.AppendLine("");
                sb.AppendLine("Liquid");
                sb.Append(String.Format("{0,-12}", "Component"));
                sb.Append(String.Format("{0,15}", "Mole Flow"));
                sb.Append(String.Format("{0,15}", "Mole Fraction"));
                sb.Append(String.Format("{0,15}", "Mass Flow"));
                sb.AppendLine(String.Format("{0,15}", "Mass Fraction"));

                sb.Append(String.Format("{0,-12}", ""));
                sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MolarFlow].Symbol));
                sb.Append(String.Format("{0,15}", "%"));
                sb.Append(String.Format("{0,15}", stream.System.VariableFactory.Output.UnitDictionary[OpenFMSL.Core.UnitsOfMeasure.PhysicalDimension.MassFlow].Symbol));
                sb.AppendLine(String.Format("{0,15}", "w-%"));



                for (int i = 0; i < stream.Liquid.ComponentMassflow.Count; i++)
                {
                    if (stream.Liquid.ComponentMolarflow[i].ValueInOutputUnit < 1e-10)
                        continue;

                    sb.Append(String.Format("  {0,-10}", stream.System.Components[i].ID));
                    sb.Append(String.Format("{0,15}", stream.Liquid.ComponentMolarflow[i].ValueInOutputUnit.ToString("0.0000")));
                    sb.Append(String.Format("{0,15}", stream.Liquid.ComponentMolarFraction[i].ValueInOutputUnit.ToString("0.0000")));
                    sb.Append(String.Format("{0,15}", stream.Liquid.ComponentMassflow[i].ValueInOutputUnit.ToString("0.0000")));
                    sb.AppendLine(String.Format("{0,15}", stream.Liquid.ComponentMassFraction[i].ValueInOutputUnit.ToString("0.0000")));
                }
                sb.Append(String.Format("  {0,-10}", "Sum"));
                sb.Append(String.Format("{0,15}", stream.Liquid.TotalMolarflow.ValueInOutputUnit.ToString("0.0000")));
                sb.Append(String.Format("{0,15}", ""));
                sb.AppendLine(String.Format("{0,15}", stream.Liquid.TotalMassflow.ValueInOutputUnit.ToString("0.0000")));
            }
            return sb.ToString();
        }

        void ToggleActivationOfSelectedUnits()
        {
            foreach (var unit in SelectedItems)
            {
                unit.IsActive = !unit.IsActive;
            }
        }
        /*    public void RaiseElementDetailsRequested(FlowsheetElementDoubleClickedMessage msg)
            {
                if (_aggregator != null)
                    _aggregator.Raise<FlowsheetElementDoubleClickedMessage>(msg);
            }

            public void RaiseStreamDetailsRequested(FlowsheetConnectionDoubleClickedMessage msg)
            {
                if (_aggregator != null)
                    _aggregator.Raise<FlowsheetConnectionDoubleClickedMessage>(msg);
            }*/



        public void RemoveConnection(Connection connection)
        {
            if (connection != null && Connections.Contains(connection))
            {
                connection.Sink.IsConnected = false;
                connection.Source.IsConnected = false;
                Connections.Remove(connection);
            }
        }

        public void RemoveItem(VisualUnit unit)
        {
            Items.Remove(unit);
            var incoming = Connections.Where(c => c.Sink.Owner == unit).ToList();
            var outgoing = Connections.Where(c => c.Source.Owner == unit).ToList();
            foreach (var con in incoming)
                RemoveConnection(con);
            foreach (var con in outgoing)
                RemoveConnection(con);
        }

        public Connection Connect(Connector sourceConnector, Connector sinkConnector)
        {
            var connection = new Connection();
            connection.Source = sourceConnector;
            connection.Sink = sinkConnector;
            sourceConnector.IsConnected = true;
            sinkConnector.IsConnected = true;
            AddConnection(connection);
            return connection;
        }

        public void AddConnection(Connection connection)
        {
            if (connection != null && !Connections.Contains(connection))
            {
                int i = 1;
                var existingConnectionNames = Connections.Select(c => c.Name);

                do
                {
                    connection.Name = "S" + (i).ToString("000");
                    i++;

                } while (existingConnectionNames.Contains(connection.Name));

                Connections.Add(connection);
            }
        }

        public void AddUnit(VisualUnit item)
        {
            if (item != null && !Items.Contains(item))
            {


                Items.Add(item);
                //   if (_aggregator != null)
                //      _aggregator.Raise<FlowsheetElementAddedMessage>(new FlowsheetElementAddedMessage { TimeStamp = DateTime.Now, Sender = this, Parameter = item });
            }
        }
        public void Export()
        {
            var exportText = "";
            foreach (var unit in Flowsheet.Items)
            {
                exportText += unit.Name + ".SetIcon(IconTypes." + unit.DisplayIcon + ", " + unit.X + "," + unit.y + ")" + Environment.NewLine;
            }

            var window = new Window();
            var dock = new DockPanel();
            var textBox = new TextBox();
            textBox.VerticalAlignment = VerticalAlignment.Stretch;
            textBox.Text = exportText;
            textBox.AcceptsReturn = true;            
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            dock.Children.Add(textBox);
            window.Content = dock;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Width = 550;
            window.Height = 400;
            window.ShowActivated = true;
            window.Title = "Export";
            window.WindowStyle = WindowStyle.ToolWindow;
            window.Owner = Application.Current.MainWindow;
            window.Show();

        }


        public void ScaleStreams()
        {
            double maxThickness = 10.0f;
            double minThickness = 1.0f;

            double maxMassFlow = Flowsheet.Connections.Max(s => s.Massflow.ValueInSI);
            double minMassFlow = Flowsheet.Connections.Min(s => s.Massflow.ValueInSI);

            foreach (var stream in Flowsheet.Connections)
            {
                var flow = stream.Massflow.ValueInSI;
                var thick = minThickness + (maxThickness - minThickness) / (maxMassFlow - minMassFlow) * (flow - minMassFlow);
                stream.Thickness = thick;
            }

        }

        public void ResetStreams()
        {
            foreach (var stream in Flowsheet.Connections)
                stream.Thickness = 3.0;
        }
    }
}
