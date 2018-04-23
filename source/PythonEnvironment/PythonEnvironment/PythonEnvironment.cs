using Caliburn.Micro;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using OpenFMSL.Contracts.Documents;
using OpenFMSL.Contracts.Entities;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Contracts.Infrastructure.Reporting;
using OpenFMSL.Contracts.Infrastructure.Scripting;
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
using System.Threading;
using System.Threading.Tasks;

namespace PythonEnvironment
{
    public class PythonEnvironmentModule : IPythonEnvironment
    {
        private readonly IEventAggregator _aggregator;
        private readonly IEntityManagerViewModel _entityManager;
        private readonly IThermodynamicSystemImporter _importer;
        private readonly IChartViewModelFactory _chartFactory;
        private readonly IFlowsheetEntityEditorFactory _flowsheetFactory;

        Action<string> _onWrite;

        private readonly ScriptEngine _pyEngine = null;
        private readonly ScriptScope _pyScope = null;
        Newton newton;
        Decomposer decomp;
        IpoptSolver ipopt;

        public PythonEnvironmentModule(IEventAggregator aggregator, IEntityManagerViewModel entityManager,
            IThermodynamicSystemImporter importer,
            IChartViewModelFactory chartFactory,
            IFlowsheetEntityEditorFactory flowsheetFactory
            )
        {
            _aggregator = aggregator;
            _entityManager = entityManager;
            _importer = importer;
            _chartFactory = chartFactory;
            _flowsheetFactory = flowsheetFactory;
            _pyEngine = Python.CreateEngine();
            _pyScope = _pyEngine.CreateScope();


            _pyScope.SetVariable("_host", this);
            _pyScope.SetVariable("Items", _entityManager);
            _pyEngine.SetSearchPaths(new List<string> { Environment.CurrentDirectory });
            var pc = HostingHelpers.GetLanguageContext(_pyEngine) as PythonContext;
            var hooks = pc.SystemState.Get__dict__()["path_hooks"] as List;
            hooks.Clear();
            Run("import sys");
            Run("import clr");
            Run("clr.AddReferenceToFile(\"OpenFMSL.Core.dll\")");
            Run("clr.AddReferenceToFile(\"OpenFMSL.Contracts.dll\")");

            Run("from OpenFMSL.Core.Expressions import *");
            Run("from OpenFMSL.Core.Flowsheeting import *");
            Run("from OpenFMSL.Core.Numerics import *");
            Run("from OpenFMSL.Core.UnitsOfMeasure import *");
            Run("from OpenFMSL.Core.ModelLibrary import *");

            Run("from OpenFMSL.Contracts.Entities import *");
            Run("from OpenFMSL.Contracts.Infrastructure.Reporting import *");

            Run("from System import Math");
            Run("sys.stdout=_host");
            Run("runFile= _host.RunFile");
            Run("run= _host.RunEntity");
            Run("pause= _host.WaitThread");
            Run("CreateThermo= _host.LoadThermodynamicSystem");

            ipopt = new IpoptSolver();
            ipopt.OnLog = (x) => Write("    " + x + Environment.NewLine);
            _pyScope.SetVariable("_ipopt", ipopt);

            newton = new Newton();
            newton.OnLog = (x) => Write("    " + x + Environment.NewLine);
            newton.OnLogDebug = (x) => Write(x + Environment.NewLine);
            newton.OnLogError = (x) => Write("!!! " + x + Environment.NewLine);
            newton.OnLogSuccess = (x) => Write("+++ " + x + Environment.NewLine);
            newton.OnLogWarning = (x) => Write("*** " + x + Environment.NewLine);
            newton.OnLogInfo = (x) => Write("--- " + x + Environment.NewLine);
            _pyScope.SetVariable("_newton", newton);

            var flash = new FlashRoutines(newton);
            _pyScope.SetVariable("_flash", flash);

            decomp = new Decomposer();
            decomp.OnLog = (x) => Write("    " + x + Environment.NewLine);
            decomp.OnLogDebug = (x) => Write(x + Environment.NewLine);
            decomp.OnLogError = (x) => Write("!!! " + x + Environment.NewLine);
            decomp.OnLogSuccess = (x) => Write("+++ " + x + Environment.NewLine);
            decomp.OnLogWarning = (x) => Write("*** " + x + Environment.NewLine);
            decomp.OnLogInfo = (x) => Write("--- " + x + Environment.NewLine);

            _pyScope.SetVariable("_decomp", decomp);

            Run("solve= _host.Solve");
            Run("decomp= _host.Decompose");
            Run("report= _host.Report");

            Run("show= _host.Show");
            Run("check= _host.Check");

            Run("info= _host.SendLogMessage");
            Run("warn= _host.SendWarningMessage");
            Run("error= _host.SendErrorMessage");

            Run("FlashPT= _flash.CalculateTP");
            Run("FlashPZ= _flash.CalculateZP");



            Run("status= _host.SendStatusTextChangeMessage");

            Run("print 'Python console running...");

        }

