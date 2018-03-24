using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowsheetEditorControl.Items
{
    public class VisualFlowsheet
    {
        double _flowsheetHeight = 3000;
        double _flowsheetWidth = 3000;

        bool _snapToGrid = true;
        bool _showGrid = true;
        bool _showTemperature = true;
        bool _showPressure = true;
        bool _showMassflow = true;
        bool _showVapourFraction = false;

        string _canvasColor = "GhostWhite";

        IList<VisualUnit> _items = new ObservableCollection<VisualUnit>();
        IList<Connection> _connections = new ObservableCollection<Connection>();


        public double FlowsheetHeight
        {
            get { return _flowsheetHeight; }
            set { _flowsheetHeight = value; }
        }

        public double FlowsheetWidth
        {
            get { return _flowsheetWidth; }
            set { _flowsheetWidth = value; }
        }

        public IList<VisualUnit> Items
        {
            get { return _items; }
            set { _items = new ObservableCollection<VisualUnit>(value); }
        }

        public IList<Connection> Connections
        {
            get { return _connections; }
            set { _connections = new ObservableCollection<Connection>(value); }
        }

        public bool SnapToGrid
        {
            get
            {
                return _snapToGrid;
            }

            set
            {
                _snapToGrid = value;
            }
        }

        public bool ShowGrid
        {
            get
            {
                return _showGrid;
            }

            set
            {
                _showGrid = value;
            }
        }

        public bool ShowTemperature
        {
            get
            {
                return _showTemperature;
            }

            set
            {
                _showTemperature = value;
            }
        }

        public bool ShowPressure
        {
            get
            {
                return _showPressure;
            }

            set
            {
                _showPressure = value;
            }
        }

        public bool ShowMassflow
        {
            get
            {
                return _showMassflow;
            }

            set
            {
                _showMassflow = value;
            }
        }

        public bool ShowVapourFraction
        {
            get
            {
                return _showVapourFraction;
            }

            set
            {
                _showVapourFraction = value;
            }
        }

        public string CanvasColor
        {
            get
            {
                return _canvasColor;
            }

            set
            {
                _canvasColor = value;
            }
        }

        public VisualUnit GetUnitByName(string name)
        {
            return Items.FirstOrDefault(i => i.Name == name);
        }
        public Connection GetStreamByName(string name)
        {
            return Connections.FirstOrDefault(i => i.Name == name);
        }

        public IList<VisualUnit> GetAllDescendantsOfUnit(VisualUnit unit)
        {
            var descendants = new List<VisualUnit>();

           // descendants.Add(unit);

            var visited = new Dictionary<VisualUnit, bool>();


            descendants.AddRange(GetDescendantsOfUnit(unit, visited));
            return descendants;
        }

        IList<VisualUnit> GetDescendantsOfUnit(VisualUnit unit, Dictionary<VisualUnit, bool> visited)
        {
            var descendants = new List<VisualUnit>();

            foreach (var outgoing in GetAllOutputStreams(unit))
            {
                if (!visited.ContainsKey(outgoing.Sink.Owner))
                {
                    descendants.Add(outgoing.Sink.Owner);
                    visited.Add(outgoing.Sink.Owner, true);
                }
            }

            foreach (var descendant in descendants.ToArray())
            {
                var otherNodes = GetDescendantsOfUnit(descendant, visited);
                descendants.AddRange(otherNodes);
            }

            return descendants;

        }

        public IList<Connection> GetAllOutputStreams(VisualUnit unit)
        {
            return Connections.Where(c => c.Source.Owner == unit).ToList();
        }
        public void Connect(VisualUnit source, string sourcePort, VisualUnit sink, string sinkPort)
        {
            if (source != null && source != null)
            {
                var sourceConnector = source.Connectors.FirstOrDefault(c => c.Name == sourcePort);
                var sinkConnector = sink.Connectors.FirstOrDefault(c => c.Name == sinkPort);

                if (sourceConnector != null && sinkConnector != null)
                {
                    var newCon = new Connection() { Source = sourceConnector, Sink = sinkConnector };
                    sourceConnector.IsConnected = true;
                    sinkConnector.IsConnected = true;

                    newCon.UpdatePathGeometry();
                    Connections.Add(newCon);
                }
            }
        }

        public void Connect(string source, string sourcePort, string sink, string sinkPort)
        {
            var sourceUnit = Items.FirstOrDefault(i => i.Name == source);
            var sinkUnit = Items.FirstOrDefault(i => i.Name == sink);

            if (sourceUnit != null && sinkUnit != null)
            {
                Connect(sourceUnit, sourcePort, sinkUnit, sinkPort);
            }
        }

    }
}
