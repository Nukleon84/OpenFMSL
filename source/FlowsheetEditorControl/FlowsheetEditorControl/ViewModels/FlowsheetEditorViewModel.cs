using Caliburn.Micro;
using FlowsheetEditor.Factory;
using FlowsheetEditorControl.Items;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        var connection=Connect(visualUnit.GetConnectorByName("Stream"), sinkVisualCon);
                        connection.Model = stream;
                    }
                }


                if (source != null && sink == null)
                {
                    var visualUnit = ItemFactory.Create(IconTypes.Product, stream.Icon.X, stream.Icon.Y);
                    visualUnit.Name = stream.Name;
                    visualUnit.BorderColor = stream.Icon.BorderColor;
                    visualUnit.FillColor = stream.Icon.FillColor;
                    visualUnit.Model = stream;
                    Items.Add(visualUnit);


                    var sourceVisual = GetUnitByName(source.Name);
                    var sourceVisualCon = sourceVisual?.GetConnectorByName(source.MaterialPorts.Where(p => p.Streams.Contains(stream)).FirstOrDefault().Name);
                    if (sourceVisualCon != null)
                    {
                        var connection=Connect(sourceVisualCon, visualUnit.GetConnectorByName("Stream"));
                        connection.Model = stream;
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
                        var connection=Connect(sourceVisualCon, sinkVisualCon);
                        connection.Model = stream;
                    }
                }
            }

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

    }
}
