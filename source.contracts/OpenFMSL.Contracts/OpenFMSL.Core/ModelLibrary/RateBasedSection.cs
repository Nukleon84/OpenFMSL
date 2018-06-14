﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Flowsheeting;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Numerics.Solvers;
using OpenFMSL.Core.Thermodynamics;
using OpenFMSL.Core.UnitsOfMeasure;

namespace OpenFMSL.Core.ModelLibrary
{

    public class NonEquilibriumTray
    {
        public Variable TL;
        public Variable TI;
        public Variable TV;

        public Variable p;

        public Variable Q;

        public Variable L;
        public Variable V;

        public Variable HL;
        public Variable HV;

        public Variable[] N;
        public Variable E;

        public Variable[] x;
        public Variable[] y;

        public Variable[] yI;
        public Variable[] xI;

        public Variable[] K;

        public Variable DP;

        public Variable h;
        public Variable d;

        public Variable aspez;
        public Variable aeff;

        public Variable dhyd;
        public Variable uV;
        public Variable ReV;

        public Variable[,] Beta;


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

        public NonEquilibriumTray(int number, ThermodynamicSystem system)
        {
            _system = system;
            Number = number;
            var numString = number.ToString();

            h = system.VariableFactory.CreateVariable("h", numString, "Packing height", PhysicalDimension.Length);
            d = system.VariableFactory.CreateVariable("d", numString, "Packing diameter", PhysicalDimension.Length);
            aspez = system.VariableFactory.CreateVariable("aspec", numString, "Specific Area", PhysicalDimension.SpecificArea);
            aeff = system.VariableFactory.CreateVariable("aeff", numString, "Effective Area", PhysicalDimension.Area);

            dhyd = system.VariableFactory.CreateVariable("dhyd", numString, "Hydrodynamic diameter", PhysicalDimension.Length);
            uV = system.VariableFactory.CreateVariable("uV", numString, "Superficial velocity", PhysicalDimension.Velocity);
            ReV = system.VariableFactory.CreateVariable("ReV", numString, "Reynolds number vapor", PhysicalDimension.Dimensionless);

            dhyd.ValueInSI = 0.00723;

            h.ValueInSI = 3;
            d.ValueInSI = 0.7;
            aspez.ValueInSI = 250;
            aeff.ValueInSI = 100;
            h.IsFixed = true;
            d.IsFixed = true;
            aspez.IsFixed = true;
            dhyd.IsFixed = true;


            TL = system.VariableFactory.CreateVariable("TL", numString, "Liquid Temperature", PhysicalDimension.Temperature);
            TI = system.VariableFactory.CreateVariable("TI", numString, "Interphase Temperature", PhysicalDimension.Temperature);
            TV = system.VariableFactory.CreateVariable("TV", numString, "Vapor Temperature", PhysicalDimension.Temperature);

            DP = system.VariableFactory.CreateVariable("DP", numString, "Pressure Drop", PhysicalDimension.Pressure);
            DP.LowerBound = 0;
            DP.ValueInSI = 0;

            p = system.VariableFactory.CreateVariable("P", numString, "Pressure", PhysicalDimension.Pressure);
            Q = system.VariableFactory.CreateVariable("Q", numString, "Heat Duty to liquid phase", PhysicalDimension.HeatFlow);
            E = system.VariableFactory.CreateVariable("E", numString, "Total heat transfer rate (Interface)", PhysicalDimension.HeatFlow);

            L = system.VariableFactory.CreateVariable("L", numString, "Liquid molar flow", PhysicalDimension.MolarFlow);
            V = system.VariableFactory.CreateVariable("V", numString, "Vapor molar flow", PhysicalDimension.MolarFlow);

            HL = system.VariableFactory.CreateVariable("HL", numString, "Liquid specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);
            HV = system.VariableFactory.CreateVariable("HV", numString, "Vapor specific molar enthalpy", PhysicalDimension.SpecificMolarEnthalpy);


            K = new Variable[system.Components.Count];
            x = new Variable[system.Components.Count];
            y = new Variable[system.Components.Count];
            xI = new Variable[system.Components.Count];
            yI = new Variable[system.Components.Count];

            N = new Variable[system.Components.Count];

            Beta = new Variable[system.Components.Count, system.Components.Count];

            for (int i = 0; i < system.Components.Count; i++)
            {
                K[i] = system.VariableFactory.CreateVariable("K", numString + ", " + system.Components[i].ID, "Equilibrium partition coefficient", PhysicalDimension.Dimensionless);
                K[i].ValueInSI = 1.2;
                x[i] = system.VariableFactory.CreateVariable("x", numString + ", " + system.Components[i].ID, "Liquid molar fraction", PhysicalDimension.MolarFraction);
                y[i] = system.VariableFactory.CreateVariable("y", numString + ", " + system.Components[i].ID, "Vapor molar fraction", PhysicalDimension.MolarFraction);
                yI[i] = system.VariableFactory.CreateVariable("yI", numString + ", " + system.Components[i].ID, "Vapor molar fraction (Interface)", PhysicalDimension.MolarFraction);
                xI[i] = system.VariableFactory.CreateVariable("xI", numString + ", " + system.Components[i].ID, "Liquid molar fraction (Interface)", PhysicalDimension.MolarFraction);

                N[i] = system.VariableFactory.CreateVariable("N", numString + ", " + system.Components[i].ID, "Total transfer rate (Interface)", PhysicalDimension.MolarFlow);
                N[i].LowerBound = -1e6;
                N[i].ValueInSI = 0.0;

                for (int j = 0; j < system.Components.Count; j++)
                {
                    Beta[i, j] = system.VariableFactory.CreateVariable("Beta", numString + ", " + system.Components[i].ID + ", " + system.Components[j].ID, "Masstransfer coefficient)", PhysicalDimension.MassTransferCoefficient);
                    Beta[i, j].IsFixed = true;
                    if (i != j)
                        Beta[i, j].ValueInSI = 0.02;
                }
            }
        }
    }


