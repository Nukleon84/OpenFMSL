
using System;
using System.ComponentModel;
using System.Windows;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using Caliburn.Micro;

namespace FlowsheetEditorControl.Items
{
    [Serializable]
    public class Connection : PropertyChangedBase
    {
        #region Fields

        private ConnectionTypes _connectionType;
        private string _name;
        private Connector _sink;
        private Connector _source;
        private System.Windows.Point _labelPoint;
        private bool _isSelected = false;

        double _thickness = 3.0;
        string _color = "DimGray";
        string _dashArray = "1,0";
        string _report;
        FlowsheetObject _model;

        #endregion

        #region Properties
        public Connector Source
        {
            get { return _source; }
            set
            {

                if (_source != value)
                {
                    if (_source != null)
                    {
                        _source.OnPositionUpdated -= onPositionUpdated;
                    }
                }
                _source = value;

                if (_source != null)
                {
                    _source.OnPositionUpdated += onPositionUpdated;

                }

                UpdatePathGeometry();
                
                NotifyOfPropertyChange(() => Source);
            }
        }

        public Connector Sink
        {
            get { return _sink; }
            set
            {

                if (_sink != value)
                {
                    if (_sink != null)
                    {
                        _sink.OnPositionUpdated -= onPositionUpdated;
                    }
                }
                _sink = value;

                if (_sink != null)
                {
                    _sink.OnPositionUpdated += onPositionUpdated;

                }

                UpdatePathGeometry();
                NotifyOfPropertyChange(() => Sink);
             
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => Name); }
        }

       

        [NonSerialized]
        System.Windows.Media.PolyLineSegment path = new System.Windows.Media.PolyLineSegment();

        public void Select()
        {
            IsSelected = true;
        }
        public System.Windows.Media.PolyLineSegment Path
        {
            get { return path; }
            set { path = value; NotifyOfPropertyChange(() => Path); }
        }

        protected void onPositionUpdated(DrawableItem sender)
        {
            UpdatePathGeometry();
        }

