using Caliburn.Micro;
using Microsoft.Win32;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Infrastructure.Reporting;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.ViewModels
{
    public class ChartEditorViewModel : PropertyChangedBase, IChartViewModel
    {
        ChartModel _model;
        PlotModel _plot;

        public ChartEditorViewModel(ChartModel model)
        {
            Model = model;
            Redraw();

        }

        public void Redraw()
        {
          
            Plot = new PlotModel();
            Plot.DefaultFontSize = Model.DefaultFontSize;
            Plot.LegendFontSize = Model.LegendFontSize;
            Plot.Title = Model.Title;
            if (Model.ShowLegend)
            {
                switch (Model.LegendPosition)
                {
                    case OpenFMSL.Contracts.Infrastructure.Reporting.LegendPosition.TopRight:
                        Plot.LegendPosition = OxyPlot.LegendPosition.RightTop;
                        break;
                    case OpenFMSL.Contracts.Infrastructure.Reporting.LegendPosition.TopLeft:
                        Plot.LegendPosition = OxyPlot.LegendPosition.TopLeft;
                        break;
                    case OpenFMSL.Contracts.Infrastructure.Reporting.LegendPosition.BottomLeft:
                        Plot.LegendPosition = OxyPlot.LegendPosition.BottomLeft;
                        break;
                    case OpenFMSL.Contracts.Infrastructure.Reporting.LegendPosition.BottomRight:
                        Plot.LegendPosition = OxyPlot.LegendPosition.BottomRight;
                        break;
                    default:
                        Plot.LegendPosition = OxyPlot.LegendPosition.RightTop;
                        break;
                }

                Plot.IsLegendVisible = true;
            }

            Plot.Axes.Clear();

            LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, Title = Model.YAxisTitle, AxisTitleDistance=10 };
            if (Model.IsReversedYAxis)
            {
                yAxis.StartPosition = 1;
                yAxis.EndPosition = 0;
            }

            Plot.Axes.Add(yAxis);

            LinearAxis xAxis = new LinearAxis { Position = AxisPosition.Bottom, Title = Model.XAxisTitle, AxisTitleDistance = 10 };
            Plot.Axes.Add(xAxis);

            if (!Model.AutoScaleX)
            {
                xAxis.Minimum = Model.XMin;
                xAxis.Maximum = Model.XMax;
            }

            if (!Model.AutoScaleY)
            {
                yAxis.Minimum = Model.YMin;
                yAxis.Maximum = Model.YMax;
            }

            Plot.Series.Clear();

            foreach (var series in Model.Series)
            {
                switch (series.Type)
                {
                    case SeriesType.StackedBar100:
                        {
                            var line = new BarSeries();
                            line.IsStacked = true;
                            line.Title = series.Name;
                            for (int i = 0; i < series.X.Count; i++)
                            {
                                var item = new BarItem(series.X[i]);

                                line.Items.Add(item);
                            }
                            if (series.ShowInLegend)
                                line.RenderInLegend = true;
                            else
                                line.RenderInLegend = false;

                            Plot.Series.Add(line);
                            break;
                        }

                    case SeriesType.Line:
                        {
                            var line = new LineSeries();
                            line.MarkerType = OxyPlot.MarkerType.Circle;
                            line.Title = series.Name;
                            line.StrokeThickness = series.Thickness;

                            if (series.Color != "Auto")
                            {
                                var seriesColor = System.Drawing.Color.FromName(series.Color);
                                line.Color = OxyColor.FromArgb(seriesColor.A, seriesColor.R, seriesColor.G, seriesColor.B);
                            }


                            switch (series.DashPattern)
                            {
                                case DashPattern.Dash:
                                    //line.BrokenLineStyle = LineStyle.Dash;
                                    line.LineStyle = LineStyle.Dash;
                                    //line.BrokenLineColor = OxyColors.Transparent;
                                    //line.BrokenLineThickness = series.Thickness;         
                                    break;

                                case DashPattern.AlternatingDash:
                                    //line.BrokenLineStyle = LineStyle.LongDash;
                                    //line.BrokenLineColor = OxyColors.Transparent;
                                    //line.BrokenLineThickness = series.Thickness;
                                    line.LineStyle = LineStyle.LongDashDot;
                                    break;

                                case DashPattern.DashDot:
                                    //line.BrokenLineStyle = LineStyle.DashDot;
                                    //line.BrokenLineColor = OxyColors.Transparent;
                                    //line.BrokenLineThickness = series.Thickness;
                                    line.LineStyle = LineStyle.DashDot;
                                    break;
                                case DashPattern.Dotted:
                                    //line.BrokenLineStyle = LineStyle.Dot;
                                    //line.BrokenLineColor = OxyColors.Transparent;
                                    //line.BrokenLineThickness = series.Thickness;
                                    line.LineStyle = LineStyle.Dot;
                                    break;

                                default:
                                    line.LineStyle = LineStyle.Solid;
                                    //line.BrokenLineStyle = LineStyle.Solid;
                                    //line.BrokenLineThickness = 0;
                                    break;
                            }


                            for (int i = 0; i < series.X.Count; i++)
                            {
                                var item = new DataPoint(series.X[i], series.Y[i]);

                                line.Points.Add(item);
                            }
                            if (series.ShowMarker)
                            {
                                line.MarkerSize = 2 * series.Thickness;
                                line.MarkerFill = line.Color;

                            }
                            
                            if (series.ShowInLegend)
                                line.RenderInLegend = true;
                            else
                                line.RenderInLegend = false;

                            Plot.Series.Add(line);
                            break;
                        }

                    case SeriesType.Scatter:
                        {
                            var line = new ScatterSeries();
                            line.Title = series.Name;
                            switch (series.Marker)
                            {
                                case OpenFMSL.Contracts.Infrastructure.Reporting.MarkerType.Circle:
                                    line.MarkerType = OxyPlot.MarkerType.Circle;
                                    break;
                                case OpenFMSL.Contracts.Infrastructure.Reporting.MarkerType.Diamond:
                                    line.MarkerType = OxyPlot.MarkerType.Diamond;
                                    break;
                                case OpenFMSL.Contracts.Infrastructure.Reporting.MarkerType.Square:
                                    line.MarkerType = OxyPlot.MarkerType.Square;
                                    break;
                                default:
                                    line.MarkerType = OxyPlot.MarkerType.Cross;
                                    break;
                            }

                            if (series.Color != "Auto")
                            {
                                var seriesColor = System.Drawing.Color.FromName(series.Color);
                                line.MarkerFill = OxyColor.FromArgb(seriesColor.A, seriesColor.R, seriesColor.G, seriesColor.B);
                            }

                            for (int i = 0; i < series.X.Count; i++)
                            {
                                var item = new ScatterPoint(series.X[i], series.Y[i]);

                                line.Points.Add(item);
                            }

                            if (series.ShowInLegend)
                                line.RenderInLegend = true;
                            else
                                line.RenderInLegend = false;

                            Plot.Series.Add(line);
                            break;
                        }

                    case SeriesType.JacobianStructure:
                        {
                            var line = new OxyPlot.Series.RectangleBarSeries();

                            LinearAxis XAxisReversed = new LinearAxis { Position = AxisPosition.Bottom, Title = "Variables", IntervalLength = 15, StartPosition = 0, EndPosition = 1 };
                            Plot.Axes.Add(XAxisReversed);

                            // Y2 Axis
                            LinearAxis YAxisReversed = new LinearAxis { Position = AxisPosition.Left, Title = "Equations", IntervalLength = 15, StartPosition = 1, EndPosition = 0 };
                            Plot.Axes.Add(YAxisReversed);


                            double currentX = 0;
                            double currentY = 0;
                            line.Title = "Jacobian Structure";
                            //line.TrackerFormatString = "{0}\n{1}: {2} {3}\n{4}: {5} {6}";

                            for (int i = 0; i < series.X.Count; i++)
                            {
                                var item = new RectangleBarItem { X0 = currentX, X1 = currentX + series.X[i], Y0 = currentY, Y1 = currentY + series.Y[i] };
                                line.Items.Add(item);
                                currentX += series.X[i];
                                currentY += series.Y[i];


                                if (series.X[i] != series.Y[i])
                                {
                                    item.Color = OxyColors.OrangeRed;
                                }
                                else
                                    item.Color = OxyColors.DodgerBlue;
                            }
                            Plot.Series.Add(line);
                        }
                        break;

                    default:
                        {
                            var line = new LineSeries();
                        }
                        break;
                }



            }
        }



        static bool ShowSaveFileDialog(string extension, string filter, out string path)
        {
            path = "";
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = extension;
            dlg.Filter = filter;
            dlg.FileName = "chart." + extension;
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                path = dlg.FileName;
                return true;
            }
            return false;
        }

        public void SaveAsPNG()
        {
            string filename;
            try
            {
                if (ShowSaveFileDialog("png", "PNG files|*.png", out filename))
                {
                    //var pngExporter = new OxyPlot.Wpf.PngExporter { Width = (int)Plot.Width, Height = (int)Plot.Height, Background = OxyColors.White };
                    //  OxyPlot.Wpf.PngExporter.Export(Plot, filename, (int)Plot.Width, (int)Plot.Height, OxyColors.White);

                    using (var stream = File.Create(filename))
                    {
                        var exporter = new OxyPlot.Wpf.PngExporter { Width = (int)Plot.Width, Height = (int)Plot.Height, Background = OxyColors.White };
                        exporter.Export(Plot, stream);
                    }

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error");
            }
        }
        public void SaveAsSVG()
        {
            string filename;
            try
            {
                if (ShowSaveFileDialog("svg", "SVG files|*.svg", out filename))
                {
                    //var pngExporter = new OxyPlot.Wpf.PngExporter { Width = (int)Plot.Width, Height = (int)Plot.Height, Background = OxyColors.White };
                    //OxyPlot.Wpf.SvgExporter.Export(Plot, filename, (int)Plot.Width, (int)Plot.Height, OxyColors.White);

                    using (var stream = File.Create(filename))
                    {
                        var exporter = new SvgExporter { Width = (int)Plot.Width, Height = (int)Plot.Height };
                        exporter.Export(Plot, stream);
                    }

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error");
            }
        }

        public ChartModel Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }

        public PlotModel Plot
        {
            get
            {
                return _plot;
            }

            set
            {
                _plot = value;
                NotifyOfPropertyChange(() => Plot);
            }
        }
    }
}