    public class RateBasedSection : ProcessUnit
    {
        int _numberOfElements = -1;

        bool _heatTransferResistanceOnVapor = false;
        bool _heatTransferResistanceOnLiquid = false;
        bool _massTransferResistanceOnVapor = false;
        bool _massTransferResistanceOnLiquid = false;


        List<NonEquilibriumTray> _trays = new List<NonEquilibriumTray>();
        List<MolecularComponent> _ignoredComponents = new List<MolecularComponent>();
        bool _noContact = true;
        bool _betaIsConstant = true;
        public double[] SherwoodParameter= { 0.0348, 0.77, 1.0 / 3.0 };

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
        public int NumberOfElements
        {
            get
            {
                return _numberOfElements;
            }

            set
            {
                _numberOfElements = value;
            }
        }

        public bool NoContact
        {
            get
            {
                return _noContact;
            }

            set
            {
                _noContact = value;
            }
        }

        public bool BetaIsConstant
        {
            get
            {
                return _betaIsConstant;
            }

            set
            {
                _betaIsConstant = value;
            }
        }

        public RateBasedSection(string name, ThermodynamicSystem system, int numberOfElements) : base(name, system)
        {
            Class = "RateSection";
            NumberOfElements = numberOfElements;
            Icon.IconType = IconTypes.RateBasedSection;
            var NC = system.Components.Count;

            MaterialPorts.Add(new Port<MaterialStream>("VIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("LIn", PortDirection.In, 1));
            MaterialPorts.Add(new Port<MaterialStream>("VOut", PortDirection.Out, 1));
            MaterialPorts.Add(new Port<MaterialStream>("LOut", PortDirection.Out, 1));


            for (int i = 0; i < NumberOfElements; i++)
            {
                var tray = new NonEquilibriumTray(i + 1, system);
                _trays.Add(tray);
            }

            AddVariables(_trays.Select(t => t.TL).ToArray());
            AddVariables(_trays.Select(t => t.TI).ToArray());
            AddVariables(_trays.Select(t => t.TV).ToArray());

            AddVariables(_trays.Select(t => t.p).ToArray());
            AddVariables(_trays.Select(t => t.DP).ToArray());
            AddVariables(_trays.Select(t => t.Q).ToArray());
            AddVariables(_trays.Select(t => t.E).ToArray());
            AddVariables(_trays.Select(t => t.L).ToArray());
            AddVariables(_trays.Select(t => t.V).ToArray());

            AddVariables(_trays.Select(t => t.HL).ToArray());
            AddVariables(_trays.Select(t => t.HV).ToArray());
            AddVariables(_trays.Select(t => t.aeff).ToArray());

            AddVariables(_trays.Select(t => t.aspez).ToArray());
            AddVariables(_trays.Select(t => t.d).ToArray());
            AddVariables(_trays.Select(t => t.h).ToArray());
            AddVariables(_trays.Select(t => t.dhyd).ToArray());

            AddVariables(_trays.Select(t => t.ReV).ToArray());
            AddVariables(_trays.Select(t => t.uV).ToArray());

            for (int i = 0; i < NumberOfElements; i++)
            {
                AddVariables(_trays[i].K);
            }
            for (int i = 0; i < NumberOfElements; i++)
            {
                AddVariables(_trays[i].x);
            }

            for (int i = 0; i < NumberOfElements; i++)
            {
                AddVariables(_trays[i].xI);
            }

            for (int i = 0; i < NumberOfElements; i++)
            {
                AddVariables(_trays[i].y);
            }

            for (int i = 0; i < NumberOfElements; i++)
            {
                AddVariables(_trays[i].yI);
            }

            for (int i = 0; i < NumberOfElements; i++)
            {
                AddVariables(_trays[i].N);
            }

            for (int n = 0; n < NumberOfElements; n++)
            {
                for (int i = 0; i < NC; i++)
                {
                    for (int j = 0; j < NC; j++)
                    {
                        AddVariables(_trays[n].Beta[i, j]);
                    }
                }
            }
        }

        public RateBasedSection MakeAdiabatic()
        {
            foreach (var tray in _trays)
            {
                tray.Q.FixValue(0);
            }

            return this;
        }

        public RateBasedSection MakeIsobaric()
        {
            foreach (var tray in _trays)
            {
                tray.DP.FixValue(0);
            }
            return this;
        }

        public RateBasedSection SetModel(bool massTransferResistanceOnVapor, bool massTransferResistanceOnLiquid, bool heatTransferResistanceOnVapor, bool heatTransferResistanceOnLiquid)
        {
            _massTransferResistanceOnVapor = massTransferResistanceOnVapor;
            _massTransferResistanceOnLiquid = massTransferResistanceOnLiquid;
            _heatTransferResistanceOnVapor = heatTransferResistanceOnVapor;
            _heatTransferResistanceOnLiquid = heatTransferResistanceOnLiquid;
            return this;
        }

        public RateBasedSection SetInitMode()
        {
            NoContact = true;
            return this;
        }
        public RateBasedSection SetSolveMode()
        {
            NoContact = false;
            return this;
        }
        public RateBasedSection EnableBetaCalculation()
        {
            BetaIsConstant = false;
            ChangeAllBetaFixStateTo(false);
            return this;
        }

        private void ChangeAllBetaFixStateTo(bool newState)
        {
            int NC = System.Components.Count;

            int NT = (int)(NumberOfElements);

            for (int t = 0; t < NT; t++)
            {
                for (int i = 0; i < NC; i++)
                {
                    for (int j = 0; j < NC; j++)
                    {
                        _trays[t].Beta[i, j].IsFixed = newState;
                    }
                }
            }
        }

        public RateBasedSection DisableBetaCalculation()
        {
            BetaIsConstant = true;
            ChangeAllBetaFixStateTo(true);
            return this;
        }


        public RateBasedSection Ignore(params string[] components)
        {
            foreach (var compId in components)
            {
                var comp = System.Components.FirstOrDefault(c => c.ID == compId);
                if (comp != null)
                    IgnoredComponents.Add(comp);
            }
            return this;
        }

        void ApplyIgnoredComponents()
        {
            int NT = (int)(NumberOfElements);

            for (int c = 0; c < System.Components.Count; c++)
            {
                if (IgnoredComponents.Contains(System.Components[c]))
                {
                    for (int i = 0; i < NT; i++)
                    {
                        _trays[i].x[c].FixValue(0);
                        _trays[i].y[c].FixValue(0);
                        _trays[i].yI[c].FixValue(0);
                        _trays[i].xI[c].FixValue(0);

                        _trays[i].x[c].IsConstant = true;
                        _trays[i].y[c].IsConstant = true;
                        _trays[i].yI[c].IsConstant = true;
                        _trays[i].xI[c].IsConstant = true;
                    }
                }
            }
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            int NC = System.Components.Count;
            var VIn = FindMaterialPort("VIn").Streams[0];
            var LIn = FindMaterialPort("LIn").Streams[0];
            var VOut = FindMaterialPort("VOut").Streams[0];
            var LOut = FindMaterialPort("LOut").Streams[0];

            var pscale = 1e4;
            var hscale = 1e6;
            var L0 = LIn.Mixed.TotalMolarflow;
            var x0 = LIn.Mixed.ComponentMolarFraction;
            var VNT = VIn.Mixed.TotalMolarflow;
            var yNT = VIn.Mixed.ComponentMolarFraction;

            ApplyIgnoredComponents();

            for (var i = 0; i < NumberOfElements; i++)
            {
                var tray = _trays[i];

                //Component (M)assbalance
                for (var comp = 0; comp < NC; comp++)
                {
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        if (i == 0)
                        {
                            AddEquationToEquationSystem(problem, (_trays[i].V * _trays[i].y[comp] - _trays[i + 1].V * _trays[i + 1].y[comp] + _trays[i].N[comp]).IsEqualTo(0));
                            AddEquationToEquationSystem(problem, (_trays[i].L * _trays[i].x[comp] - L0 * x0[comp] - _trays[i].N[comp]).IsEqualTo(0));
                        }
                        else if (i == NumberOfElements - 1)
                        {
                            AddEquationToEquationSystem(problem, (_trays[i].V * _trays[i].y[comp] - VNT * yNT[comp] + _trays[i].N[comp]).IsEqualTo(0));
                            AddEquationToEquationSystem(problem, (_trays[i].L * _trays[i].x[comp] - _trays[i - 1].L * _trays[i - 1].x[comp] - _trays[i].N[comp]).IsEqualTo(0));
                        }
                        else
                        {
                            AddEquationToEquationSystem(problem, (_trays[i].V * _trays[i].y[comp] - _trays[i + 1].V * _trays[i + 1].y[comp] + _trays[i].N[comp]).IsEqualTo(0));
                            AddEquationToEquationSystem(problem, (_trays[i].L * _trays[i].x[comp] - _trays[i - 1].L * _trays[i - 1].x[comp] - _trays[i].N[comp]).IsEqualTo(0));
                        }
                    }
                }

                //(E)quilibrium
                for (var comp = 0; comp < NC; comp++)
                {
                    System.EquationFactory.EquilibriumCoefficient(System, tray.K[comp], tray.TI, tray.p, tray.xI.ToList(), tray.yI.ToList(), comp);
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        AddEquationToEquationSystem(problem, (tray.yI[comp]).IsEqualTo(tray.K[comp] * tray.xI[comp]));
                    }
                }


                //(R)ate Equation - Intefacial Mass Transfer
                for (var comp = 0; comp < NC; comp++)
                {
                    if (!IgnoredComponents.Contains(System.Components[comp]))
                    {
                        if (NoContact)
                        {
                            AddEquationToEquationSystem(problem, (_trays[i].N[comp]).IsEqualTo(0));
                            AddEquationToEquationSystem(problem, (_trays[i].xI[comp]).IsEqualTo(_trays[i].x[comp]));
                        }
                        else
                        {
                            if (!_massTransferResistanceOnLiquid)
                                AddEquationToEquationSystem(problem, (_trays[i].xI[comp]).IsEqualTo(_trays[i].x[comp]));
                            if (!_massTransferResistanceOnVapor)
                            {
                                AddEquationToEquationSystem(problem, (_trays[i].yI[comp]).IsEqualTo(_trays[i].y[comp]));
                            }
                            else
                            {
                                //Simplified Stefan-Maxwell Equation for Film model
                                var y = _trays[i].y;
                                var yI = _trays[i].yI;
                                var n = _trays[i].N;
                                var ii = comp;
                                var TV = _trays[i].TV;
                                var p = _trays[i].p;
                                var nu = Sym.Sum(0, NC, j => 1 / System.EquationFactory.GetVaporDensityExpression(System, System.Components[j], TV, p) * _trays[i].y[j]);
                                AddEquationToEquationSystem(problem,
                                    (1 / nu * _trays[i].aeff * Sym.Par(_trays[i].y[comp] - _trays[i].yI[comp])).IsEqualTo(
                                        Sym.SumX(0, NC, comp, j => (0.5 * Sym.Par(y[j] + yI[j]) * n[ii] - 0.5 * Sym.Par(y[ii] + yI[ii]) * n[j]) / _trays[i].Beta[ii, j])
                                        )
                                        );
                            }
                        }
                    }
                }

                if (!BetaIsConstant)
                {
                    for (int k = 0; k < NC; k++)
                    {
                        for (int j = 0; j < NC; j++)
                        {

                            if (k != j)
                            {
                                var parameterSet = System.BinaryParameters.FirstOrDefault(ps => ps.Name == "DIJ");
                                if (parameterSet == null)
                                    throw new ArgumentNullException("No Diffusion coefficients defined");

                                var DVij = parameterSet.Matrices["A"][k, j];

                                var rhoV = System.EquationFactory.GetAverageVaporDensityExpression(System, _trays[i].y, _trays[i].TV, _trays[i].p) * System.EquationFactory.GetAverageMolarWeightExpression(System, _trays[i].y) / 1000;
                                var etaV = System.EquationFactory.GetAverageVaporViscosityExpression(System, _trays[i].y, _trays[i].TV, _trays[i].p);
                                var Scij = etaV / (rhoV * DVij);
                                var Shij = SherwoodParameter[0] * Sym.Pow(_trays[i].ReV, SherwoodParameter[1]) * Sym.Pow(Scij, SherwoodParameter[2]);
                                AddEquationToEquationSystem(problem, _trays[i].Beta[k, j].IsEqualTo(Shij * DVij / _trays[i].dhyd));
                            }
                            else
                                AddEquationToEquationSystem(problem, _trays[i].Beta[k, j].IsEqualTo(0));
                        }
                    }
                }




                //(R)ate Equation - Intefacial Heat Transfer

                if (NoContact)
                {
                    AddEquationToEquationSystem(problem, (_trays[i].E).IsEqualTo(0));
                    AddEquationToEquationSystem(problem, (_trays[i].TI).IsEqualTo(0.5 * Sym.Par(_trays[i].TL + _trays[i].TV)));
                }
                else
                {
                    if (!_heatTransferResistanceOnLiquid)
                        AddEquationToEquationSystem(problem, (_trays[i].TI).IsEqualTo(_trays[i].TL));
                    if (!_heatTransferResistanceOnVapor)
                        AddEquationToEquationSystem(problem, (_trays[i].TI).IsEqualTo(_trays[i].TV));
                }


                //(S)ummation                
                AddEquationToEquationSystem(problem, Sym.Sum(0, NC, (j) => Sym.Par(tray.x[j])).IsEqualTo(1));
                AddEquationToEquationSystem(problem, Sym.Sum(0, NC, (j) => Sym.Par(tray.y[j])).IsEqualTo(1));


                //Hydraulics
                AddEquationToEquationSystem(problem, _trays[i].aeff.IsEqualTo(_trays[i].h * Math.PI / 4 * Sym.Pow(_trays[i].d, 2) * _trays[i].aspez));

                AddEquationToEquationSystem(problem, _trays[i].uV.IsEqualTo(_trays[i].V / (System.EquationFactory.GetAverageVaporDensityExpression(System, _trays[i].y, _trays[i].TV, _trays[i].p) * Math.PI / 4 * Sym.Pow(_trays[i].d, 2))));
                AddEquationToEquationSystem(problem, _trays[i].ReV.IsEqualTo(System.EquationFactory.GetAverageVaporDensityExpression(System, _trays[i].y, _trays[i].TV, _trays[i].p) * System.EquationFactory.GetAverageMolarWeightExpression(System, _trays[i].y) / 1000 * _trays[i].uV * _trays[i].dhyd / System.EquationFactory.GetAverageVaporViscosityExpression(System, _trays[i].y, _trays[i].TV, _trays[i].p)));


                //Ent(H)alpy
                tray.HL.BindTo(Sym.Sum(0, NC, (idx) => tray.x[idx] * System.EquationFactory.GetLiquidEnthalpyExpression(System, idx, tray.TL)));
                tray.HV.BindTo(Sym.Sum(0, NC, (idx) => tray.y[idx] * System.EquationFactory.GetVaporEnthalpyExpression(System, idx, tray.TV)));


                if (i == 0)
                {
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i].V * _trays[i].HV - _trays[i + 1].V * _trays[i + 1].HV + _trays[i].E) / hscale).IsEqualTo(0));
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i].L * _trays[i].HL - L0 * LIn.Mixed.SpecificEnthalpy - _trays[i].E + tray.Q) / hscale).IsEqualTo(0));
                }
                else if (i == NumberOfElements - 1)
                {
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i].V * _trays[i].HV - VNT * VIn.Mixed.SpecificEnthalpy + _trays[i].E) / hscale).IsEqualTo(0));
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i].L * _trays[i].HL - _trays[i - 1].L * _trays[i - 1].HL - _trays[i].E + tray.Q) / hscale).IsEqualTo(0));
                }
                else
                {
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i].V * _trays[i].HV - _trays[i + 1].V * _trays[i + 1].HV + _trays[i].E) / hscale).IsEqualTo(0));
                    AddEquationToEquationSystem(problem, (Sym.Par(_trays[i].L * _trays[i].HL - _trays[i - 1].L * _trays[i - 1].HL - _trays[i].E + tray.Q) / hscale).IsEqualTo(0));
                }

                //Pressure profile
                if (i == NumberOfElements - 1)
                    AddEquationToEquationSystem(problem, (tray.p / pscale).IsEqualTo((VIn.Mixed.Pressure - _trays[NumberOfElements - 1].DP) / pscale));
                else
                    AddEquationToEquationSystem(problem, (tray.p / pscale).IsEqualTo(Sym.Par(_trays[i + 1].p - _trays[i].DP) / pscale));

            }

            for (var comp = 0; comp < NC; comp++)
            {
                AddEquationToEquationSystem(problem, VOut.Mixed.ComponentMolarflow[comp].IsEqualTo(_trays[0].V * _trays[0].y[comp]));
                AddEquationToEquationSystem(problem, LOut.Mixed.ComponentMolarflow[comp].IsEqualTo(_trays[NumberOfElements - 1].L * _trays[NumberOfElements - 1].x[comp]));
            }

            AddEquationToEquationSystem(problem, (VOut.Mixed.SpecificEnthalpy / 1e6).IsEqualTo(_trays[0].HV / 1e6));

            // AddEquationToEquationSystem(problem, VOut.Mixed.Temperature.IsEqualTo(_trays[0].TV));
            AddEquationToEquationSystem(problem, VOut.Mixed.Pressure.IsEqualTo(_trays[0].p));
            AddEquationToEquationSystem(problem, LOut.Mixed.Temperature.IsEqualTo(_trays[NumberOfElements - 1].TL));
            AddEquationToEquationSystem(problem, LOut.Mixed.Pressure.IsEqualTo(_trays[NumberOfElements - 1].p));


            base.FillEquationSystem(problem);
        }

        public RateBasedSection SetHeight(double height, Unit uom, int start = 1, int end = 1000)
        {
            for (int i = start - 1; i < (end < NumberOfElements ? end : NumberOfElements); i++)
            {
                _trays[i].h.SetValue(height / ((double)NumberOfElements), uom);
            }
            return this;
        }
        public RateBasedSection SetSpecificArea(double specificArea, Unit uom, int start = 1, int end = 1000)
        {
            for (int i = start - 1; i < (end < NumberOfElements ? end : NumberOfElements); i++)
            {
                _trays[i].aspez.SetValue(specificArea, uom);
            }
            return this;
        }
        public RateBasedSection SetDiameter(double diameter, Unit uom, int start = 1, int end = 1000)
        {
            for (int i = start - 1; i < (end < NumberOfElements ? end : NumberOfElements); i++)
            {
                _trays[i].d.SetValue(diameter, uom);
            }
            return this;
        }

        #region Initialization
        void InitAbsorber()
        {
            int NC = System.Components.Count;

            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");

            var TTop = LIn.Streams[0].Mixed.Temperature.ValueInSI;
            var TBot = VIn.Streams[0].Mixed.Temperature.ValueInSI;

            for (int i = 0; i < NumberOfElements; i++)
            {
                _trays[i].TL.ValueInSI = TTop;
                _trays[i].TV.ValueInSI = TBot;

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

                    _trays[i].xI[j].ValueInSI = LIn.Streams[0].Mixed.ComponentMolarFraction[j].ValueInSI;
                    _trays[i].yI[j].ValueInSI = VIn.Streams[0].Mixed.ComponentMolarFraction[j].ValueInSI;

                }
                _trays[i].TI.ValueInSI = 0.5 * (_trays[i].TL.ValueInSI + _trays[i].TV.ValueInSI);
            }

            for (int i = NumberOfElements - 1; i >= 0; i--)
            {
                if (i < NumberOfElements - 1)
                    _trays[i].p.ValueInSI = _trays[i + 1].p.ValueInSI - _trays[i + 1].DP.ValueInSI;
                else
                    _trays[i].p.ValueInSI = VIn.Streams[0].Mixed.Pressure.ValueInSI;
            }

            VOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[0].TV.ValueInSI;
            VOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[0].p.ValueInSI - _trays[0].DP.ValueInSI;

            LOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[NumberOfElements - 1].TL.ValueInSI;
            LOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[NumberOfElements - 1].p.ValueInSI;

            for (int j = 0; j < System.Components.Count; j++)
            {
                VOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[0].V.ValueInSI * _trays[0].y[j].ValueInSI;
                LOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[NumberOfElements - 1].L.ValueInSI * _trays[NumberOfElements - 1].x[j].ValueInSI;
            }

            InitOutlets();
        }

        void InitOutlets()
        {
            int NC = System.Components.Count;
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");

            VOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[0].TV.ValueInSI;
            VOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[0].p.ValueInSI - _trays[0].DP.ValueInSI;

            LOut.Streams[0].Mixed.Temperature.ValueInSI = _trays[NumberOfElements - 1].TL.ValueInSI;
            LOut.Streams[0].Mixed.Pressure.ValueInSI = _trays[NumberOfElements - 1].p.ValueInSI;

            for (int j = 0; j < System.Components.Count; j++)
            {
                VOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[0].V.ValueInSI * _trays[0].y[j].ValueInSI;
                LOut.Streams[0].Mixed.ComponentMolarflow[j].ValueInSI = _trays[NumberOfElements - 1].L.ValueInSI * _trays[NumberOfElements - 1].x[j].ValueInSI;
            }
            var flash = new FlashRoutines(new Numerics.Solvers.Newton());

            flash.CalculateTP(VOut.Streams[0]);
            flash.CalculateTP(LOut.Streams[0]);

        }

        public override ProcessUnit Initialize()
        {

            int NC = System.Components.Count;
            var VIn = FindMaterialPort("VIn");
            var LIn = FindMaterialPort("LIn");
            var VOut = FindMaterialPort("VOut");
            var LOut = FindMaterialPort("LOut");

            if (LIn.IsConnected == false)
                throw new InvalidOperationException("LIn must be connected");
            if (VIn.IsConnected == false)
                throw new InvalidOperationException("VIn must be connected");
            if (LOut.IsConnected == false)
                throw new InvalidOperationException("LOut must be connected");
            if (VOut.IsConnected == false)
                throw new InvalidOperationException("VOut must be connected");

            InitAbsorber();
            return this;


        }
        #endregion
    }
}
