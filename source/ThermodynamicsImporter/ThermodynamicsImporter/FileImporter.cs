
using Caliburn.Micro;
using OpenFMSL.Contracts.Infrastructure.Messaging;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermodynamicsImporter
{
    public class FileImporter
    {
        IList<MolecularComponent> _components = new List<MolecularComponent>();
        IList<ThermodynamicSystem> _systems = new List<ThermodynamicSystem>();
        ThermodynamicSystem _currentSystem;

        public IList<MolecularComponent> Components
        {
            get { return _components; }
            set { _components = value; }
        }

        public IList<ThermodynamicSystem> Systems
        {
            get { return _systems; }
            set { _systems = value; }
        }


        private readonly IEventAggregator _aggregator;

        public FileImporter(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            // _aggregator.Raise<LogMessage>(new LogMessage { Channel = LogChannels.Information, MessageText = "IKC importer successfully created.", Sender = this, TimeStamp = DateTime.Now });

        }

        public ThermodynamicSystem ImportSystem(string source)
        {
            return ImportSystem(source, 0);
        }

        void Reset()
        {
            Systems = new List<ThermodynamicSystem>();
            Components = new List<MolecularComponent>();
        }

        bool IsLineFunctionDescription(string keyword)
        {
            switch (keyword)
            {
                case "CL":
                case "CPID":
                case "VP":
                case "HVAP":
                case "VISL":
                case "VISV":
                case "KLIQ":
                case "KVAP":
                case "ST":
                case "DENL":               
                    return true;
                default:
                    return false;
            }

        }

        void ParseSystem(string[] line)
        {
            _currentSystem = new ThermodynamicSystem();
            _currentSystem.Name = line[1];
            Systems.Add(_currentSystem);
        }
        MolecularComponent DefaultComponent(string name, string id)
        {
            var newComp = new MolecularComponent();
            newComp.Name = name;
            newComp.ID = id;
            newComp.CasNumber = "123-456-7";
            newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("MolarWeight", 18.17, SI.kg / SI.kmol));
            newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("CriticalTemperature", 600, SI.K));
            newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("CriticalPressure", 221e5, SI.Pa));
            newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("CriticalDensity", 0.1, SI.kmol/SI.cum));
            newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("HeatOfFormation", 0, SI.J/SI.kmol));

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.VaporPressure,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 1) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("VP")
            });

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.IdealGasHeatCapacity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("CPID")
            });

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.LiquidHeatCapacity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("CL")
            });
            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.LiquidDensity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("DENL")
            });
            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.HeatOfVaporization,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("HVAP")
            });

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.SurfaceTension,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("ST")
            });
            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.LiquidHeatConductivity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("KLIQ")
            });

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.VaporHeatConductivity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("KVAP")
            });

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.LiquidViscosity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("VISL")
            });

            newComp.Functions.Add(new PropertyFunction()
            {
                Type = FunctionType.Polynomial,
                Property = EvaluatedProperties.VaporViscosity,
                Coefficients = new List<OpenFMSL.Core.Expressions.Variable> { new OpenFMSL.Core.Expressions.Variable("C1", 0) },
                MinimumX = new OpenFMSL.Core.Expressions.Variable("Tmin", 1),
                MaximumX = new OpenFMSL.Core.Expressions.Variable("Tmax", 1000),
                XUnit = SI.K,
                YUnit = GetSIUnitForTProperty("VISV")
            });

            return newComp;

        }

        MolecularComponent GetOrCreateComponent(int id)
        {
            if (_currentSystem == null)
                throw new InvalidOperationException("_currentSystem must not be null");

            if (_currentSystem.Components.Count < id - 1)
                return _currentSystem.Components[id - 1];
            else
            {
                for (int i = 1; i <= id; i++)
                {
                    if (_currentSystem.Components.Count < i)
                    {
                        var newComp = DefaultComponent("Default", "Dummy");
                        _currentSystem.EnthalpyMethod.PureComponentEnthalpies.Add(PureEnthalpyFunction.Create(_currentSystem, newComp));
                        _currentSystem.EnthalpyMethod.PureComponentEnthalpies[id - 1].ReferenceState = PhaseState.Liquid;
                        _currentSystem.Components.Add(newComp);
                    }
                }
                return _currentSystem.Components[id - 1];
            }


        }
        void ParseShortname(string[] line)
        {
            int compId = ParseInteger(line[1]);
            var component = GetOrCreateComponent(compId);
            component.ID = line[2];
        }
        void ParseName(string[] line)
        {
            int compId = ParseInteger(line[1]);
            var component = GetOrCreateComponent(compId);
            component.Name = line[2];
        }
        void ParseCASNo(string[] line)
        {
            int compId = ParseInteger(line[1]);
            var component = GetOrCreateComponent(compId);
            component.CasNumber = line[2];
        }


        void ParseConstant(string[] line, Func<MolecularComponent, Variable> selector, Func<double, double> transform = null)
        {
            int compId = Int32.Parse(line[1]);
            var component = GetOrCreateComponent(compId);

            if (transform == null)
                transform = (x) => x;


            selector(component).ValueInSI = transform(ParseDouble(line[3]));

        }

        Unit GetSIUnitForTProperty(string prop)
        {
            switch (prop)
            {
                case "CL":
                case "CPID":
                    return SI.J / SI.kmol / SI.K;
                case "HVAP":
                    return SI.J / SI.kmol;
                case "DENL":
                    return SI.kmol / SI.cum;
                case "VP":
                    return SI.Pa;
                case "ST":
                    return SI.N / SI.m;
                case "KLIQ":
                case "KVAP":
                    return SI.W / SI.m / SI.K;
                case "VISL":
                case "VISV":
                    return SI.Pa *  SI.s;
                default:
                    throw new InvalidOperationException("Property type " + prop + " is not suppported.");

            }
        }
        EvaluatedProperties GetPropertyTypeFromString(string prop)
        {
            switch (prop)
            {
                case "CL":
                    return EvaluatedProperties.LiquidHeatCapacity;
                case "CPID":
                    return EvaluatedProperties.IdealGasHeatCapacity;
                case "HVAP":
                    return EvaluatedProperties.HeatOfVaporization;
                case "DENL":
                    return EvaluatedProperties.LiquidDensity;
                case "VP":
                    return EvaluatedProperties.VaporPressure;
                case "ST":
                    return EvaluatedProperties.SurfaceTension;
                case "KLIQ":
                    return EvaluatedProperties.LiquidHeatConductivity;
                case "KVAP":
                    return EvaluatedProperties.VaporHeatConductivity;
                case "VISL":
                    return EvaluatedProperties.LiquidViscosity;
                case "VISV":
                    return EvaluatedProperties.VaporViscosity;

                default:
                    throw new InvalidOperationException("Property type " + prop + " is not suppported.");

            }
        }
        void ParsePureFunction(string[] line, Func<MolecularComponent, PropertyFunction> selector)
        {
            int i = ParseInteger(line[1]);
            var component = GetOrCreateComponent(i);
            var function = selector(component);

            switch (line[3])
            {
                case "POLY":
                    function.Type = FunctionType.Polynomial;
                    break;
                case "WATS":
                    function.Type = FunctionType.Watson;
                    break;
                case "ANT1":
                    function.Type = FunctionType.ExtendedAntoine;
                    break;
                case "ANTO":
                    function.Type = FunctionType.Antoine;
                    break;
                case "ALYL":
                    function.Type = FunctionType.AlyLee;
                    break;
                case "DIP4":
                    function.Type = FunctionType.Dippr106;
                    break;
                case "DIP5":
                    function.Type = FunctionType.Dippr102;
                    break;
                case "WAGN":
                    function.Type = FunctionType.Wagner;
                    break;
                case "RACK":
                    function.Type = FunctionType.Rackett;
                    break;
                case "KIR1":
                    function.Type = FunctionType.Kirchhoff;
                    break;
                case "SUTH":
                    function.Type = FunctionType.Sutherland;
                    break;
                default:
                    throw new InvalidOperationException("Function type " + line[3] + " is not suppported.");
            }

            function.Property = GetPropertyTypeFromString(line[0]);
            var paramCount = ParseInteger(line[4]);
            function.YUnit = GetSIUnitForTProperty(line[0]);

            function.MinimumX.ValueInSI = ParseDouble(line[5]);
            function.MaximumX.ValueInSI = ParseDouble(line[6]);

            function.Coefficients.Clear();
            for (int c = 0; c < paramCount; c++)
            {
                var paramValue = ParseDouble(line[8 + c]);
                //paramValue.ToString()"C" + (c + 1)
                function.Coefficients.Add(new Variable(paramValue.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), paramValue) { IsFixed = true, IsConstant = true });
            }

        }
        void ParseLVEQ(string[] line)
        {
            switch (line[2])
            {
                case "NRTL":
                    _currentSystem.EquilibriumMethod.Activity = ActivityMethod.NRTL;
                    break;
                case "UNIQ":
                    _currentSystem.EquilibriumMethod.Activity = ActivityMethod.UNIQUAC;
                    break;
                case "WILS":
                    _currentSystem.EquilibriumMethod.Activity = ActivityMethod.Wilson;
                    break;
                case "NONE":
                case "IDEA":
                    _currentSystem.EquilibriumMethod.Activity = ActivityMethod.Ideal;
                    break;
                default:
                    _aggregator.PublishOnUIThread(new LogMessage { MessageText = "Activity coefficient method " + line[2] + " is not supported. Using IDEAL method" });
                    _currentSystem.EquilibriumMethod.Activity = ActivityMethod.Ideal;
                    break;
            }

            switch (line[3])
            {
                case "PR":
                    _currentSystem.EquilibriumMethod.Fugacity = FugacityMethod.PengRobinson;
                    break;
                case "SRK":
                    _currentSystem.EquilibriumMethod.Fugacity = FugacityMethod.SoaveRedlichKwong;
                    break;
                case "NONE":
                    _currentSystem.EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    break;
                default:
                    _aggregator.PublishOnUIThread(new LogMessage { MessageText = "Fugacity coefficient method " + line[2] + " is not supported. Using IDEAL method" });
                    _currentSystem.EquilibriumMethod.Fugacity = FugacityMethod.Ideal;
                    break;
            }

        }

        void ParseNRTL(string[] line)
        {

            int i = ParseInteger(line[2]);
            int j = ParseInteger(line[3]);
            double aij = ParseDouble(line[4]);
            double aji = ParseDouble(line[5]);
            //_currentSystem.MixtureProperties.NRTL
            var comp1 = _currentSystem.Components[i - 1];
            var comp2 = _currentSystem.Components[j - 1];


            if (_currentSystem.BinaryParameters.Count(ps => ps.Name == "NRTL") == 0)
                _currentSystem.BinaryParameters.Add(new NRTL(_currentSystem));

            var currentNRTL = _currentSystem.BinaryParameters.FirstOrDefault(ps => ps.Name == "NRTL");
            switch (line[1])
            {
                case "ALPH":
                case "C":
                    currentNRTL.SetParam("C", comp1, comp2, aij);
                    currentNRTL.SetParam("C", comp2, comp1, aji);
                    break;
                case "A":
                    currentNRTL.SetParam("A", comp1, comp2, aij);
                    currentNRTL.SetParam("A", comp2, comp1, aji);
                    break;
                case "B":
                    currentNRTL.SetParam("B", comp1, comp2, aij);
                    currentNRTL.SetParam("B", comp2, comp1, aji);
                    break;
                case "D":
                case "BETA":
                    currentNRTL.SetParam("D", comp1, comp2, aij);
                    currentNRTL.SetParam("D", comp2, comp1, aji);
                    break;
                case "E":
                    currentNRTL.SetParam("E", comp1, comp2, aij);
                    currentNRTL.SetParam("E", comp2, comp1, aji);
                    break;
                case "F":
                    currentNRTL.SetParam("F", comp1, comp2, aij);
                    currentNRTL.SetParam("F", comp2, comp1, aji);
                    break;
            }

        }
        void ParseHenry(string[] line)
        {

            int i = ParseInteger(line[2]);
            int j = ParseInteger(line[3]);
            double aij = ParseDouble(line[4]);
            double aji = ParseDouble(line[5]);
            //_currentSystem.MixtureProperties.NRTL
            var comp1 = _currentSystem.Components[i - 1];
            var comp2 = _currentSystem.Components[j - 1];


            if (_currentSystem.BinaryParameters.Count(ps => ps.Name == "HENRY") == 0)
                _currentSystem.BinaryParameters.Add(new HENRY(_currentSystem));

            var currentNRTL = _currentSystem.BinaryParameters.FirstOrDefault(ps => ps.Name == "HENRY");
            switch (line[1])
            {    
                case "A":
                    currentNRTL.SetParam("A", comp1, comp2, aij);
                    currentNRTL.SetParam("A", comp2, comp1, aji);
                    break;
                case "B":
                    currentNRTL.SetParam("B", comp1, comp2, aij);
                    currentNRTL.SetParam("B", comp2, comp1, aji);
                    break;
                case "C":
                    currentNRTL.SetParam("C", comp1, comp2, aij);
                    currentNRTL.SetParam("C", comp2, comp1, aji);
                    break;
                case "D":               
                    currentNRTL.SetParam("D", comp1, comp2, aij);
                    currentNRTL.SetParam("D", comp2, comp1, aji);
                    break;
                default:
                    _aggregator.PublishOnUIThread(new LogMessage { MessageText = "Parameter " + line[1] + " is not allowed for Henry." });
                    break;                
            }

        }
        void ParseWilson(string[] line)
        {

            int i = ParseInteger(line[2]);
            int j = ParseInteger(line[3]);
            double aij = ParseDouble(line[4]);
            double aji = ParseDouble(line[5]);
          
            var comp1 = _currentSystem.Components[i - 1];
            var comp2 = _currentSystem.Components[j - 1];


            if (_currentSystem.BinaryParameters.Count(ps => ps.Name == "WILSON") == 0)
                _currentSystem.BinaryParameters.Add(new WILSON(_currentSystem));

            var currentParameterSet = _currentSystem.BinaryParameters.FirstOrDefault(ps => ps.Name == "WILSON");
            switch (line[1])
            {
                case "A":
                    currentParameterSet.SetParam("A", comp1, comp2, aij);
                    currentParameterSet.SetParam("A", comp2, comp1, aji);
                    break;
                case "B":
                    currentParameterSet.SetParam("B", comp1, comp2, aij);
                    currentParameterSet.SetParam("B", comp2, comp1, aji);
                    break;
                case "C":
                    currentParameterSet.SetParam("C", comp1, comp2, aij);
                    currentParameterSet.SetParam("C", comp2, comp1, aji);
                    break;
                case "D":
                    currentParameterSet.SetParam("D", comp1, comp2, aij);
                    currentParameterSet.SetParam("D", comp2, comp1, aji);
                    break;
                default:
                    _aggregator.PublishOnUIThread(new LogMessage { MessageText = "Parameter " + line[1] + " is not allowed for Wilson." });
                    break;
            }
        }
        int ParseInteger(string token)
        {
            int value = -1;
            if (Int32.TryParse(token, out value))
                return value;
            else
                throw new ArgumentException("Can not parse " + token + " to integer.");
        }

        double ParseDouble(string token)
        {
            if (token.Contains("D"))
                token = token.Replace("D", "E");
            if (token.Contains("d"))
                token = token.Replace("d", "E");

            double value = -1;
            if (Double.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
                return value;
            else
                throw new ArgumentException("Can not parse " + token + " to double.");
        }

        bool EvaluateLine(string[] line)
        {
            try
            {
                switch (line[0])
                {
                    case "SYST":
                        ParseSystem(line);
                        return true;
                    case "SHOR":
                        ParseShortname(line);
                        return true;
                    case "NAME":
                        ParseName(line);
                        return true;
                    case "CASN":
                        ParseCASNo(line);
                        return true;
                    case "MOLW":
                        ParseConstant(line, c => c.GetConstant(ConstantProperties.MolarWeight));
                        return true;
                    case "TC":
                        ParseConstant(line, c => c.GetConstant(ConstantProperties.CriticalTemperature));
                        return true;
                    case "PC":
                        ParseConstant(line, c => c.GetConstant(ConstantProperties.CriticalPressure));
                        return true;
                    case "RHOC":
                        ParseConstant(line, c => c.GetConstant(ConstantProperties.CriticalDensity));
                        return true;
                    case "LVEQ":
                        ParseLVEQ(line);
                        return true;
                    case "NRTL":
                        ParseNRTL(line);
                        return true;
                    case "HNRY":
                        ParseHenry(line);
                        return true;
                    case "WILS":
                        ParseWilson(line);
                        return true;
                    case "VP":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.VaporPressure));
                        return true;
                    case "CL":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.LiquidHeatCapacity));
                        return true;
                    case "CPID":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.IdealGasHeatCapacity));
                        return true;
                    case "HVAP":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.HeatOfVaporization));
                        return true;
                    case "KLIQ":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.LiquidHeatConductivity));
                        return true;
                    case "KVAP":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.VaporHeatConductivity));
                        return true;
                    case "ST":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.SurfaceTension));
                        return true;
                    case "DENL":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.LiquidDensity));
                        return true;
                    case "VISL":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.LiquidViscosity));
                        return true;
                    case "VISV":
                        ParsePureFunction(line, c => c.GetFunction(EvaluatedProperties.VaporViscosity));
                        return true;
                    case "HREF":
                        {
                            int compIdx = ParseInteger(line[1]);
                            _currentSystem.EnthalpyMethod.PureComponentEnthalpies[compIdx - 1].Href.ValueInSI = ParseDouble(line[2]);
                        }
                        return true;

                    case "TREF":
                        {
                            int compIdx = ParseInteger(line[1]);
                            _currentSystem.EnthalpyMethod.PureComponentEnthalpies[compIdx - 1].Tref.ValueInSI = ParseDouble(line[2]);
                        }
                        return true;
                    case "T_PH":
                        {
                            int compIdx = ParseInteger(line[1]);
                            _currentSystem.EnthalpyMethod.PureComponentEnthalpies[compIdx - 1].TPhaseChange.ValueInSI = ParseDouble(line[2]);
                            _currentSystem.EnthalpyMethod.PureComponentEnthalpies[compIdx - 1].PhaseChangeAtSystemTemperature = false;
                        }
                        return true;
                    case "PHAS":
                        {
                            int compIdx = ParseInteger(line[1]);
                            _currentSystem.EnthalpyMethod.PureComponentEnthalpies[compIdx - 1].ReferenceState = line[2] == "LIQU" ? PhaseState.Liquid : PhaseState.Vapour;
                        }
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishOnUIThread(new LogMessage { Channel = LogChannels.Error, MessageText = "Error occured while parsing line <" + String.Join(" ", line) + "> = " + e.Message });
                return false;
            }

        }
        public ThermodynamicSystem ImportSystem(string source, int index)
        {
            Reset();

            var parser = new FileParser(source);

            string[] currentLine = null;

            do
            {
                currentLine = parser.ParseNextLine();
                if (currentLine != null)
                {
                    if (IsLineFunctionDescription(currentLine[0]))
                        currentLine = parser.MergeNextLine(currentLine);

                    if (parser.IsNextLineContinuation())
                        currentLine = parser.MergeNextLine(currentLine);

                    if (!EvaluateLine(currentLine))
                    {
                        _aggregator.PublishOnUIThread(new ChangeStatusBarTextMessage { StatusBarText = "SKIP " + String.Join(" | ", currentLine) });
                    }


                }
                else
                {
                    _aggregator.PublishOnUIThread(new LogMessage { MessageText = "File parsed completely" });
                }
            }
            while (currentLine != null);

            if (index < Systems.Count)
                return Systems[index];
            else
                return null;
        }
    }
}