        public Action<string> OnWrite
        {
            get
            {
                return _onWrite;
            }

            set
            {
                _onWrite = value;
            }
        }

        void WriteLine(string message)
        {
            Write("" + message + Environment.NewLine);
        }
        void Write(string message)
        {
            OnWrite?.Invoke(message);
        }
        public void write(string message)
        {
            Write(message);
        }

        public object GetObject(string name)
        {
            if (_pyScope.ContainsVariable(name))
                return _pyScope.GetVariable(name);
            else
                return null;
        }

        public void InjectObject(string name, object instance)
        {
            _pyScope.SetVariable(name, instance);
        }
        public void WaitThread(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
        public void Show(ChartModel chart)
        {
            var vm = _chartFactory.Create(chart);

            _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = chart.Title, Parameter = vm });
        }

        public void Show(Chart chart)
        {
            var vm = _chartFactory.Create(chart);

            _aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = chart.Model.Title, Parameter = vm });
        }
        public void Show(Snapshot snap)
        {
            //var vm = _chartFactory.Create(chart);

            _aggregator.PublishOnUIThread(new RequestEntityEditorMessage { Target = snap });
        }

        public void Show(Flowsheet flow)
        {
            //var vm = _flowsheetFactory.Create();
            var entity = new FlowsheetEntity(flow.Name, flow);
            _aggregator.PublishOnUIThread(new RequestEntityEditorMessage { Target = entity });
            //_aggregator.PublishOnUIThread(new AddNewDocumentMessage { TimeStamp = DateTime.Now, Sender = this, Title = flow.Name, Parameter = vm });
        }



        public void Report(ThermodynamicSystem system)
        {
            WriteLine("");
            WriteLine("Report for thermodynamic system " + system.Name);
            WriteLine("================================================");

            WriteLine("");
            WriteLine("Equilibrium");
            WriteLine("");

            WriteLine("VLEQ Method      : " + system.EquilibriumMethod.EquilibriumApproach);
            WriteLine("Activity Method  : " + system.EquilibriumMethod.Activity);
            WriteLine("Fugacity Method  : " + system.EquilibriumMethod.Fugacity);
            WriteLine("Henry Method     : " + system.EquilibriumMethod.AllowHenryComponents);

            WriteLine("");
            WriteLine("Unit of Measure");
            WriteLine("");
            WriteLine(String.Format("{0,-25} {1,-15} {2,-15}", "Dimension", "Input", "Output"));

            foreach (var unit in system.VariableFactory.Internal.UnitDictionary)
            {
                WriteLine(String.Format("{0,-25} {1,-15} {2,-15}", unit.Key, unit.Value, system.VariableFactory.Output.UnitDictionary[unit.Key]));

            }

            WriteLine("");
            WriteLine("Components");
            WriteLine("");

            WriteLine(String.Format("{0,-25} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15} {6,-15}", "Name", "ID", "CAS-No", "Inert", "MOLW", "TC", "PC"));
            foreach (var comp in system.Components)
            {
                WriteLine(String.Format("{0,-25} {1,-15} {2,-15} {3,-15} {4,-15} {5,-15} {6,-15}",
                    comp.Name,
                    comp.ID,
                    comp.CasNumber,
                    comp.IsInert,
                    comp.GetConstant(ConstantProperties.MolarWeight).ValueInSI.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    comp.GetConstant(ConstantProperties.CriticalTemperature).ValueInSI.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    comp.GetConstant(ConstantProperties.CriticalPressure).ValueInSI.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo)));
            }
            WriteLine("");
            WriteLine("Enthalpy Functions");
            WriteLine("");
            WriteLine(String.Format("{0,-15} {1,-10} {2,-12} {3,-8} {4,-8} {5,-5}", "Comp", "Phase", "Href", "Tref", "TPc", "Fixed Phase Change"));
            foreach (var enth in system.EnthalpyMethod.PureComponentEnthalpies)
            {
                WriteLine(String.Format("{0,-15} {1,-10} {2,-12} {3,-8} {4,-8} {5,-5}",
                    enth.Component.ID,
                    enth.ReferenceState,
                    enth.Href.ValueInOutputUnit.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    enth.Tref.ValueInOutputUnit.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    enth.TPhaseChange.ValueInOutputUnit.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    !enth.PhaseChangeAtSystemTemperature));

            }


            WriteLine("");
            WriteLine("Property Functions");
            WriteLine("");
            WriteLine(String.Format("{0,-15} {1,-25} {2,-15} {3,-8} {4,-8} {5,-5} {6,-25}", "Comp", "Property", "Form", "Min T", "Max T", "Coeff", "Equation"));
            foreach (var comp in system.Components)
            {
                foreach (var func in comp.Functions)
                {
                    WriteLine(String.Format("{0,-15} {1,-25} {2,-15} {3,-8} {4,-8} {5,-5} {6,-25}",
                        comp.ID,
                        func.Property,
                        func.Type,
                        func.MinimumX.ValueInSI.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                        func.MaximumX.ValueInSI.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                        func.Coefficients.Count,
                        "Y = " + system.CorrelationFactory.CreateExpression(func.Type, func, new OpenFMSL.Core.Expressions.Variable("T", 1), new OpenFMSL.Core.Expressions.Variable("TC", 1), new OpenFMSL.Core.Expressions.Variable("PC", 1)).ToString()));

                }
            }


        }

        public OptimizationProblem Solve(OptimizationProblem problem)
        {
            ipopt.Solve(problem);
            return problem;
        }

        public EquationSystem Solve(Flowsheet flowsheet, double brakeFactor, int maxIter)
        {
            var equationSystem = new EquationSystem();
            equationSystem.Name = flowsheet.Name;
            flowsheet.FillEquationSystem(equationSystem);
            var oldState = newton.DoLinesearch;
            var oldFactor = newton.BrakeFactor;
            var oldIter = newton.MaximumIterations;
            newton.DoLinesearch = false;
            newton.MaximumIterations = maxIter;
            newton.BrakeFactor = brakeFactor;
            newton.Solve(equationSystem);

            newton.MaximumIterations = oldIter;
            newton.BrakeFactor = oldFactor;
            newton.DoLinesearch = oldState;
            return equationSystem;
        }


        public EquationSystem Solve(Flowsheet flowsheet, double brakeFactor)
        {
            var equationSystem = new EquationSystem();
            equationSystem.Name = flowsheet.Name;
            flowsheet.FillEquationSystem(equationSystem);
            var oldState = newton.DoLinesearch;
            var oldFactor = newton.BrakeFactor;
            newton.DoLinesearch = false;
            newton.BrakeFactor = brakeFactor;
            newton.Solve(equationSystem);

            newton.BrakeFactor = oldFactor;
            newton.DoLinesearch = oldState;
            return equationSystem;
        }

        public EquationSystem Solve(Flowsheet flowsheet)
        {
            var equationSystem = new EquationSystem();
            equationSystem.Name = flowsheet.Name;
            flowsheet.FillEquationSystem(equationSystem);
            newton.Solve(equationSystem);
            return equationSystem;
        }

        public EquationSystem Solve(EquationSystem equationSystem)
        {
            newton.Solve(equationSystem);
            return equationSystem;
        }

        public EquationSystem Decompose(Flowsheet flowsheet)
        {
            var equationSystem = new EquationSystem();
            equationSystem.Name = flowsheet.Name;
            flowsheet.FillEquationSystem(equationSystem);
            decomp.Solve(equationSystem);
            return equationSystem;
        }
        public EquationSystem Decompose(EquationSystem equationSystem)
        {
            decomp.Solve(equationSystem);
            return equationSystem;
        }


        public void Check(Flowsheet flowsheet)
        {
            WriteLine("");
            WriteLine("Equation system summary for flowsheet " + flowsheet.Name);
            WriteLine("================================================");
            WriteLine("");


            WriteLine(String.Format("{0,-15} {1,-15} {2,6} {3,6} {4,6} {5,6}", "Name", "Model", "Vars", "Eqs", "Fixed", "Sum"));
            WriteLine("");
            var eqsys = new EquationSystem();

            int lastEquations = 0;
            int lastVariables = 0;
            int lastFixedVariables = 0;

            foreach (var stream in flowsheet.MaterialStreams)
            {
                stream.FillEquationSystem(eqsys);

                var numberOfVariables = eqsys.Variables.Where(v => v.DefiningExpression == null).Count() - lastVariables;
                var numberOfFixedVariables = eqsys.Variables.Where(v => v.IsFixed).Count() - lastFixedVariables;
                var numberOfEquations = eqsys.NumberOfEquations - numberOfFixedVariables - lastEquations;


                WriteLine(String.Format("{0,-15} {1,-15} {2,6} {3,6} {4,6} {5,6}", stream.Name, stream.Class, numberOfVariables, numberOfEquations, numberOfFixedVariables, numberOfVariables - numberOfEquations - numberOfFixedVariables));
                lastEquations = eqsys.NumberOfEquations;
                lastVariables = eqsys.NumberOfVariables;
                lastFixedVariables = eqsys.Variables.Where(v => v.IsFixed).Count();
            }
            foreach (var stream in flowsheet.HeatStreams)
            {
                stream.FillEquationSystem(eqsys);
                var numberOfVariables = eqsys.Variables.Where(v => v.DefiningExpression == null).Count() - lastVariables;
                var numberOfFixedVariables = eqsys.Variables.Where(v => v.IsFixed).Count() - lastFixedVariables;
                var numberOfEquations = eqsys.NumberOfEquations - numberOfFixedVariables - lastEquations;


                WriteLine(String.Format("{0,-15} {1,-15} {2,6} {3,6} {4,6} {5,6}", stream.Name, stream.Class, numberOfVariables, numberOfEquations, numberOfFixedVariables, numberOfVariables - numberOfEquations - numberOfFixedVariables));
                lastEquations = eqsys.NumberOfEquations;
                lastVariables = eqsys.NumberOfVariables;
                lastFixedVariables = eqsys.Variables.Where(v => v.IsFixed).Count();
            }
            foreach (var unit in flowsheet.Units)
            {
                unit.FillEquationSystem(eqsys);

                var numberOfVariables = eqsys.Variables.Where(v => v.DefiningExpression == null).Count() - lastVariables;
                var numberOfFixedVariables = eqsys.Variables.Where(v => v.IsFixed).Count() - lastFixedVariables;
                var numberOfEquations = eqsys.NumberOfEquations - numberOfFixedVariables - lastEquations;
                WriteLine(String.Format("{0,-15} {1,-15} {2,6} {3,6} {4,6} {5,6}", unit.Name, unit.Class, numberOfVariables, numberOfEquations, numberOfFixedVariables, numberOfVariables - numberOfEquations - numberOfFixedVariables));

                lastEquations = eqsys.NumberOfEquations;
                lastVariables = eqsys.NumberOfVariables;
                lastFixedVariables = eqsys.Variables.Where(v => v.IsFixed).Count();
            }



            //foreach(var spec in flowsheet.DesignSpecifications)
            var totalNumberOfVariables = lastVariables + flowsheet.Variables.Where(v => v.DefiningExpression == null && !v.IsFixed).Count();
            var totalNumberOfFixedVars = lastFixedVariables + flowsheet.Variables.Where(v => v.IsFixed).Count();
            var totalNumberOfEquations = lastEquations - totalNumberOfFixedVars + flowsheet.DesignSpecifications.Count();
            WriteLine("");
            WriteLine(String.Format("{0,-15} {1,-15} {2,6} {3,6} {4,6} {5,6}", flowsheet.Name, "Flowsheet", totalNumberOfVariables, totalNumberOfEquations, totalNumberOfFixedVars, totalNumberOfVariables - totalNumberOfEquations - totalNumberOfFixedVars));


        }

        public void Report(OptimizationProblem problem)
        {
            WriteLine("");
            WriteLine("Report for problem " + problem.Name);
            WriteLine("================================================");
            WriteLine("");
            WriteLine("Objective Function");
            WriteLine(String.Format("F = {0,12:G6} [{1}]", problem.ObjectiveFunction.Eval(new Evaluator()), problem.ObjectiveFunction.ToString()));
            WriteLine("");
            WriteLine("Constraints");
            foreach (var constraint in problem.Constraints)
            {
                WriteLine(String.Format("C = {0,12:G6} [{1}]", constraint.Residual(new Evaluator()), constraint.ToString()));
            }
            WriteLine("");
            WriteLine("Decisions");
            foreach (var variable in problem.DecisionVariables)
            {
                WriteLine(String.Format("{0,12:G6} <= {1,10} = {2,12:G6} <= {3,12:G6} [{4}]", variable.LowerBound, variable.FullName, variable.ValueInSI, variable.UpperBound, variable.InternalUnit));
            }
        }


        public void Report(Flowsheet flowsheet)
        {
            Report(flowsheet, -1, false);
        }

        public void Report(Flowsheet flowsheet, bool showPhases)
        {
            Report(flowsheet, -1, showPhases);
        }
        public void Report(Flowsheet flowsheet, int columns = -1, bool showPhases = false)
        {
            WriteLine("");
            WriteLine("Report for flowsheet " + flowsheet.Name);
            WriteLine("================================================");
            WriteLine("");
            WriteLine("Material Streams");

            var groups = flowsheet.MaterialStreams.GroupBy(s => s.System);

            var formatter = System.Globalization.NumberFormatInfo.InvariantInfo;

            var lineFormat = "{0,-25} {1,-10} {2}";
            var lineFormat2 = "{0,25} {1,-10} {2}";
            WriteLine("");
            foreach (var group in groups)
            {
                WriteLine(String.Format(lineFormat, "System", group.Key.Name, ""));
                WriteLine("");
                int batches = 1;
                if (columns != -1)
                    batches = (int)Math.Ceiling(group.Count() / (double)columns);


                for (int i = 0; i < batches; i++)
                {
                    IEnumerable<MaterialStream> currentStreamBatch = group;

                    if (columns != -1)
                        currentStreamBatch = group.Skip(i * columns).Take(columns);


                    WriteLine(String.Format(lineFormat, "Property", "Unit", String.Join(" ", currentStreamBatch.Select(s => String.Format("{0,12}", s.Name)))));
                    WriteLine("");

                    Func<PhysicalDimension, Unit> unitFor = d => group.Key.VariableFactory.Output.UnitDictionary[d];
                    Func<MaterialStream, string, string> valueSelector = (s, v) => String.Format(formatter, "{0,12:0.0000}", s.GetVariable(v).ValueInOutputUnit);


                    WriteLine(String.Format(lineFormat, "Temperature", unitFor(PhysicalDimension.Temperature), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "T")))));

                    WriteLine(String.Format(lineFormat, "Pressure", unitFor(PhysicalDimension.Pressure), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "p")))));
                    WriteLine(String.Format(lineFormat, "Vapor Fraction", unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "VF")))));
                    WriteLine(String.Format(lineFormat, "Specific Enthalpy", unitFor(PhysicalDimension.SpecificMolarEnthalpy), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "h")))));
                    WriteLine(String.Format(lineFormat, "Density", unitFor(PhysicalDimension.MassDensity), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "rho")))));
                    WriteLine(String.Format(lineFormat, "Volume Flow", unitFor(PhysicalDimension.VolumeFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "V")))));

                    WriteLine("");
                    WriteLine(String.Format(lineFormat, "Total Molar Flow", unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "n")))));

                    var compExists = new Dictionary<MolecularComponent, bool>();

                    foreach (var c in group.Key.Components)
                    {
                        var test = currentStreamBatch.Select(s => s.GetVariable("n[" + c.ID + "]").ValueInOutputUnit).Sum();
                        if (test > 1e-8)
                            compExists.Add(c, true);
                        else
                            compExists.Add(c, false);

                        if (compExists[c])
                            WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "n[" + c.ID + "]")))));
                    }

                    WriteLine(String.Format(lineFormat, "Molar Composition", "", ""));
                    foreach (var c in group.Key.Components)
                    {
                        if (compExists[c])
                            WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MolarFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "x[" + c.ID + "]")))));
                    }

                    WriteLine("");
                    WriteLine(String.Format(lineFormat, "Total Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "m")))));
                    foreach (var c in group.Key.Components)
                    {
                        if (compExists[c])
                            WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "m[" + c.ID + "]")))));
                    }

                    WriteLine(String.Format(lineFormat, "Mass Composition", "", ""));
                    foreach (var c in group.Key.Components)
                    {
                        if (compExists[c])
                            WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "w[" + c.ID + "]")))));
                    }
                    WriteLine("");
                    if (showPhases)
                    {
                        WriteLine(String.Format(lineFormat, "Liquid Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mL")))));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mL[" + c.ID + "]")))));
                        }
                        WriteLine(String.Format(lineFormat, "Liquid Mass Composition", "", ""));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "wL[" + c.ID + "]")))));
                        }
                        WriteLine("");
                        WriteLine(String.Format(lineFormat, "Vapor Mass Flow", unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mV")))));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFlow), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "mV[" + c.ID + "]")))));
                        }
                        WriteLine(String.Format(lineFormat, "Vapor Mass Composition", "", ""));
                        foreach (var c in group.Key.Components)
                        {
                            if (compExists[c])
                                WriteLine(String.Format(lineFormat2, c.ID, unitFor(PhysicalDimension.MassFraction), String.Join(" ", currentStreamBatch.Select(s => valueSelector(s, "wV[" + c.ID + "]")))));
                        }
                        WriteLine("");

                    }
                    WriteLine("");

                }



                WriteLine("");
                WriteLine("Design Specifications");
                WriteLine("");

                WriteLine(String.Format("{0,-15} {1,-30} {2,-15} {3,-20} {4,15} {5}", "Name", "Model", "Class", "Group", "Residual", "Equation"));

                foreach (var eq in flowsheet.DesignSpecifications)
                {
                    WriteLine(String.Format("{0,-15} {1,-30} {2,-15} {3,-20} {4,15} {5}", eq.Name, eq.ModelName, eq.ModelClass, eq.Group, eq.Residual(new OpenFMSL.Core.Expressions.Evaluator()).ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), eq.ToString()));
                }
                WriteLine("");

            }
        }
        public void Report(Variable variable)
        {
            var formatter = System.Globalization.NumberFormatInfo.InvariantInfo;
            WriteLine(String.Format("{0, -25} = {1} {2,-12} [{3}]", variable.ModelName + "." + variable.FullName, String.Format(formatter, "{0,12:0.0000}", variable.ValueInOutputUnit), variable.OutputUnit, variable.Description));
        }

        public void Report(ProcessUnit unit)
        {
            WriteLine("");
            WriteLine("Report for unit " + unit.Name + "[" + unit.Class + "]");
            WriteLine("================================================");
            WriteLine("Material Ports");
            WriteLine("");

            WriteLine(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", "Name", "Direction", "Multi", "Num", "Streams"));

            foreach (var port in unit.MaterialPorts)
            {
                WriteLine(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", port.Name, port.Direction, port.Multiplicity, port.NumberOfStreams, String.Join(", ", port.Streams.Select(s => s.Name))));
            }

            if (unit.HeatPorts.Count > 0)
            {
                WriteLine("");
                WriteLine("Heat Ports");
                WriteLine("");
                WriteLine(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", "Name", "Direction", "Multi", "Num", "Streams"));
                foreach (var port in unit.HeatPorts)
                {
                    WriteLine(String.Format("{0,-15} {1,-10} {2,-5} {3,-5} {4,-25}", port.Name, port.Direction, port.Multiplicity, port.NumberOfStreams, String.Join(", ", port.Streams.Select(s => s.Name))));
                }
            }

            WriteLine("");
            WriteLine("Variables");
            WriteLine("");
            WriteLine(String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,15} {5,-15} {6,-15} {7,-15} {8,-25} {9,-15}", "Name", "Model", "Class", "Group", "Value", "Unit", "Min", "Max", "Dimension", "Description"));
            foreach (var vari in unit.Variables)
            {
                WriteLine(String.Format("{0,-15} {1,-15} {2,-15} {3,-15} {4,15} {5,-15} {6,-15} {7,-15} {8,-25} {9,-15}",
                    vari.FullName,
                    vari.ModelName,
                    vari.ModelClass,
                    vari.Group,
                    vari.ValueInOutputUnit.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    vari.OutputUnit.Symbol,
                    Unit.Convert(vari.InternalUnit, vari.OutputUnit, vari.LowerBound).ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    Unit.Convert(vari.InternalUnit, vari.OutputUnit, vari.UpperBound).ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo),
                    vari.Dimension,
                    vari.Description));
            }
            WriteLine("");
        }

        public void Report(EquationSystem system)
        {
            Report(system, false);
        }
        public void Report(EquationSystem system, bool showDefined)
        {
            WriteLine("Report for system " + system.Name);
            WriteLine("");
            WriteLine("Number of Variables: " + system.NumberOfVariables);
            WriteLine("Number of Defined Variables: " + system.DefinedVariables.Count);
            WriteLine("Number of Equation: " + system.NumberOfEquations);

            WriteLine("");
            WriteLine("Free Variables");
            WriteLine("");

            WriteLine(String.Format("{0,-20} {1,-30} {2,-15} {3,-15} {4,15} {5,-15} {6,-15} {7,-15} {8,-25} {9,-15}", "Name", "Model", "Class", "Group", "Value", "Unit", "Min", "Max", "Dimension", "Description"));
            foreach (var vari in system.Variables)
            {
                WriteLine(String.Format("{0,-20} {1,-30} {2,-15} {3,-15} {4,15} {5,-15} {6,-15} {7,-15} {8,-25} {9,-15}", vari.FullName, vari.ModelName, vari.ModelClass, vari.Group, vari.ValueInSI.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), vari.InternalUnit.Symbol, vari.LowerBound.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), vari.UpperBound.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), vari.Dimension, vari.Description));
            }
            if (showDefined)
            {
                WriteLine("");
                WriteLine("Defined Variables");
                WriteLine("");

                WriteLine(String.Format("{0,-20} {1,-30} {2,-15} {3,-15} {4,15} {5,-15} {6,-15}", "Name", "Model", "Class", "Group", "Value", "Unit", "Expression"));
                foreach (var vari in system.DefinedVariables)
                {
                    WriteLine(String.Format("{0,-20} {1,-30} {2,-15} {3,-15} {4,15} {5,-15} {6,-15}", vari.FullName, vari.ModelName, vari.ModelClass, vari.Group, vari.ValueInOutputUnit.ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), vari.OutputUnit.Symbol, vari.DefiningExpression));
                }
            }
            WriteLine("");
            WriteLine("Equations");
            WriteLine("");

            WriteLine(String.Format("{0,-20} {1,-30} {2,-15} {3,-20} {4,15} {5}", "Name", "Model", "Class", "Group", "Residual", "Equation"));

            foreach (var eq in system.Equations.OrderByDescending(eq => Math.Abs(eq.Residual(new OpenFMSL.Core.Expressions.Evaluator()))))
            {
                //, String.Join(", ", eq.Incidence(new OpenFMSL.Core.Expressions.Evaluator()).Select(v => v.FullName))
                WriteLine(String.Format("{0,-20} {1,-30} {2,-15} {3,-20} {4,15} {5}", eq.Name, eq.ModelName, eq.ModelClass, eq.Group, eq.Residual(new OpenFMSL.Core.Expressions.Evaluator()).ToString("G6", System.Globalization.NumberFormatInfo.InvariantInfo), eq.ToString()));

            }
        }

        public ThermodynamicSystem LoadThermodynamicSystem(ThermodynamicSystemEntity entity)
        {
            return _importer.ImportNeutralFile(entity.SourceCode);
        }


        public void RunEntity(Script script)
        {
            try
            {
                Run(script.SourceCode);
            }
            catch (Exception ex)
            {
                Write(String.Format("!!! Python script {0} could not be executed. The error was {1}\n", script.Name, ex.ToString()));

            }
        }
        public void RunFile(string filename)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(filename);
                if (String.IsNullOrEmpty(dir))
                    dir = Environment.CurrentDirectory;
                var ext = System.IO.Path.GetExtension(filename);
                if (String.IsNullOrEmpty(ext))
                    filename += ".py";

                var path = System.IO.Path.Combine(dir, filename);
                var source = System.IO.File.ReadAllText(path);
                Run(source);
            }
            catch (Exception ex)
            {
                Write(String.Format("!!! Python file {0} could not be executed. The error was {1}\n", filename, ex.ToString()));

            }
        }

        public void SendLogMessage(string text)
        {
            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Information, MessageText = text });
        }
        public void SendWarningMessage(string text)
        {
            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Warning, MessageText = text });
        }
        public void SendErrorMessage(string text)
        {
            _aggregator.PublishOnUIThread(new LogMessage { TimeStamp = DateTime.Now, Sender = this, Channel = LogChannels.Error, MessageText = text });
        }
        public void SendStatusTextChangeMessage(string text)
        {
            _aggregator.PublishOnUIThread(new ChangeStatusBarTextMessage { TimeStamp = DateTime.Now, Sender = this, StatusBarText = text });
        }

        public void Run(string sourceCode)
        {
            if (sourceCode == null)
                return;

            try
            {
                ExecutePythonStatement(sourceCode);
            }
            catch (Microsoft.Scripting.SyntaxErrorException ex)
            {
                Write(String.Format("*** Syntax error encountered in '{3}'. The error was reported as '{0}' in line {1}, column {2}\n", ex.Message, ex.Line, ex.Column, ex.GetCodeLine()));
            }
            catch (Exception ex)
            {
                Write(String.Format("*** Python command could not be executed. The error was {1} {2}\n", sourceCode, ex.ToString(), ex.GetType()));
            }
        }

        void ExecutePythonStatement(string statement)
        {
            var source = _pyEngine.CreateScriptSourceFromString(statement);
            var compiled = source.Compile();
            var result = compiled.Execute(_pyScope);
        }


    }
}
