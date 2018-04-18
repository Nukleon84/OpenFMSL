using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Core.Numerics;

namespace OpenFMSL.Core.ModelLibrary
{
    public enum FlowPattern { CounterCurrent, CoCurrent };
    public enum SolveMode { Initialize, Basic, Full };
    public class HeatExchangerCell : Heater
    {
        public HeatExchangerCell(string name, ThermodynamicSystem system) : base(name, system)
        {
            Class = "HexCell";
        }
    }
    public class ShellAndTubeHeatExchanger : ProcessUnit
    {
        readonly int _numberOfPasses = 1;
        readonly int _discretization = 1;
        Variable _area;
        Variable _koverall;
        FlowPattern _flowPattern = FlowPattern.CounterCurrent;
        List<HeatExchangerCell> _shell = new List<HeatExchangerCell>();
        List<HeatExchangerCell> _tube = new List<HeatExchangerCell>();
        List<MaterialStream> _shellStreams = new List<MaterialStream>();
        List<MaterialStream> _tubeStreams = new List<MaterialStream>();
        List<HeatStream> _qExchanged = new List<HeatStream>();
 
        public int Discretization
        {
            get
            {
                return _discretization;
            }
        }

        public int NumberOfPasses
        {
            get
            {
                return _numberOfPasses;
            }
        }

        public List<HeatExchangerCell> Shell
        {
            get
            {
                return _shell;
            }

            set
            {
                _shell = value;
            }
        }

        public List<HeatExchangerCell> Tube
        {
            get
            {
                return _tube;
            }

            set
            {
                _tube = value;
            }
        }

        public List<MaterialStream> ShellStreams
        {
            get
            {
                return _shellStreams;
            }

            set
            {
                _shellStreams = value;
            }
        }

        public List<MaterialStream> TubeStreams
        {
            get
            {
                return _tubeStreams;
            }

            set
            {
                _tubeStreams = value;
            }
        }

        public List<HeatStream> QExchanged
        {
            get
            {
                return _qExchanged;
            }

            set
            {
                _qExchanged = value;
            }
        }

        public FlowPattern FlowPattern
        {
            get
            {
                return _flowPattern;
            }

            set
            {
                _flowPattern = value;
            }
        }

