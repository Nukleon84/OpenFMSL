using System.Collections.Generic;

namespace OpenFMSL.Contracts.Infrastructure.Reporting
{
    public enum LegendPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    public class ChartModel
    {
        string _title = "New Chart";
        string _xAxisTitle = "x";
        string _yAxisTitle = "y";
        bool _showLegend = true;
        bool _isxAxisCategory = false;
        bool _isReversedYAxis = false;
        bool _isReversedXAxis = false;
        List<SeriesModel> _series = new List<SeriesModel>();
        List<string> _xAxisLabels = new List<string>();
        bool _autoScaleX = false;
        bool _autoScaleY = false;
        LegendPosition _legendPosition = LegendPosition.TopRight;

        double _xMin, _xMax, _yMin, _yMax;
    

        public bool AutoScaleX
        {
            get { return _autoScaleX; }
            set { _autoScaleX = value; }
        }
      
        public double YMax
        {
            get { return _yMax; }
            set { _yMax = value; }
        }

        public double XMax
        {
            get { return _xMax; }
            set { _xMax = value; }
        }

        public double YMin
        {
            get { return _yMin; }
            set { _yMin = value; }
        }

        public double XMin
        {
            get { return _xMin; }
            set { _xMin = value; }
        }

        public bool IsxAxisCategory
        {
            get { return _isxAxisCategory; }
            set { _isxAxisCategory = value; }
        }

        public List<string> XAxisLabels
        {
            get { return _xAxisLabels; }
            set { _xAxisLabels = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    
        public string XAxisTitle
        {
            get { return _xAxisTitle; }
            set { _xAxisTitle = value; }
        }
      
        public string YAxisTitle
        {
            get { return _yAxisTitle; }
            set { _yAxisTitle = value; }
        }

     
        public bool ShowLegend
        {
            get { return _showLegend; }
            set { _showLegend = value; }
        }

      
        public List<SeriesModel> Series
        {
            get { return _series; }
            set { _series = value; }
        }

        public bool IsReversedYAxis
        {
            get
            {
                return _isReversedYAxis;
            }

            set
            {
                _isReversedYAxis = value;
            }
        }

        public bool IsReversedXAxis
        {
            get
            {
                return _isReversedXAxis;
            }

            set
            {
                _isReversedXAxis = value;
            }
        }

        public bool AutoScaleY
        {
            get
            {
                return _autoScaleY;
            }

            set
            {
                _autoScaleY = value;
            }
        }

        public LegendPosition LegendPosition
        {
            get
            {
                return _legendPosition;
            }

            set
            {
                _legendPosition = value;
            }
        }

        public ChartModel()
        {

        }

        public ChartModel(string name)
        {
            Title = name;
        }

        public List<double> CreateList()
        {
            return new List<double>();
        }
    }
}
