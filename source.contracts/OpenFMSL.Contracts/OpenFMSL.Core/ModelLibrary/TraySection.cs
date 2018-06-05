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

namespace OpenFMSL.Core.ModelLibrary
{
    public enum TrayEfficiencyType { None, Murphree, ExtendedMurphree, BausaMurphree };

    public class TrayConnectivity
    {
        MaterialStream _stream;
        int _stage;
        Variable _factor = new Variable("F", 0, 0, 1, SI.nil, "Split factor for sidestream") { Superscript = "Side", IsFixed = true };
        PhaseState _phase = PhaseState.Liquid;

        public MaterialStream Stream
        {
            get
            {
                return _stream;
            }

            set
            {
                _stream = value;
            }
        }

        public int Stage
        {
            get
            {
                return _stage;
            }

            set
            {
                _stage = value;
            }
        }

        public Variable Factor
        {
            get
            {
                return _factor;
            }

            set
            {
                _factor = value;
            }
        }

        public PhaseState Phase
        {
            get
            {
                return _phase;
            }

            set
            {
                _phase = value;
            }
        }


    }

    public class EquilibriumTray
    {
        public Variable T;
        public Variable TV;
        public Variable eps;
        public Variable p;
        public Variable Q;
        public Variable L;
        public Variable V;
        public Variable W;
        public Variable U;
        public Variable RV;
        public Variable RL;
        public Variable F;
        public Variable HL;
        public Variable HV;
        public Variable HF;
        public Variable[] x;
        public Variable[] y;
        public Variable[] yeq;
        public Variable[] z;

        public Variable[] K;
        //Variable[] EPS;
        public Variable DP;
        int _number = -1;
        ThermodynamicSystem _system;

        public int Number
        {
            get
            {
                return _number;
            }

            set
            {
                _number = value;
            }
        }