        public virtual void UpdatePathGeometry()
        {
            if (Path == null)
                Path = new System.Windows.Media.PolyLineSegment();

            if (Source != null && Sink != null)
            {
                if (Source.Owner != null && Sink.Owner != null)
                {

                    System.Windows.Point startPoint = new System.Windows.Point(Source.Owner.X + Source.X + 5, Source.Owner.Y + Source.Y + 5);
                    System.Windows.Point endPoint = new System.Windows.Point(Sink.Owner.X + Sink.X + 5, Sink.Owner.Y + Sink.Y + 5);

                    Path.Points.Clear();


                    var connectionRoute = 0;

                    if (Source.Direction == ConnectorDirection.Up && Sink.Direction == ConnectorDirection.Left)
                    {
                        if (startPoint.Y > endPoint.Y)
                            connectionRoute = 1;
                        else
                            connectionRoute = 8;
                    }
                    if (Source.Direction == ConnectorDirection.Up && Sink.Direction == ConnectorDirection.Right)
                    {
                        if (startPoint.Y > endPoint.Y)
                            connectionRoute = 1;
                    }

                    if (Source.Direction == ConnectorDirection.Up && Sink.Direction == ConnectorDirection.Up)
                    {
                        connectionRoute = 13;
                    }

                    if (Source.Direction == ConnectorDirection.Up && Sink.Direction == ConnectorDirection.Down)
                    {
                        if (startPoint.Y > endPoint.Y)
                            connectionRoute = 12;
                    }


                    if (Source.Direction == ConnectorDirection.Right && Sink.Direction == ConnectorDirection.Left)
                    {
                        if (startPoint.X < endPoint.X)
                            connectionRoute = 2;
                    }

                    if (Source.Direction == ConnectorDirection.Right && Sink.Direction == ConnectorDirection.Right)
                    {
                        if (startPoint.X > endPoint.X)
                            connectionRoute = 3;
                    }

                    if (Source.Direction == ConnectorDirection.Down && Sink.Direction == ConnectorDirection.Left)
                    {
                        if (startPoint.X < endPoint.X)
                        {
                            if (startPoint.Y < endPoint.Y)
                                connectionRoute = 4;
                            else
                                connectionRoute = 9;
                        }

                    }

                    if (Source.Direction == ConnectorDirection.Down && Sink.Direction == ConnectorDirection.Right)
                    {
                        if (startPoint.Y < endPoint.Y)
                            connectionRoute = 4;
                    }

                    if (Source.Direction == ConnectorDirection.Down && Sink.Direction == ConnectorDirection.Down)
                    {

                        connectionRoute = 11;
                    }
                    if (Source.Direction == ConnectorDirection.Down && Sink.Direction == ConnectorDirection.Up)
                    {
                        if (startPoint.Y < endPoint.Y)
                            connectionRoute = 12;
                    }

                    if (Source.Direction == ConnectorDirection.Right && Sink.Direction == ConnectorDirection.Up)
                    {
                      //  if (startPoint.X > endPoint.X && startPoint.Y > endPoint.Y)
                            connectionRoute = 10;
                    }

                    if (Source.Direction == ConnectorDirection.Right && Sink.Direction == ConnectorDirection.Left)
                    {
                        if (startPoint.X > endPoint.X && startPoint.Y > endPoint.Y)
                            connectionRoute = 5;
                    }

                    if (Source.Direction == ConnectorDirection.Left && Sink.Direction == ConnectorDirection.Right)
                    {
                        if (startPoint.X > endPoint.X)
                            connectionRoute = 2;
                    }


                    switch (connectionRoute)
                    {
                        case 1:
                        case 4:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X), endPoint.Y));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);
                            break;
                        case 2:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, startPoint.Y));
                            Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, endPoint.Y));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);
                            break;

                        case 3:
                        case 5:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X + 20), startPoint.Y));
                            Path.Points.Add(new System.Windows.Point((startPoint.X + 20), endPoint.Y));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[2].X - 20, Path.Points[2].Y - 10);
                            break;
                        case 8:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X), startPoint.Y - 20));
                            Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, startPoint.Y - 20));
                            Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, endPoint.Y));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[2].X - 20, Path.Points[2].Y - 10);
                            break;
                        case 9:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X), startPoint.Y + 20));
                            Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, startPoint.Y + 20));
                            Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, endPoint.Y));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[2].X - 20, Path.Points[2].Y - 10);
                            break;
                        case 10:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point(endPoint.X, startPoint.Y));
                            //Path.Points.Add(new System.Windows.Point((startPoint.X + 20), startPoint.Y));
                            //Path.Points.Add(new System.Windows.Point((startPoint.X + 20), endPoint.Y - 20));
                            //Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y - 20));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);
                            break;
                        case 11:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X), Math.Max(startPoint.Y + 20, endPoint.Y + 20)));
                            Path.Points.Add(new System.Windows.Point((endPoint.X), Math.Max(startPoint.Y + 20, endPoint.Y + 20)));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);
                            break;
                        case 12:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X), (startPoint.Y + endPoint.Y) / 2.0));
                            Path.Points.Add(new System.Windows.Point((endPoint.X), (startPoint.Y + endPoint.Y) / 2.0));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);
                            break;
                        case 13:
                            Path.Points.Add(startPoint);
                            Path.Points.Add(new System.Windows.Point((startPoint.X), Math.Min(startPoint.Y - 20, endPoint.Y - 20)));
                            Path.Points.Add(new System.Windows.Point((endPoint.X), Math.Min(startPoint.Y - 20, endPoint.Y - 20)));
                            Path.Points.Add(endPoint);
                            LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);
                            break;
                        default:
                            if (startPoint.X < endPoint.X)
                            {
                                Path.Points.Add(startPoint);

                                if (Source.Y == -5)
                                {
                                    Path.Points.Add(new System.Windows.Point((startPoint.X), startPoint.Y - 20));
                                    Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, startPoint.Y - 20));
                                }
                                else if (Source.Y == (Source.Owner.Height - 5))
                                {
                                    Path.Points.Add(new System.Windows.Point((startPoint.X), startPoint.Y + 20));
                                    Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, startPoint.Y + 20));
                                }
                                else
                                {
                                    Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, startPoint.Y));
                                }


                                Path.Points.Add(new System.Windows.Point((startPoint.X + endPoint.X) / 2, endPoint.Y));

                                Path.Points.Add(endPoint);
                                LabelPoint = new Point(Path.Points[2].X - 20, Path.Points[2].Y - 10);
                            }
                            if (startPoint.X > endPoint.X)
                            {
                                if (startPoint.Y > endPoint.Y)
                                {
                                    Path.Points.Add(startPoint);

                                    Path.Points.Add(new System.Windows.Point(startPoint.X + 20, startPoint.Y));
                                    Path.Points.Add(new System.Windows.Point(startPoint.X + 20, endPoint.Y - 50));

                                    var off = 20;

                                    if (Sink.X == Sink.Owner.Width - 5)
                                        off *= -1;

                                    Path.Points.Add(new System.Windows.Point(endPoint.X - off, endPoint.Y - 50));
                                    Path.Points.Add(new System.Windows.Point(endPoint.X - off, endPoint.Y));

                                    Path.Points.Add(endPoint);
                                    LabelPoint = new Point(Path.Points[3].X - 20, Path.Points[3].Y - 10);
                                }
                                else
                                {
                                    Path.Points.Add(startPoint);

                                    Path.Points.Add(new System.Windows.Point(startPoint.X + 20, startPoint.Y));
                                    Path.Points.Add(new System.Windows.Point(startPoint.X + 20, endPoint.Y + 50));

                                    var off = 20;

                                    if (Sink.X == Sink.Owner.Width - 5)
                                        off *= -1;

                                    Path.Points.Add(new System.Windows.Point(endPoint.X - off, endPoint.Y + 50));
                                    Path.Points.Add(new System.Windows.Point(endPoint.X - off, endPoint.Y));

                                    Path.Points.Add(endPoint);
                                    LabelPoint = new Point(Path.Points[3].X - 20, Path.Points[3].Y - 10);
                                }

                            }
                            break;
                    }
                    DrawArrowPoint();

                    NotifyOfPropertyChange(() => Path);
                    NotifyOfPropertyChange(() => LabelPoint);             
                }

                if (Source.Owner != null && Sink.Owner == null)
                {
                    System.Windows.Point startPoint = new System.Windows.Point(Source.Owner.X + Source.X + 5, Source.Owner.Y + Source.Y + 5);
                    System.Windows.Point endPoint = new System.Windows.Point(Sink.X + 5, Sink.Y + 5);

                    Path.Points.Clear();
                    Path.Points.Add(startPoint);
                    Path.Points.Add(endPoint);
                    LabelPoint = new Point(Path.Points[1].X - 20, Path.Points[1].Y - 10);

                    NotifyOfPropertyChange(() => Path);
                    NotifyOfPropertyChange(() => LabelPoint);
                
                }



            }
        }
        void DrawArrowPoint()
        {
            System.Windows.Point endPoint = new System.Windows.Point(Sink.Owner.X + Sink.X + 5, Sink.Owner.Y + Sink.Y + 5);
            double headSize = 5;

            if (Sink.Direction == ConnectorDirection.Up)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X - headSize, endPoint.Y - 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X + headSize, endPoint.Y - 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }

            if (Sink.Direction == ConnectorDirection.Down)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X - headSize, endPoint.Y + 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X + headSize, endPoint.Y + 2 * headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }

            if (Sink.Direction == ConnectorDirection.Left)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X - 2 * headSize, endPoint.Y - headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X - 2 * headSize, endPoint.Y + headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }
            if (Sink.Direction == ConnectorDirection.Right)
            {
                Path.Points.Add(new System.Windows.Point(endPoint.X + 2 * headSize, endPoint.Y - headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X + 2 * headSize, endPoint.Y + headSize));
                Path.Points.Add(new System.Windows.Point(endPoint.X, endPoint.Y));
            }
        }

        public ConnectionTypes ConnectionType
        {
            get { return _connectionType; }
            set { _connectionType = value;
                NotifyOfPropertyChange(() => ConnectionType);
                }
        }

        public Point LabelPoint
        {
            get
            {
                return _labelPoint;
            }

            set
            {
                _labelPoint = value; NotifyOfPropertyChange(() => LabelPoint);
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value; NotifyOfPropertyChange(() => IsSelected);
            }
        }

        public Variable Temperature
        {
            get
            {
                return _model?.GetVariable("T");
            }
        }

        public Variable Pressure
        {
            get
            {
                return _model?.GetVariable("p");
            }

        }

        public Variable Massflow
        {
            get
            {
                 return _model?.GetVariable("m"); 
            }
                        
        }
        public FlowsheetObject Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);

                NotifyOfPropertyChange(() => Temperature);
                NotifyOfPropertyChange(() => Pressure);
                NotifyOfPropertyChange(() => Massflow);
                NotifyOfPropertyChange(() => VapourFraction);
            }
        }

        public Variable VapourFraction
        {
            get
            {
                return _model?.GetVariable("VF");
            }

        }


        public string Report
        {
            get
            {
                return _report;
            }

            set
            {
                _report = value; NotifyOfPropertyChange(() => Report);
            }
        }

        public double Thickness
        {
            get
            {
                return _thickness;
            }

            set
            {
                _thickness = value; NotifyOfPropertyChange(() => Thickness); 
            }
        }

        public string Color
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value; NotifyOfPropertyChange(() => Color);
            }
        }

       

        public string DashArray
        {
            get
            {
                return _dashArray;
            }

            set
            {
                _dashArray = value;
                NotifyOfPropertyChange(() => DashArray); 
            }
        }
        #endregion

    }
}