        public ShellAndTubeHeatExchanger(string name,  ThermodynamicSystem system, int passes, int discretization) : base(name, system)
        {
            Class = "Shell&Tube";
            Icon.IconType = IconTypes.HeatExchanger;
            _numberOfPasses = passes;
            _discretization = discretization;

            MaterialPorts.Add(new Port<MaterialStream>("ShellIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("ShellOut", PortDirection.Out, 1));

            MaterialPorts.Add(new Port<MaterialStream>("TubeIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("TubeOut", PortDirection.Out, 1));

            _area = system.VariableFactory.CreateVariable("A", "Area", PhysicalDimension.Area);
            _koverall = system.VariableFactory.CreateVariable("k", "Effective Heat Transfer Coefficient", PhysicalDimension.HeatTransferCoefficient);
            AddVariables(_area, _koverall);
         

        }
        public ShellAndTubeHeatExchanger Initialize(double shellSideDT, double tubeSideDT, double shellSideDP, double tubeSideDP)
        {
            var numTotalCells = NumberOfPasses * Discretization;

            for (int i = 0; i < numTotalCells; i++)
            {
                Shell[i].Specify("DP", shellSideDP / numTotalCells, METRIC.mbar);
                Shell[i].Specify("T", ShellStreams[i].Mixed.Temperature.ValueInSI + shellSideDT / numTotalCells, SI.K);
                
                Shell[i].Initialize();

                Tube[i].Specify("DP", tubeSideDP / numTotalCells, METRIC.mbar);
                Tube[i].Specify("T", TubeStreams[i].Mixed.Temperature.ValueInSI + tubeSideDT / numTotalCells, SI.K);
                Tube[i].Initialize();
            }
            for (int i = 0; i < numTotalCells; i++)
            {
                Shell[i].Unspecify("T");
                Tube[i].Unspecify("T");
               

                QExchanged[i].Q.ValueInSI = Tube[i].GetVariable("Q").ValueInSI;
            }
            return this;
        }
        public override ProcessUnit Initialize()
        {
            Initialize(10, -10, 10, 10);
            return this;
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;
            var numTotalCells = NumberOfPasses * Discretization;

            for (int i = 1; i < numTotalCells ; i++)
            {
                ShellStreams[i].FillEquationSystem(problem);
                TubeStreams[i].FillEquationSystem(problem);

            }

            for (int i = 0; i < numTotalCells; i++)
            {
                Shell[i].FillEquationSystem(problem);
                Tube[i].FillEquationSystem(problem);
                QExchanged[i].FillEquationSystem(problem);

                var DT = Sym.Par(TubeStreams[i].Mixed.Temperature - ShellStreams[i].Mixed.Temperature);                
                AddEquationToEquationSystem(problem, QExchanged[i].Q.IsEqualTo(_koverall * _area/numTotalCells * DT));
            }


            base.FillEquationSystem(problem);
        }
        public ShellAndTubeHeatExchanger SetFlowPattern(FlowPattern pattern)
        {
            FlowPattern = pattern;
            return this;
        }

        public ShellAndTubeHeatExchanger Configure()
        {
            if (!FindMaterialPort("TubeIn").IsConnected ||
                !FindMaterialPort("TubeOut").IsConnected ||
                !FindMaterialPort("ShellIn").IsConnected ||
                !FindMaterialPort("ShellOut").IsConnected)
                throw new InvalidOperationException("Shell&Tube heat Exchanger not connected correctly");

        
            var TubeIn = FindMaterialPort("TubeIn").Streams[0];
            var TubeOut = FindMaterialPort("TubeOut").Streams[0];

            var ShellIn = FindMaterialPort("ShellIn").Streams[0];
            var ShellOut = FindMaterialPort("ShellOut").Streams[0];

            var shellSystem = ShellIn.System;
            var tubeSystem = TubeIn.System;

            TubeStreams.Add(TubeIn);
            ShellStreams.Add(ShellIn);
            var numTotalCells = NumberOfPasses * Discretization;
            for (int i = 0; i < numTotalCells; i++)
            {
                if (i < numTotalCells - 1)
                {
                    TubeStreams.Add(new MaterialStream(Name+".TubeStream[" + (i + 1) + "]", tubeSystem));
                    ShellStreams.Add(new MaterialStream(Name + ".ShellStream[" + (i + 1) + "]", shellSystem));
                }
                else
                {
                    TubeStreams.Add(TubeOut);
                    ShellStreams.Add(ShellOut);
                }


                QExchanged.Add(new HeatStream(Name + ".Q[" + (i + 1) + "]", System));

                var currentTubeCell = new HeatExchangerCell(Name + ".TubeWall[" + (i + 1) + "]", tubeSystem);
                Tube.Add(currentTubeCell);
                var currentShellCell = new HeatExchangerCell(Name + ".ShellWall[" + (i + 1) + "]", shellSystem);
                Shell.Add(currentShellCell);

                currentTubeCell.Connect("In", TubeStreams[i]);
                currentShellCell.Connect("In", ShellStreams[i]);

                if (i < numTotalCells - 1)
                {
                    currentTubeCell.Connect("Out", TubeStreams[i + 1]);
                    currentShellCell.Connect("Out", ShellStreams[i + 1]);
                }
                else
                {
                    currentTubeCell.Connect("Out", TubeStreams[i + 1]);
                    currentShellCell.Connect("Out", ShellStreams[i + 1]);
                }


            }

            switch (FlowPattern)
            {
                case FlowPattern.CounterCurrent:
                    for (int i = 0; i < numTotalCells; i++)
                    {
                        Tube[i].Connect("Duty", QExchanged[i]);
                        Tube[i].FindHeatPort("Duty").Direction = PortDirection.Out;
                        Shell[numTotalCells - i - 1].Connect("Duty", QExchanged[i]);
                    }
                    break;
                case FlowPattern.CoCurrent:
                    for (int i = 0; i < numTotalCells; i++)
                    {
                        Tube[i].Connect("Duty", QExchanged[i]);
                        Tube[i].FindHeatPort("Duty").Direction = PortDirection.Out;
                        Shell[i].Connect("Duty", QExchanged[i]);
                    }
                    break;
                default:
                    throw new NotSupportedException("Flow pattern " + FlowPattern + " is not supported.");
            }
            return this;

        }

    }
}