        public EquilibriumTray(int number, ThermodynamicSystem system)
        {
            _system = system;
            Number = number;
            var numString = number.ToString();
            T = system.VariableFactory.CreateVariable("T", numString, "Stage Temperature", PhysicalDimension.Temperature);
            TV = system.VariableFactory.CreateVariable("TV", numString, "Stage Vapor Temperature", PhysicalDimension.Temperature);
            DP = system.VariableFactory.CreateVariable("DP", numString, "Pressure Drop", PhysicalDimension.Pressure);
            DP.LowerBound = 0;
            DP.ValueInSI = 0;

            p = system.VariableFactory.CreateVariable("P", numString, "Stage Pressure", PhysicalDimension.Pressure);
            Q = system.VariableFactory.CreateVariable("Q", numString, "Heat Duty", PhysicalDimension.HeatFlow);

            L = system.VariableFactory.CreateVariable("L", numString, "Liquid molar flow", PhysicalDimension.MolarFlow);
            V = system.VariableFactory.CreateVariable("V", numString, "Vapor molar flow", PhysicalDimension.MolarFlow);
            U = system.VariableFactory.CreateVariable("U", numString, "Liquid molar flow", PhysicalDimension.MolarFlow);
            W = system.VariableFactory.CreateVariable("W", numString, "Vapor molar flow", PhysicalDimension.MolarFlow);
            F = system.VariableFactory.CreateVariable("F", numString, "Feed molar flow", PhysicalDimension.MolarFlow);

            RL = system.VariableFactory.CreateVariable("RL", numString, "Liquid sidestream fraction", PhysicalDimension.Dimensionless);
            RV = system.VariableFactory.CreateVariable("RV", numString, "Vapor sidestream fraction", PhysicalDimension.Dimensionless);
            RV.LowerBound = 0;
            RL.LowerBound = 0;

            HF = system.VariableFactory.CreateVariable("HF", numString, "Feed specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
            HL = system.VariableFactory.CreateVariable("HL", numString, "Liquid specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
            HV = system.VariableFactory.CreateVariable("HV", numString, "Vapor specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);

            eps = system.VariableFactory.CreateVariable("eps", numString, "Tray efficiency", PhysicalDimension.Dimensionless);
            eps.ValueInSI = 1;
            eps.LowerBound = 0;
            eps.UpperBound = 1;

            K = new Variable[system.Components.Count];
            x = new Variable[system.Components.Count];
            y = new Variable[system.Components.Count];
            yeq = new Variable[system.Components.Count];
            z = new Variable[system.Components.Count];
            for (int i = 0; i < system.Components.Count; i++)
            {
                K[i] = system.VariableFactory.CreateVariable("K", numString + ", " + system.Components[i].ID, "Equilibrium partition coefficient", PhysicalDimension.Dimensionless);
                K[i].ValueInSI = 1.2;
                x[i] = system.VariableFactory.CreateVariable("x", numString + ", " + system.Components[i].ID, "Liquid molar fraction", PhysicalDimension.MolarFraction);
                y[i] = system.VariableFactory.CreateVariable("y", numString + ", " + system.Components[i].ID, "Vapor molar fraction", PhysicalDimension.MolarFraction);
                yeq[i] = system.VariableFactory.CreateVariable("yeq", numString + ", " + system.Components[i].ID, "Equilibrium Vapor molar fraction", PhysicalDimension.MolarFraction);
                z[i] = system.VariableFactory.CreateVariable("z", numString + ", " + system.Components[i].ID, "Feed molar fraction", PhysicalDimension.MolarFraction);

            }
        }
    }

    public class TraySection : ProcessUnit
    {
        int _numberOfTrays = -1;
        TrayEfficiencyType _efficiencyType = TrayEfficiencyType.None;

        List<EquilibriumTray> _trays = new List<EquilibriumTray>();
        List<TrayConnectivity> _feeds = new List<TrayConnectivity>();
        List<TrayConnectivity> _sidestream = new List<TrayConnectivity>();
        List<MolecularComponent> _ignoredComponents = new List<MolecularComponent>();

        public int NumberOfTrays
        {
            get
            {
                return _numberOfTrays;
            }

            set
            {
                _numberOfTrays = value;
            }
        }

        public TrayEfficiencyType EfficiencyType
        {
            get
            {
                return _efficiencyType;
            }

            set
            {
                _efficiencyType = value;
            }
        }

        public List<MolecularComponent> IgnoredComponents
        {
            get
            {
                return _ignoredComponents;
            }

            set
            {
                _ignoredComponents = value;
            }
        }

        public TraySection(string name, ThermodynamicSystem system, int numberOfTrays) : base(name, system)
        {
            Class = "TraySection";
            NumberOfTrays = numberOfTrays;
            Icon.IconType = IconTypes.ColumnSection;

            MaterialPorts.Add(new Port<MaterialStream>("Feeds", PortDirection.In, -1));
            MaterialPorts.Add(new Port<MaterialStream>("VIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("LIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("VOut", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("LOut", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("Sidestreams", PortDirection.Out, -1));

            for (int i = 0; i < NumberOfTrays; i++)
            {
                var tray = new EquilibriumTray(i + 1, system);
                _trays.Add(tray);
            }

            AddVariables(_trays.Select(t => t.T).ToArray());
            AddVariables(_trays.Select(t => t.TV).ToArray());

            AddVariables(_trays.Select(t => t.p).ToArray());
            AddVariables(_trays.Select(t => t.DP).ToArray());
            AddVariables(_trays.Select(t => t.Q).ToArray());
            AddVariables(_trays.Select(t => t.F).ToArray());
            AddVariables(_trays.Select(t => t.L).ToArray());
            AddVariables(_trays.Select(t => t.V).ToArray());
            AddVariables(_trays.Select(t => t.U).ToArray());
            AddVariables(_trays.Select(t => t.W).ToArray());
            AddVariables(_trays.Select(t => t.RL).ToArray());
            AddVariables(_trays.Select(t => t.RV).ToArray());
            AddVariables(_trays.Select(t => t.eps).ToArray());
            AddVariables(_trays.Select(t => t.HF).ToArray());
            AddVariables(_trays.Select(t => t.HL).ToArray());
            AddVariables(_trays.Select(t => t.HV).ToArray());

            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].K);
            }
            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].x);
            }
            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].y);
            }
            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].yeq);
            }

            for (int i = 0; i < NumberOfTrays; i++)
            {
                AddVariables(_trays[i].z);
            }


            int NC = System.Components.Count;
            for (var tray = 0; tray < NumberOfTrays; tray++)
            {
                _trays[tray].U.FixValue(0);
                _trays[tray].W.FixValue(0);
                _trays[tray].RL.FixValue(0);
                _trays[tray].RV.FixValue(0);
                _trays[tray].F.FixValue(0);
                _trays[tray].HF.FixValue(0);

                /*     _trays[tray].U.IsConstant = true;
                     _trays[tray].W.IsConstant = true;
                     _trays[tray].RL.IsConstant = true;
                     _trays[tray].RV.IsConstant = true;
                     _trays[tray].F.IsConstant = true;
                      _trays[tray].HF.IsConstant = true;*/

                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[tray].z[comp].FixValue(0);
                    //  _trays[tray].z[comp].IsConstant = true;
                }
            }

        }

        public TraySection ConnectFeed(MaterialStream stream, int stage, PhaseState phase = PhaseState.LiquidVapor)
        {
            _feeds.Add(new TrayConnectivity() { Stage = stage, Stream = stream, Phase = phase });
            Connect("Feeds", stream);
            return this;
        }

        public TraySection ConnectLiquidSidestream(MaterialStream stream, int stage, double factor)
        {
            var con = new TrayConnectivity() { Stage = stage, Stream = stream, Phase = PhaseState.Liquid };
            con.Factor.ValueInSI = factor;
            _sidestream.Add(con);
            Connect("Sidestreams", stream);
            return this;
        }

        public TraySection ConnectVaporSidestream(MaterialStream stream, int stage, double factor)
        {
            var con = new TrayConnectivity() { Stage = stage, Stream = stream, Phase = PhaseState.Vapour };
            con.Factor.ValueInSI = factor;
            _sidestream.Add(con);
            Connect("Sidestreams", stream);
            return this;
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn").Streams[0];
            var LIn = FindMaterialPort("LIn").Streams[0];
            var VOut = FindMaterialPort("VOut").Streams[0];
            var LOut = FindMaterialPort("LOut").Streams[0];
            var Sidestreams = FindMaterialPort("Sidestreams");

            var pscale = 1e4;
            var hscale = 1e6;
            var L0 = LIn.Mixed.TotalMolarflow;
            var x0 = LIn.Mixed.ComponentMolarFraction;
            var VNT = VIn.Mixed.TotalMolarflow;
            var yNT = VIn.Mixed.ComponentMolarFraction;

            ApplyIgnoredComponents();

            for (var i = 0; i < NumberOfTrays; i++)
            {
                var tray = _trays[i];

                //(M)ass Balance
                var Mi = Sym.Par(1 + _trays[i].RV) * tray.V + Sym.Par(1 + tray.RL) * _trays[i].L;
                if (i == 0)
                    AddEquationToEquationSystem(problem, ((_trays[i + 1].V + L0 + tray.F)).IsEqualTo(Mi));
                else if (i == NumberOfTrays - 1)
                    AddEquationToEquationSystem(problem, ((VNT + _trays[i - 1].L + tray.F)).IsEqualTo(Mi));
                else
                    AddEquationToEquationSystem(problem, ((_trays[i + 1].V + _trays[i - 1].L + tray.F)).IsEqualTo(Mi));

                //Component Massbalance
                for (var comp = 0; comp < NC; comp++)
                {
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        var nij = Sym.Par(1 + tray.RV) * tray.V * tray.y[comp] + Sym.Par(1 + tray.RL) * tray.L * tray.x[comp];
                        if (i == 0)
                            AddEquationToEquationSystem(problem, ((L0 * x0[comp] + _trays[i + 1].V * _trays[i + 1].y[comp] + tray.F * tray.z[comp])).IsEqualTo(nij));
                        else if (i == NumberOfTrays - 1)
                            AddEquationToEquationSystem(problem, ((VNT * yNT[comp] + _trays[i - 1].L * _trays[i - 1].x[comp] + tray.F * tray.z[comp])).IsEqualTo(nij));
                        else
                            AddEquationToEquationSystem(problem, ((_trays[i + 1].V * _trays[i + 1].y[comp] + _trays[i - 1].L * _trays[i - 1].x[comp] + tray.F * tray.z[comp])).IsEqualTo(nij));
                    }
                }

                //(E)quilibrium
                for (var comp = 0; comp < NC; comp++)
                {
                    System.EquationFactory.EquilibriumCoefficient(System, tray.K[comp], tray.T, tray.p, tray.x.ToList(), tray.y.ToList(), comp);
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        AddEquationToEquationSystem(problem, (tray.yeq[comp]).IsEqualTo(tray.K[comp] * tray.x[comp]));
                    }

                }

                //(S)ummation                
                AddEquationToEquationSystem(problem, Sym.Sum(0, NC, (j) => Sym.Par(tray.x[j] - tray.yeq[j])).IsEqualTo(0));

                //Ent(H)alpy
                tray.HL.BindTo(Sym.Sum(0, NC, (idx) => tray.x[idx] * System.EquationFactory.GetLiquidEnthalpyExpression(System, idx, tray.T)));
                tray.HV.BindTo(Sym.Sum(0, NC, (idx) => tray.y[idx] * System.EquationFactory.GetVaporEnthalpyExpression(System, idx, tray.TV)));
                var Hi = Sym.Par(Sym.Par(1 + tray.RV) * tray.V * tray.HV + Sym.Par(1 + tray.RL) * tray.L * tray.HL) / hscale;
                if (i == 0)
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i + 1].V * _trays[i + 1].HV + L0 * LIn.Mixed.SpecificEnthalpy + tray.F * tray.HF + tray.Q) / hscale).IsEqualTo(Hi));
                else if (i == NumberOfTrays - 1)
                    AddEquationToEquationSystem(problem, (Sym.Par(VNT * VIn.Mixed.SpecificEnthalpy + _trays[i - 1].L * _trays[i - 1].HL + tray.F * tray.HF + tray.Q) / hscale).IsEqualTo(Hi));
                else
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i + 1].V * _trays[i + 1].HV + _trays[i - 1].L * _trays[i - 1].HL + tray.F * tray.HF + tray.Q) / hscale).IsEqualTo(Hi));


                if (EfficiencyType == TrayEfficiencyType.None)
                {
                    AddEquationToEquationSystem(problem, (tray.T / 100).IsEqualTo(tray.TV / 100));
                    //tray.TV.BindTo(tray.T);

                    for (var comp = 0; comp < NC; comp++)
                    {       // AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(tray.yeq[comp]));
                        tray.yeq[comp].BindTo(tray.y[comp]);
                    }
                }

                if (EfficiencyType == TrayEfficiencyType.Murphree)
                {
                    AddEquationToEquationSystem(problem, (tray.T / 100).IsEqualTo(tray.TV / 100));
                    //tray.TV.BindTo(tray.T);

                    for (var comp = 0; comp < NC; comp++)
                    {
                        tray.yeq[comp].Unbind();
                        if (!IgnoredComponents.Contains(System.Components[comp]))
                        {
                            if (i < NumberOfTrays - 1)
                            {
                                Expression yIN = _trays[i + 1].y[comp];

                                if (_feeds.Any(f => f.Stage == i + 2))
                                {
                                    yIN = (_trays[i + 1].y[comp] * _trays[i + 1].V + _trays[i].z[comp] * _trays[i].F) / (_trays[i + 1].V + _trays[i].F);
                                }

                                AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(yIN + tray.eps * Sym.Par(_trays[i].yeq[comp] - yIN)));
                            }
                            else
                                AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(VIn.Mixed.ComponentMolarFraction[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - VIn.Mixed.ComponentMolarFraction[comp])));
                        }
                    }
                }

                if (EfficiencyType == TrayEfficiencyType.ExtendedMurphree)
                {
                    // tray.TV.Unbind();
                    if (i < NumberOfTrays - 1)
                    {
                        AddEquationToEquationSystem(problem, (tray.TV).IsEqualTo(_trays[i + 1].TV + tray.eps * Sym.Par(_trays[i].T - _trays[i + 1].TV)));
                    }
                    else
                        AddEquationToEquationSystem(problem, (tray.TV).IsEqualTo(VIn.Mixed.Temperature + tray.eps * Sym.Par(_trays[i].T - VIn.Mixed.Temperature)));

                    for (var comp = 0; comp < NC; comp++)
                    {
                        tray.yeq[comp].Unbind();
                        if (!IgnoredComponents.Contains(System.Components[comp]))
                        {
                            if (i < NumberOfTrays - 1)
                                AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(_trays[i + 1].y[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - _trays[i + 1].y[comp])));
                            else
                                AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(VIn.Mixed.ComponentMolarFraction[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - VIn.Mixed.ComponentMolarFraction[comp])));
                        }
                    }
                }

                if (EfficiencyType == TrayEfficiencyType.BausaMurphree)
                {
                    // tray.TV.Unbind();
                    if (i < NumberOfTrays - 1)
                    {
                        Expression TIN = _trays[i + 1].TV;

                        if (_feeds.Any(f => f.Stage == i + 1 && f.Phase == PhaseState.Vapour))
                        {
                            var nextFeed = _feeds.FirstOrDefault(f => f.Stage == i + 1 && f.Phase == PhaseState.Vapour);
                            TIN = (_trays[i + 1].T * _trays[i + 1].V + nextFeed.Stream.Mixed.Temperature * _trays[i].F) / (_trays[i + 1].V + _trays[i].F);
                        }
                        AddEquationToEquationSystem(problem, (tray.TV).IsEqualTo(TIN + tray.eps * Sym.Par(_trays[i].T - TIN)));
                    }
                    else
                        AddEquationToEquationSystem(problem, (tray.TV).IsEqualTo(VIn.Mixed.Temperature + tray.eps * Sym.Par(_trays[i].T - VIn.Mixed.Temperature)));

                    for (var comp = 0; comp < NC; comp++)
                    {
                        tray.yeq[comp].Unbind();
                        if (!IgnoredComponents.Contains(System.Components[comp]))
                        {
                            if (i < NumberOfTrays - 1)
                            {
                                Expression yIN = _trays[i + 1].y[comp];

                                if (_feeds.Any(f => f.Stage == i + 1 && f.Phase == PhaseState.Vapour))
                                {
                                    yIN = (_trays[i + 1].y[comp] * _trays[i + 1].V + _trays[i].z[comp] * _trays[i].F) / (_trays[i + 1].V + _trays[i].F);
                                }
                                AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(yIN + tray.eps * Sym.Par(_trays[i].yeq[comp] - yIN)));
                            }
                            else
                                AddEquationToEquationSystem(problem, (tray.y[comp]).IsEqualTo(VIn.Mixed.ComponentMolarFraction[comp] + tray.eps * Sym.Par(_trays[i].yeq[comp] - VIn.Mixed.ComponentMolarFraction[comp])));
                        }
                    }
                }


                //Pressure profile
                if (i == NumberOfTrays - 1)
                    AddEquationToEquationSystem(problem, (tray.p / pscale).IsEqualTo((VIn.Mixed.Pressure - _trays[NumberOfTrays - 1].DP) / pscale));
                else
                    AddEquationToEquationSystem(problem, (tray.p / pscale).IsEqualTo(Sym.Par(_trays[i + 1].p - _trays[i].DP) / pscale));

            }

            for (var comp = 0; comp < NC; comp++)
            {
                AddEquationToEquationSystem(problem, VOut.Mixed.ComponentMolarflow[comp].IsEqualTo(_trays[0].V * _trays[0].y[comp]));
                AddEquationToEquationSystem(problem, LOut.Mixed.ComponentMolarflow[comp].IsEqualTo(_trays[NumberOfTrays - 1].L * _trays[NumberOfTrays - 1].x[comp]));
            }

            // AddEquationToEquationSystem(problem, VOut.Mixed.Temperature.IsEqualTo(_trays[0].TV));
            AddEquationToEquationSystem(problem, (VOut.Mixed.SpecificEnthalpy/1e6).IsEqualTo(_trays[0].HV / 1e6));
            //AddEquationToEquationSystem(problem, VOut.Mixed.Pressure.IsEqualTo(_trays[0].p - _trays[0].DP));
            AddEquationToEquationSystem(problem, VOut.Mixed.Pressure.IsEqualTo(_trays[0].p));
            AddEquationToEquationSystem(problem, LOut.Mixed.Temperature.IsEqualTo(_trays[NumberOfTrays - 1].T));
            AddEquationToEquationSystem(problem, LOut.Mixed.Pressure.IsEqualTo(_trays[NumberOfTrays - 1].p));


            foreach (var feed in _feeds)
            {
                _trays[feed.Stage - 1].HF.Unfix();
                _trays[feed.Stage - 1].F.Unfix();
                _trays[feed.Stage - 1].HF.IsConstant = false;
                _trays[feed.Stage - 1].F.IsConstant = false;

                _trays[feed.Stage - 1].HF.ValueInSI = feed.Stream.Liquid.SpecificEnthalpy.ValueInSI;
                _trays[feed.Stage - 1].F.ValueInSI = feed.Stream.Liquid.TotalMolarflow.ValueInSI;

                if (feed.Stage > 1 && feed.Phase == PhaseState.LiquidVapor)
                {
                    _trays[feed.Stage - 2].HF.Unfix();
                    _trays[feed.Stage - 2].F.Unfix();
                    _trays[feed.Stage - 2].HF.IsConstant = false;
                    _trays[feed.Stage - 2].F.IsConstant = false;

                    _trays[feed.Stage - 1].HF.ValueInSI = feed.Stream.Liquid.SpecificEnthalpy.ValueInSI;
                    _trays[feed.Stage - 1].F.ValueInSI = feed.Stream.Liquid.TotalMolarflow.ValueInSI;


                    _trays[feed.Stage - 2].HF.ValueInSI = feed.Stream.Vapor.SpecificEnthalpy.ValueInSI;
                    _trays[feed.Stage - 2].F.ValueInSI = feed.Stream.Vapor.TotalMolarflow.ValueInSI;
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 2].F).IsEqualTo(feed.Stream.Vapor.TotalMolarflow));
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 2].HF / 1e4).IsEqualTo(feed.Stream.Vapor.SpecificEnthalpy / 1e4));
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].F).IsEqualTo(feed.Stream.Liquid.TotalMolarflow));
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].HF / 1e4).IsEqualTo(feed.Stream.Liquid.SpecificEnthalpy / 1e4));

                }
                else
                {
                    _trays[feed.Stage - 1].HF.ValueInSI = feed.Stream.Mixed.SpecificEnthalpy.ValueInSI;
                    _trays[feed.Stage - 1].F.ValueInSI = feed.Stream.Mixed.TotalMolarflow.ValueInSI;


                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].F).IsEqualTo(feed.Stream.Mixed.TotalMolarflow));
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].HF / 1e4).IsEqualTo(feed.Stream.Mixed.SpecificEnthalpy / 1e4));
                }

                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[feed.Stage - 1].z[comp].Unfix();
                    _trays[feed.Stage - 1].z[comp].IsConstant = false;


                    if (feed.Stage > 1 && feed.Phase == PhaseState.LiquidVapor)
                    {
                        _trays[feed.Stage - 2].z[comp].Unfix();
                        _trays[feed.Stage - 2].z[comp].IsConstant = false;

                        AddEquationToEquationSystem(problem, (_trays[feed.Stage - 2].z[comp]).IsEqualTo(feed.Stream.Vapor.ComponentMolarFraction[comp]));
                        _trays[feed.Stage - 2].z[comp].ValueInSI = feed.Stream.Vapor.ComponentMolarFraction[comp].ValueInSI;

                        AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].z[comp]).IsEqualTo(feed.Stream.Liquid.ComponentMolarFraction[comp]));
                        _trays[feed.Stage - 1].z[comp].ValueInSI = feed.Stream.Liquid.ComponentMolarFraction[comp].ValueInSI;

                    }
                    else
                    {
                        AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].z[comp]).IsEqualTo(feed.Stream.Mixed.ComponentMolarFraction[comp]));
                        _trays[feed.Stage - 1].z[comp].ValueInSI = feed.Stream.Mixed.ComponentMolarFraction[comp].ValueInSI;
                    }
                }
            }

            foreach (var feed in _sidestream)
            {
                if (feed.Phase == PhaseState.Liquid)
                {

                    _trays[feed.Stage - 1].W.Unfix();
                    _trays[feed.Stage - 1].RL.Unfix();
                    _trays[feed.Stage - 1].RL.ValueInSI = feed.Factor.ValueInSI;
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].RL).IsEqualTo(feed.Factor));
                    AddEquationToEquationSystem(problem, (_trays[feed.Stage - 1].W).IsEqualTo(_trays[feed.Stage - 1].RL * _trays[feed.Stage - 1].L));
                    _trays[feed.Stage - 1].W.ValueInSI = (_trays[feed.Stage - 1].RL.ValueInSI * _trays[feed.Stage - 1].L.ValueInSI);
                    _trays[feed.Stage - 1].RL.ValueInSI = feed.Factor.ValueInSI;

                    AddEquationToEquationSystem(problem, feed.Stream.Mixed.Pressure.IsEqualTo(_trays[feed.Stage - 1].p));
                    AddEquationToEquationSystem(problem, feed.Stream.Mixed.Temperature.IsEqualTo(_trays[feed.Stage - 1].T));
                    for (var comp = 0; comp < NC; comp++)
                    {
                        AddEquationToEquationSystem(problem, feed.Stream.Mixed.ComponentMolarflow[comp].IsEqualTo(_trays[feed.Stage - 1].W * _trays[feed.Stage - 1].x[comp]));
                    }
                }
            }


            base.FillEquationSystem(problem);
        }

        public TraySection Ignore(params string[] components)
        {
            foreach (var compId in components)
            {
                var comp = System.Components.FirstOrDefault(c => c.ID == compId);
                if (comp != null)
                    IgnoredComponents.Add(comp);
            }
            return this;
        }
        public TraySection MakeAdiabatic()
        {
            foreach (var tray in _trays)
            {
                tray.Q.FixValue(0);
            }

            return this;
        }

        public TraySection MakeIsobaric()
        {
            foreach (var tray in _trays)
            {
                tray.DP.FixValue(0);
            }
            return this;
        }

        public TraySection FixStageEfficiency(double value)
        {
            foreach (var tray in _trays)
            {
                tray.eps.FixValue(value);
            }
            return this;
        }

        public void FlashTrays()
        {
            var flash = new FlashRoutines(new Numerics.Solvers.Newton());
            MaterialStream stageStream = new MaterialStream("Stage", System);
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");

            for (int i = 0; i < NumberOfTrays; i++)
            {
                FlashCurrentStage(flash, stageStream, VIn, LIn, i);
            }

            for (int i = NumberOfTrays - 1; i >= 0; i--)
            {
                FlashCurrentStage(flash, stageStream, VIn, LIn, i);
            }
        }

        void ApplyIgnoredComponents()
        {
            int NT = (int)(NumberOfTrays);

            for (int c = 0; c < System.Components.Count; c++)
            {
                if (IgnoredComponents.Contains(System.Components[c]))
                {
                    for (int i = 0; i < NT; i++)
                    {
                        _trays[i].x[c].FixValue(0);
                        _trays[i].y[c].FixValue(0);
                        _trays[i].yeq[c].FixValue(0);

                        _trays[i].x[c].IsConstant = true;
                        _trays[i].y[c].IsConstant = true;
                        _trays[i].yeq[c].IsConstant = true;
                    }
                }
            }
        }

        private void FlashCurrentStage(FlashRoutines flash, MaterialStream stageStream, Port<MaterialStream> VIn, Port<MaterialStream> LIn, int i)
        {
            stageStream.Mixed.Temperature.ValueInSI = _trays[i].T.ValueInSI;
            stageStream.Mixed.Pressure.ValueInSI = _trays[i].p.ValueInSI;
            var enthsum = 0.0;

            if (i > 0)
                enthsum += _trays[i - 1].L.ValueInSI * _trays[i - 1].HL.ValueInSI;
            else
                enthsum += LIn.Streams[0].Mixed.TotalMolarflow.ValueInSI * LIn.Streams[0].Mixed.SpecificEnthalpy.ValueInSI;

            if (i < NumberOfTrays - 1)
                enthsum += _trays[i + 1].V.ValueInSI * _trays[i + 1].HV.ValueInSI;
            else
                enthsum += VIn.Streams[0].Mixed.TotalMolarflow.ValueInSI * VIn.Streams[0].Mixed.SpecificEnthalpy.ValueInSI;
            enthsum += _trays[i].F.ValueInSI * _trays[i].HF.ValueInSI;

            for (int j = 0; j < System.Components.Count; j++)
            {
                var flowsum = 0.0;

                if (i > 0)
                    flowsum += _trays[i - 1].L.ValueInSI * _trays[i - 1].x[j].ValueInSI;
                else
                    flowsum += LIn.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI;

                flowsum += _trays[i].F.ValueInSI * _trays[i].z[j].ValueInSI;

                if (i < NumberOfTrays - 1)
                    flowsum += _trays[i].V.ValueInSI * _trays[i].y[j].ValueInSI;
                else
                    flowsum += VIn.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI;

                stageStream.Mixed.ComponentMolarflow[j].ValueInSI = flowsum;
            }

            //stageStream.Mixed. = enthsum ;

            //stageStream.Vfmolar.ValueInSI = 0.5;
            //flash.CalculateZP(stageStream);
            flash.CalculatePQ(stageStream, enthsum);

            _trays[i].V.ValueInSI = stageStream.Vapor.TotalMolarflow.ValueInSI;
            _trays[i].L.ValueInSI = stageStream.Liquid.TotalMolarflow.ValueInSI;

            for (int j = 0; j < System.Components.Count; j++)
            {
                _trays[i].y[j].ValueInSI = stageStream.Vapor.ComponentMolarFraction[j].ValueInSI;
                _trays[i].x[j].ValueInSI = stageStream.Liquid.ComponentMolarFraction[j].ValueInSI;
            }
        }


        void InitAbsorber()
        {
            int NC = System.Components.Count;

            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");

            var TTop = LIn.Streams[0].Mixed.Temperature.ValueInSI;
            var TBot = VIn.Streams[0].Mixed.Temperature.ValueInSI;

            for (int i = 0; i < NumberOfTrays; i++)
            {
                _trays[i].T.ValueInSI = TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i;
                _trays[i].TV.ValueInSI = TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i;

                if (i == 0)
                    _trays[i].L.ValueInSI = LIn.Streams[0].Mixed.TotalMolarflow.ValueInSI;
                else
                    _trays[i].L.ValueInSI = _trays[i - 1].L.ValueInSI;

                _trays[i].V.ValueInSI = VIn.Streams[0].Mixed.TotalMolarflow.ValueInSI;
                _trays[i].HV.ValueInSI = VIn.Streams[0].Mixed.SpecificEnthalpy.ValueInSI;
                _trays[i].HL.ValueInSI = LIn.Streams[0].Mixed.SpecificEnthalpy.ValueInSI;

                for (int j = 0; j < System.Components.Count; j++)

                {
                    _trays[i].x[j].ValueInSI = LIn.Streams[0].Mixed.ComponentMolarFraction[j].ValueInSI;
                    _trays[i].y[j].ValueInSI = VIn.Streams[0].Mixed.ComponentMolarFraction[j].ValueInSI;
                }
            }

            for (int i = NumberOfTrays - 1; i >= 0; i--)
            {
                if (i < NumberOfTrays - 1)
                    _trays[i].p.ValueInSI = _trays[i + 1].p.ValueInSI - _trays[i + 1].DP.ValueInSI;
                else
                    _trays[i].p.ValueInSI = VIn.Streams[0].Mixed.Pressure.ValueInSI;
            }

            VOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[0].T.ValueInSI;
            VOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[0].p.ValueInSI - _trays[0].DP.ValueInSI;

            LOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[NumberOfTrays - 1].T.ValueInSI;
            LOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[NumberOfTrays - 1].p.ValueInSI;

            for (int j = 0; j < System.Components.Count; j++)
            {
                VOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[0].V.ValueInSI * _trays[0].y[j].ValueInSI;
                LOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[NumberOfTrays - 1].L.ValueInSI * _trays[NumberOfTrays - 1].x[j].ValueInSI;
            }

            InitOutlets();
        }

        void InitRectifaction()
        {
            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");
            var Sidestreams = FindMaterialPort("Sidestreams");

            var TTop = LIn.Streams[0].Mixed.Temperature.ValueInSI;
            var TBot = VIn.Streams[0].Mixed.Temperature.ValueInSI;

            var flash1 = new FlashRoutines(new Numerics.Solvers.Newton());

            var feedcopy = new MaterialStream("FC", In.Streams[0].System);
            feedcopy.CopyFrom(In.Streams[0]);
            feedcopy.Vfmolar.ValueInSI = 0.01;
            flash1.CalculateZP(feedcopy);
            TTop = feedcopy.Mixed.Temperature.ValueInSI;
            feedcopy.Vfmolar.ValueInSI = 0.99;
            flash1.CalculateZP(feedcopy);
            TBot = feedcopy.Mixed.Temperature.ValueInSI;

            foreach (var feed in _feeds)
            {
                _trays[feed.Stage - 1].HF.ValueInSI = feed.Stream.Mixed.SpecificEnthalpy.ValueInSI;
                _trays[feed.Stage - 1].F.ValueInSI = feed.Stream.Mixed.TotalMolarflow.ValueInSI;
                _trays[feed.Stage - 1].HF.IsConstant = false;
                _trays[feed.Stage - 1].F.IsConstant = false;
                for (var comp = 0; comp < NC; comp++)
                {
                    _trays[feed.Stage - 1].z[comp].ValueInSI = feed.Stream.Mixed.ComponentMolarFraction[comp].ValueInSI;
                    _trays[feed.Stage - 1].z[comp].IsConstant = false;
                }
            }

            for (int i = 0; i < NumberOfTrays; i++)
            {
                _trays[i].T.ValueInSI = TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i;
                _trays[i].TV.ValueInSI = TTop + (TBot - TTop) / (double)(NumberOfTrays - 1) * i;
                if (i == 0)
                    _trays[i].L.ValueInSI = In.Streams[0].Mixed.TotalMolarflow.ValueInSI * 0.5;
                else
                    _trays[i].L.ValueInSI = _trays[i - 1].L.ValueInSI + _trays[i].F.ValueInSI;

                _trays[i].V.ValueInSI = In.Streams[0].Mixed.TotalMolarflow.ValueInSI;

                for (int j = 0; j < System.Components.Count; j++)
                {
                    _trays[i].x[j].ValueInSI = _feeds[0].Stream.Liquid.ComponentMolarFraction[j].ValueInSI;
                    _trays[i].y[j].ValueInSI = _feeds[0].Stream.Vapor.ComponentMolarFraction[j].ValueInSI;
                }
            }

            for (int i = NumberOfTrays - 1; i >= 0; i--)
            {
                if (i < NumberOfTrays - 1)
                    _trays[i].p.ValueInSI = _trays[i + 1].p.ValueInSI - _trays[i + 1].DP.ValueInSI;
                else
                    _trays[i].p.ValueInSI = VIn.Streams[0].Mixed.Pressure.ValueInSI;
            }

            InitOutlets();

        }

        void InitOutlets()
        {
            int NC = System.Components.Count;



            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");

            VOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[0].T.ValueInSI;
            VOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[0].p.ValueInSI - _trays[0].DP.ValueInSI;

            LOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[NumberOfTrays - 1].T.ValueInSI;
            LOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[NumberOfTrays - 1].p.ValueInSI;

            for (int j = 0; j < System.Components.Count; j++)
            {
                VOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[0].V.ValueInSI * _trays[0].y[j].ValueInSI;
                LOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[NumberOfTrays - 1].L.ValueInSI * _trays[NumberOfTrays - 1].x[j].ValueInSI;
            }
            var flash = new FlashRoutines(new Numerics.Solvers.Newton());

            VOut.Streams[0].Vfmolar.ValueInSI = 1.0;
            LOut.Streams[0].Vfmolar.ValueInSI = 0.0;
            flash.CalculateZP(VOut.Streams[0]);
            flash.CalculateZP(LOut.Streams[0]);


            // VOut.Streams[0].State = PhaseState.DewPoint;
            LOut.Streams[0].State = PhaseState.BubblePoint;

            foreach (var feed in _sidestream)
            {
                if (feed.Phase == PhaseState.Liquid)
                {
                    _trays[feed.Stage - 1].RL.ValueInSI = feed.Factor.ValueInSI;
                    _trays[feed.Stage - 1].W.ValueInSI = (_trays[feed.Stage - 1].RL.ValueInSI * _trays[feed.Stage - 1].L.ValueInSI);

                    feed.Stream.Mixed.Temperature.ValueInSI = _trays[feed.Stage - 1].T.ValueInSI;
                    feed.Stream.Mixed.Pressure.ValueInSI = _trays[feed.Stage - 1].p.ValueInSI;
                    feed.Stream.Mixed.TotalMolarflow.ValueInSI = _trays[feed.Stage - 1].W.ValueInSI;
                    for (var comp = 0; comp < NC; comp++)
                    {
                        feed.Stream.Mixed.ComponentMolarflow[comp].ValueInSI = _trays[feed.Stage - 1].x[comp].ValueInSI * _trays[feed.Stage - 1].W.ValueInSI;
                    }
                    feed.Stream.FlashPT();
                }
            }


        }

        public override ProcessUnit Initialize()
        {

            int NC = System.Components.Count;
            var In = FindMaterialPort("Feeds");
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");
            var Sidestreams = FindMaterialPort("Sidestreams");

            if (LIn.IsConnected == false)
                throw new InvalidOperationException("LIn must be connected");
            if (VIn.IsConnected == false)
                throw new InvalidOperationException("VIn must be connected");
            if (LOut.IsConnected == false)
                throw new InvalidOperationException("LOut must be connected");
            if (VOut.IsConnected == false)
                throw new InvalidOperationException("VOut must be connected");

            if (In.IsConnected)
                InitRectifaction();
            else
                InitAbsorber();

            return this;


        }
    }
}
