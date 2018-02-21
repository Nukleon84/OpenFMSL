using System.Collections.Generic;

namespace OpenFMSL.Contracts.Infrastructure.Reporting
{
    public enum SeriesType { Scatter, Line, Bar, Column, StackedBar100, StackedBar, StackedColumn, StackedColumn100, JacobianStructure };
    public enum MarkerType { Square, Circle, Diamond };
    public enum DashPattern { None, Dash, AlternatingDash, Dotted, DashDot}
    public class SeriesModel
    {
        string _name;
        string _color = "Black";
        double _thickness = 2;
        double _markerSize = 10;
        bool _showInLegend = true;
        bool _showMarker = false;
        List<double> _x = new List<double>();
        List<double> _y = new List<double>();
        SeriesType _type = SeriesType.Scatter;
        MarkerType _marker = MarkerType.Circle;
        DashPattern _dashPattern = DashPattern.None;

        public bool ShowMarker
        {
            get { return _showMarker; }
            set { _showMarker = value; }
        }

        public MarkerType Marker
        {
            get { return _marker; }
            set { _marker = value; }
        }

        public double MarkerSize
        {
            get { return _markerSize; }
            set { _markerSize = value; }
        }

        public double Thickness
        {
            get { return _thickness; }
            set { _thickness = value; }
        }

        public bool ShowInLegend
        {
            get { return _showInLegend; }
            set { _showInLegend = value; }
        }

        public string Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<double> X
        {
            get { return _x; }
            set { _x = value; }
        }

        public List<double> Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public SeriesType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public DashPattern DashPattern
        {
            get
            {
                return _dashPattern;
            }

            set
            {
                _dashPattern = value;
            }
        }

        public SeriesModel()
        {

        }
        public SeriesModel(string name, SeriesType type, List<double> x, List<double> y, string color)
        {
            Name = name;
            Type = type;
            X = x;
            Y = y;
            Color = color;
        }
    }
}
