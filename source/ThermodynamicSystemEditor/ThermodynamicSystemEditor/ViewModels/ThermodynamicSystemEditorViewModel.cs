using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Reporting;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Numerics.Solvers;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicSystemEditor.ViewModels
{
    public enum BinaryAnalysisType { TXY, PXY, XY };

    public class ThermodynamicSystemEditorViewModel : Conductor<IScreen>, IThermodynamicSystemViewModel, IHandle<PersistChangesMessage>
    {
        private readonly IEventAggregator _aggregator;
        private readonly ThermodynamicSystemEntity _source;
        private readonly IThermodynamicSystemImporter _importer;
        private readonly IChartViewModelFactory _chartFactory;
        ThermodynamicSystem _currentSystem;
        MolecularComponent _selectedComponent;

        MolecularComponent _binaryComponent1;
        MolecularComponent _binaryComponent2;


        EvaluatedProperties _selectedFunction;
        List<EvaluatedProperties> _availableFunctionTypes = new List<EvaluatedProperties>();

        IChartViewModel _pureComponentPropertyChart;

        IChartViewModel _binaryAnalysisChart;
        IChartViewModel _enthalpyChart;

        List<CheckableComponent> _componentsForPureAnalysis = new List<CheckableComponent>();

        BinaryAnalysisType _selectedBinaryAnalysis = BinaryAnalysisType.TXY;
        List<BinaryAnalysisType> _binaryAnalysisTypes = new List<BinaryAnalysisType> { BinaryAnalysisType.PXY, BinaryAnalysisType.TXY, BinaryAnalysisType.XY };
        double _pressureForTX = 1000;
        double _temperatureForPX = 25;
        double _minimumTemperature = 0;
        double _maximumTemperature = 300;

        double _minimumTemperatureEnthalpy = 0;
        double _maximumTemperatureEnthalpy = 300;

        TextDocument _scriptDocument;
        public ThermodynamicSystemEditorViewModel(IEventAggregator aggregator, ThermodynamicSystemEntity source, IThermodynamicSystemImporter importer, IChartViewModelFactory chartFactory)
        {
            _aggregator = aggregator;
            _source = source;
            _importer = importer;
            _chartFactory = chartFactory;
            _aggregator.Subscribe(this);
            ScriptDocument = new TextDocument(_source.SourceCode);

            var types = Enum.GetValues(typeof(EvaluatedProperties));
            foreach (var type in types.OfType<EvaluatedProperties>())
                AvailableFunctionTypes.Add(type);
            try
            {
                CurrentSystem = _importer.ImportNeutralFile(_source.SourceCode);
                ComponentsForPureAnalysis = CurrentSystem.Components.Select(c => new CheckableComponent() { Data = c }).ToList();

                
            }
            catch (Exception e)
            {
                _aggregator.PublishOnUIThread(new LogMessage { Sender = this, TimeStamp = DateTime.Now, Channel = LogChannels.Error, MessageText = e.Message });
            }

        }

        public ThermodynamicSystemEntity Source
        {
            get
            {
                return _source;
            }
        }

        public string SourceCode
        {
            get
            {
                return ScriptDocument.Text;
            }


        }

        public TextDocument ScriptDocument
        {
            get
            {
                return _scriptDocument;
            }

            set
            {
                _scriptDocument = value;
                NotifyOfPropertyChange(() => ScriptDocument);
            }
        }

        public ThermodynamicSystem CurrentSystem
        {
            get
            {
                return _currentSystem;
            }

            set
            {
                _currentSystem = value;

                if (value != null)
                {
                    SelectedComponent = AvailableComponents.FirstOrDefault();

                    BinaryComponent1 = AvailableComponents.FirstOrDefault();
                    BinaryComponent2 = AvailableComponents.LastOrDefault();
                }
                NotifyOfPropertyChange(() => CurrentSystem);
                NotifyOfPropertyChange(() => AvailableComponents);
            }
        }


        public List<MolecularComponent> AvailableComponents
        {
            get
            {
                return _currentSystem?.Components;
            }
        }
      
        public MolecularComponent SelectedComponent
        {
            get
            {
                return _selectedComponent;
            }

            set
            {
                _selectedComponent = value;
             
                NotifyOfPropertyChange(() => SelectedComponent);
                NotifyOfPropertyChange(() => AvailableConstants);
                
            }
        }

        public List<Variable> AvailableConstants
        {
            get
            {
                return SelectedComponent?.Constants;
            }
        }

    

        public double MaximumTemperature
        {
            get
            {
                return _maximumTemperature;
            }

            set
            {
                _maximumTemperature = value;
                NotifyOfPropertyChange(() => MaximumTemperature);
            }
        }

        public double MinimumTemperature
        {
            get
            {
                return _minimumTemperature;
            }

            set
            {
                _minimumTemperature = value;
                NotifyOfPropertyChange(() => MinimumTemperature);
            }
        }

        public IChartViewModel PureComponentPropertyChart
        {
            get
            {
                return _pureComponentPropertyChart;
            }

            set
            {
                _pureComponentPropertyChart = value;
                NotifyOfPropertyChange(() => PureComponentPropertyChart);
            }
        }

        public EvaluatedProperties SelectedFunctionType
        {
            get
            {
                return _selectedFunction;
            }

            set
            {
                _selectedFunction = value;
                NotifyOfPropertyChange(() => SelectedFunctionType);
            }
        }

        public MolecularComponent BinaryComponent1
        {
            get
            {
                return _binaryComponent1;
            }

            set
            {
                _binaryComponent1 = value;
                NotifyOfPropertyChange(() => BinaryComponent1);
            }
        }

        public MolecularComponent BinaryComponent2
        {
            get
            {
                return _binaryComponent2;
            }

            set
            {
                _binaryComponent2 = value;
                NotifyOfPropertyChange(() => BinaryComponent2);
            }
        }

        public double PressureForTX
        {
            get
            {
                return _pressureForTX;
            }

            set
            {
                _pressureForTX = value;
                NotifyOfPropertyChange(() => PressureForTX);
            }
        }

        public double TemperatureForPX
        {
            get
            {
                return _temperatureForPX;
            }

            set
            {
                _temperatureForPX = value;
                NotifyOfPropertyChange(() => TemperatureForPX);
            }
        }

        public BinaryAnalysisType SelectedBinaryAnalysis
        {
            get
            {
                return _selectedBinaryAnalysis;
            }

            set
            {
                _selectedBinaryAnalysis = value;
                NotifyOfPropertyChange(() => SelectedBinaryAnalysis);
            }
        }

        public List<BinaryAnalysisType> BinaryAnalysisTypes
        {
            get
            {
                return _binaryAnalysisTypes;
            }

            set
            {
                _binaryAnalysisTypes = value;
                NotifyOfPropertyChange(() => BinaryAnalysisTypes);
            }
        }

        public IChartViewModel BinaryAnalysisChart
        {
            get
            {
                return _binaryAnalysisChart;
            }

            set
            {
                _binaryAnalysisChart = value;
                NotifyOfPropertyChange(() => BinaryAnalysisChart);
            }
        }

        public double MinimumTemperatureEnthalpy
        {
            get
            {
                return _minimumTemperatureEnthalpy;
            }

            set
            {
                _minimumTemperatureEnthalpy = value;
                NotifyOfPropertyChange(() => MinimumTemperatureEnthalpy);
            }
        }

        public double MaximumTemperatureEnthalpy
        {
            get
            {
                return _maximumTemperatureEnthalpy;
            }

            set
            {
                _maximumTemperatureEnthalpy = value;
                NotifyOfPropertyChange(() => MaximumTemperatureEnthalpy);
            }
        }

        public IChartViewModel EnthalpyChart
        {
            get
            {
                return _enthalpyChart;
            }

            set
            {
                _enthalpyChart = value;
                NotifyOfPropertyChange(() => EnthalpyChart);
            }
        }

        public List<EvaluatedProperties> AvailableFunctionTypes
        {
            get
            {
                return _availableFunctionTypes;
            }

            set
            {
                _availableFunctionTypes = value;

                NotifyOfPropertyChange(() => AvailableFunctionTypes);
            }
        }

        public List<CheckableComponent> ComponentsForPureAnalysis
        {
            get
            {
                return _componentsForPureAnalysis;
            }

            set
            {
                _componentsForPureAnalysis = value;
            }
        }

        public void RedrawBinaryAnalysisChart()
        {
            if (BinaryComponent1 == BinaryComponent2)
                return;

            Task.Factory.StartNew(() => RedrawBinaryAnalysisCharts());
        }

        void RedrawBinaryAnalysisCharts()
        {
            var chart = new ChartModel();
            chart.Title = SelectedBinaryAnalysis.ToString() + " [" + BinaryComponent1.ID + " /" + BinaryComponent2.ID + "]";
            chart.XAxisTitle = "Molar Composition " + BinaryComponent1.ID + " [mol/mol]";

            chart.XMin = 0;
            chart.XMax = 1;
            chart.AutoScaleY = true;
            int steps = 31;

            List<double> y1Values = new List<double>();
            List<double> y2Values = new List<double>();
            List<double> x1Values = new List<double>();
            List<double> x2Values = new List<double>();

            var eval = new Evaluator();

            var stream = new MaterialStream("S1", CurrentSystem);
            foreach (var comp in CurrentSystem.Components)
            {
                if (comp != BinaryComponent1 && comp != BinaryComponent2)
                    stream.Specify("n[" + comp.ID + "]", 0.0);
            }
            var solver = new Newton();
            var flashAlgo = new FlashRoutines(solver);


            switch (SelectedBinaryAnalysis)
            {
                case BinaryAnalysisType.PXY:
                    {
                        chart.YAxisTitle = "Pressure [mbar]";
                        chart.Title += " at " + TemperatureForPX + " °C";

                        stream.Specify("T", TemperatureForPX, METRIC.C);
                        stream.Specify("p", 1000, METRIC.mbar);
                        stream.Specify("n[" + BinaryComponent1.ID + "]", 0.0);
                        stream.Specify("n[" + BinaryComponent2.ID + "]", 1.0);

                        flashAlgo.CalculateTP(stream);
                        stream.Unspecify("p");
                        stream.Specify("VF", 0);
                        var equationSystem = GetEquationSystem(stream);

                        for (int i = 0; i < steps; i++)
                        {
                            var x1 = (double)i / (double)(steps - 1);
                            var x2 = 1.0 - x1;
                          
                            stream.Specify("n[" + BinaryComponent1.ID + "]", x1);
                            stream.Specify("n[" + BinaryComponent2.ID + "]", x2);

                            solver.Solve(equationSystem);
                            eval.Reset();

                            x1Values.Add(x1);
                            y1Values.Add(stream.GetVariable("p").ValueInSI / 100.0);
                            
                        }
                        stream.Specify("VF", 1);

                        for (int i = 0; i < steps; i++)
                        {
                            var x1 = (double)i / (double)(steps - 1);
                            var x2 = 1.0 - x1;
                           
                            stream.Specify("n[" + BinaryComponent1.ID + "]", x1);
                            stream.Specify("n[" + BinaryComponent2.ID + "]", x2);
                            solver.Solve(equationSystem);
                            eval.Reset();
                            x2Values.Add(x1);
                            y2Values.Add(stream.GetVariable("p").ValueInSI / 100.0);                            
                        }

                    }
                    break;
                case BinaryAnalysisType.TXY:
                    {
                        chart.YAxisTitle = "Temperature [°C]";
                        chart.Title += " at " + PressureForTX + " mbar";

                        stream.Specify("T", 25, METRIC.C);
                        stream.Specify("p", PressureForTX, METRIC.mbar);
                        stream.Specify("n[" + BinaryComponent1.ID + "]", 0.0);
                        stream.Specify("n[" + BinaryComponent2.ID + "]", 1.0);

                        flashAlgo.CalculateTP(stream);
                        stream.Unspecify("T");
                        stream.Specify("VF", 0);
                        var equationSystem = GetEquationSystem(stream);

                        for (int i = 0; i < steps; i++)
                        {
                            var x1 = (double)i / (double)(steps - 1);
                            var x2 = 1.0 - x1;
                            stream.Specify("VF", 0);
                            stream.Specify("n[" + BinaryComponent1.ID + "]", x1);
                            stream.Specify("n[" + BinaryComponent2.ID + "]", x2);

                            solver.Solve(equationSystem);
                            eval.Reset();

                            x1Values.Add(x1);
                            y1Values.Add(stream.GetVariable("T").ValueInSI - 273.15);

                            stream.Specify("VF", 1.0);
                            solver.Solve(equationSystem);
                            eval.Reset();
                            x2Values.Add(x1);
                            y2Values.Add(stream.GetVariable("T").ValueInSI - 273.150);

                        }
                    }
                    break;
                case BinaryAnalysisType.XY:
                    {
                        chart.YAxisTitle = "Molar Composition Vapor [mol/mol]";
                        chart.Title += " at " + PressureForTX + " mbar";

                        stream.Specify("T", 25, METRIC.C);
                        stream.Specify("p", PressureForTX, METRIC.mbar);
                        stream.Specify("n[" + BinaryComponent1.ID + "]", 0.0);
                        stream.Specify("n[" + BinaryComponent2.ID + "]", 1.0);

                        flashAlgo.CalculateTP(stream);
                        stream.Specify("VF", 0);
                        stream.Unspecify("T");
                        var equationSystem = GetEquationSystem(stream);


                        for (int i = 0; i < steps; i++)
                        {
                            var x1 = (double)i / (double)(steps - 1);
                            var x2 = 1.0 - x1;
                            stream.Specify("n[" + BinaryComponent1.ID + "]", x1);
                            stream.Specify("n[" + BinaryComponent2.ID + "]", x2);
                            solver.Solve(equationSystem);
                            eval.Reset();
                            x1Values.Add(x1);
                            y1Values.Add(stream.GetVariable("xV[" + BinaryComponent1.ID + "]").ValueInSI);
                            x2Values.Add(x2);
                            y2Values.Add(stream.GetVariable("xV[" + BinaryComponent2.ID + "]").ValueInSI);

                        }
                    }
                    break;
            }



            var ySeries1 = new SeriesModel(BinaryComponent1.ID, SeriesType.Line, x1Values, y1Values, "Red");
            var ySeries2 = new SeriesModel(BinaryComponent2.ID, SeriesType.Line, x2Values, y2Values, "Red");
            chart.Series.Add(ySeries1);
            chart.Series.Add(ySeries2);
            BinaryAnalysisChart = _chartFactory.Create(chart);
        }

        EquationSystem GetEquationSystem(MaterialStream stream)
        {
            var eq = new EquationSystem();
            eq.TreatFixedVariablesAsConstants = true;
            stream.FillEquationSystem(eq);
            return eq;
        }
      

        public void RedrawEnthalpyChart()
        {
            var chart = new ChartModel();
            chart.Title = "Enthalpy Curves" + " [" + SelectedComponent.ID + "]";
            chart.XAxisTitle = "Temperature [°C]";
            chart.YAxisTitle = "Specific Enthalpy" + " [kJ/mol]";
            chart.XMin = MinimumTemperatureEnthalpy;
            chart.XMax = MaximumTemperatureEnthalpy;
            chart.AutoScaleY = true;
            int steps = 41;

            var T = CurrentSystem.VariableFactory.CreateVariable("T", "Temperature",PhysicalDimension.Temperature);
            T.OutputUnit = METRIC.C;
            var hV = CurrentSystem.EquationFactory.GetVaporEnthalpyExpression(CurrentSystem, CurrentSystem.Components.IndexOf(SelectedComponent), T);
            var hL = CurrentSystem.EquationFactory.GetLiquidEnthalpyExpression(CurrentSystem, CurrentSystem.Components.IndexOf(SelectedComponent), T);
            var eval = new Evaluator();
            List<double> y1Values = new List<double>();
            List<double> y2Values = new List<double>();
            List<double> xValues = new List<double>();
            for (int i = 0; i < steps; i++)
            {
                eval.Reset();
                T.SetValue(MinimumTemperatureEnthalpy + (MaximumTemperatureEnthalpy - MinimumTemperatureEnthalpy) / (double)steps * i, METRIC.C);
                var y1 = hV.Eval(eval)/1e3;
                var y2 = hL.Eval(eval) / 1e3;
      
                xValues.Add(T.ValueInOutputUnit);                
                y1Values.Add(y1);
                y2Values.Add(y2);

            }
            var y1Series = new SeriesModel("hV", SeriesType.Line, xValues, y1Values, "Red");
            var y2Series = new SeriesModel("hL", SeriesType.Line, xValues, y2Values, "Blue");
            chart.Series.Add(y1Series);
            chart.Series.Add(y2Series);
            EnthalpyChart = _chartFactory.Create(chart);
        }

       
        public void RedrawPureComponentChart()
        {
         
            var chart = new ChartModel();
            chart.Title = SelectedFunctionType.ToString();
            chart.XAxisTitle = "Temperature [°C]";
            chart.XMin = MinimumTemperature;
            chart.XMax = MaximumTemperature;
            chart.ShowLegend = true;
            chart.AutoScaleY = true;
            int steps = 41;

            foreach (var comp in ComponentsForPureAnalysis.Where(c => c.IsChecked).Select(c=>c.Data))
            {
                var SelectedFunction = comp.GetFunction(SelectedFunctionType);
                chart.YAxisTitle = SelectedFunctionType.ToString() + " [" + SelectedFunction.YUnit + "]";

                // RedrawPureComponent(comp.Data);
                var T = CurrentSystem.VariableFactory.CreateVariable("T", "Temperature", PhysicalDimension.Temperature);
                T.OutputUnit = METRIC.C;
                var funcExpr = CurrentSystem.CorrelationFactory.CreateExpression(SelectedFunction.Type, SelectedFunction, T, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
                double maxVal = 1e20;
                if(SelectedFunction.Property== EvaluatedProperties.VaporPressure)
                {
                    var funcExprMax = CurrentSystem.CorrelationFactory.CreateExpression(SelectedFunction.Type, SelectedFunction, comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalTemperature), comp.GetConstant(ConstantProperties.CriticalPressure));
                    maxVal= funcExprMax.Eval(new Evaluator());
                }
                var eval = new Evaluator();
                List<double> yValues = new List<double>();
                List<double> xValues = new List<double>();
                for (int i = 0; i < steps; i++)
                {
                    eval.Reset();
                    T.SetValue(MinimumTemperature + (MaximumTemperature - MinimumTemperature) / (double)steps * i, METRIC.C);
                    var y = funcExpr.Eval(eval);
                    if (Double.IsNaN(y))
                        y = 0;

                    if (y > maxVal)
                        y = maxVal;

                    xValues.Add(T.ValueInOutputUnit);
                    yValues.Add(y);

                }
                var ySeries = new SeriesModel(comp.ID + " " + SelectedFunction.Property.ToString(), SeriesType.Line, xValues, yValues, "Red");
                chart.Series.Add(ySeries);
            }


           
            PureComponentPropertyChart = _chartFactory.Create(chart);
        }
        public void Handle(PersistChangesMessage message)
        {
            Source.SourceCode = ScriptDocument.Text;
        }
    }

}
