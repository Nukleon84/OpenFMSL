using OpenFMSL.Contracts.Infrastructure.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFMSL.Core.Thermodynamics;
using System.Xml.Linq;
using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.UnitsOfMeasure;
using System.IO;

namespace ChemSepDBAdapter
{
    public class Adapter : IPureComponentPropertyDatabase
    {
        XElement db = null;
        Action<string> _callback;
        List<NRTLBIPSet> nrtldb = null;



        public void FillBIPs(ThermodynamicSystem system)
        {
            switch (system.EquilibriumMethod.Activity)
            {
                case ActivityMethod.NRTL:
                case ActivityMethod.NRTLRP:
                    FillNRTL(system);
                    break;
            }
        }



        public void SetLogCallback(Action<string> callback)
        {
            _callback = callback;
        }

        public void ListComponents(string pattern)
        {
            if (db == null)
                db = XElement.Load(".\\data\\chemsep1.xml");

            pattern = pattern.ToLowerInvariant();
            var comps = from nm in db.Elements("compound")
                        where ((string)nm.Element("CompoundID").Attribute("value")).ToLowerInvariant().Contains(pattern)
                        select nm;

            foreach (var comp in comps)
            {
                var id = (string)comp.Element("CompoundID").Attribute("value");
                var cas = (string)comp.Element("CAS").Attribute("value");
                Write(String.Format("{0,-30} [{1}]" + Environment.NewLine, id, cas));
            }
        }


        public MolecularComponent FindComponent(string name)
        {
            if (db == null)
                db = XElement.Load(".\\data\\chemsep1.xml");

            var comps = from nm in db.Elements("compound")
                        where (string)nm.Element("CompoundID").Attribute("value") == name
                        select nm;


            if (comps.Count() == 1)
            {
                var c = comps.First();
                var component = new MolecularComponent()
                {
                    Name = name,
                    ID = name,
                    CasNumber = (string)c.Element("CAS").Attribute("value")
                };

                component.Constants.Add(GetConstant(c.Element("MolecularWeight"), ConstantProperties.MolarWeight));
                component.Constants.Add(GetConstant(c.Element("CriticalTemperature"), ConstantProperties.CriticalTemperature));
                component.Constants.Add(GetConstant(c.Element("CriticalPressure"), ConstantProperties.CriticalPressure));
                component.Constants.Add(GetConstant(c.Element("AcentricityFactor"), ConstantProperties.AcentricFactor));

                component.Functions.Add(GetFunction(c.Element("VaporPressure"), EvaluatedProperties.VaporPressure));
                component.Functions.Add(GetFunction(c.Element("HeatOfVaporization"), EvaluatedProperties.HeatOfVaporization));

                component.Functions.Add(GetFunction(c.Element("LiquidDensity"), EvaluatedProperties.LiquidDensity));
                component.Functions.Add(GetFunction(c.Element("LiquidHeatCapacityCp"), EvaluatedProperties.LiquidHeatCapacity));
                component.Functions.Add(GetFunction(c.Element("LiquidViscosity"), EvaluatedProperties.LiquidViscosity));
                component.Functions.Add(GetFunction(c.Element("LiquidThermalConductivity"), EvaluatedProperties.LiquidHeatConductivity));

                component.Functions.Add(GetFunction(c.Element("RPPHeatCapacityCp"), EvaluatedProperties.IdealGasHeatCapacity));
                component.Functions.Add(GetFunction(c.Element("VaporViscosity"), EvaluatedProperties.VaporViscosity));
                component.Functions.Add(GetFunction(c.Element("VaporThermalConductivity"), EvaluatedProperties.VaporHeatConductivity));

                component.Functions.Add(GetFunction(c.Element("SurfaceTension"), EvaluatedProperties.SurfaceTension));

                //CPIG, HVAP, CL, DENL, VP
                return component;

            }
            else
            {
                throw new ArgumentException("No compound found for name " + name);
            }


        }


        void Write(string msg)
        {
            if (_callback != null)
                _callback(msg);
        }


        Variable GetConstant(XElement xElement, ConstantProperties prop)
        {
            if (xElement == null)
                throw new ArgumentException("xElement was null");
            var uom = ParseUOM((string)xElement.Attribute("units"));
            var constant = new Variable(prop.ToString(), (double)xElement.Attribute("value"), uom);
            return constant;
        }



        Unit ParseUOM(string uomstring)
        {
            switch (uomstring)
            {
                case "K":
                    return SI.K;
                case "Pa":
                    return SI.Pa;
                case "kg/kmol":
                    return SI.kg / SI.kmol;
                case "J/kmol":
                    return SI.J / SI.kmol;
                case "J/kmol/K":
                    return SI.J / (SI.kmol * SI.K);
                case "kmol/m3":
                    return SI.kmol / SI.cum;
                case "N/m":
                    return SI.N / SI.m;
                case "Pa.s":
                    return SI.Pa * SI.s;
                case "W/m/K":
                    return SI.W / SI.m / SI.K;
                default:
                    return SI.nil;
            }
        }

        Variable GetVariable(XElement xElement, string name)
        {
            if (xElement == null)
                throw new ArgumentException("xElement was null");
            var uom = ParseUOM((string)xElement.Attribute("units"));
            var vari = new Variable(name, (double)xElement.Attribute("value"), uom);
            return vari;
        }

        FunctionType GetFunctionType(int eqno)
        {
            switch (eqno)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 100:
                    return FunctionType.Polynomial;
                case 105:
                    return FunctionType.Rackett;
                case 101:
                    return FunctionType.Chemsep101;
                case 102:
                    return FunctionType.Chemsep102;
                case 106:
                    return FunctionType.Chemsep106;
                case 16:
                    return FunctionType.Chemsep16;

                default:
                    return FunctionType.Polynomial;
            }
        }

        PropertyFunction GetFunction(XElement xElement, EvaluatedProperties prop)
        {
            if (xElement == null)
                throw new ArgumentException("xElement was null");
            var uom = ParseUOM((string)xElement.Attribute("units"));
            var tfunc = new PropertyFunction()
            {
                Type = GetFunctionType((int)xElement.Element("eqno").Attribute("value")),
                Property = prop,
                MinimumX = GetVariable(xElement.Element("Tmin"), "Tmin"),
                MaximumX = GetVariable(xElement.Element("Tmax"), "Tmax"),
                XUnit = GetVariable(xElement.Element("Tmax"), "Tmax").InternalUnit,
                YUnit = uom
            };

            var a = xElement.Element("A");
            if (a != null)
                tfunc.Coefficients.Add(new Variable("A", (double)a.Attribute("value")) { IsConstant = true, IsFixed = true });
            var b = xElement.Element("B");
            if (b != null)
                tfunc.Coefficients.Add(new Variable("B", (double)b.Attribute("value")) { IsConstant = true, IsFixed = true });
            var c = xElement.Element("C");
            if (c != null)
                tfunc.Coefficients.Add(new Variable("C", (double)c.Attribute("value")) { IsConstant = true, IsFixed = true });
            var d = xElement.Element("D");
            if (d != null)
                tfunc.Coefficients.Add(new Variable("D", (double)d.Attribute("value")) { IsConstant = true, IsFixed = true });
            var e = xElement.Element("E");
            if (e != null)
                tfunc.Coefficients.Add(new Variable("E", (double)e.Attribute("value")) { IsConstant = true, IsFixed = true });

            return tfunc;
        }

        void LoadNRTLDB()
        {
            var rawdata = File.ReadAllLines(".\\Data\\nrtl.ipd");
            nrtldb = new List<NRTLBIPSet>();

            foreach (var line in rawdata)
            {
                if (!String.IsNullOrEmpty(line) && Char.IsNumber(line[0]))
                {
                    var tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Count() >= 5)
                    {
                        var entry = new NRTLBIPSet();
                        entry.Cas1 = tokens[0];
                        entry.Cas2 = tokens[1];

                        if (tokens[2].StartsWith("."))
                            tokens[2] = "0" + tokens[4];
                        entry.Aij = Double.Parse(tokens[2],System.Globalization.NumberFormatInfo.InvariantInfo );

                        if (tokens[3].StartsWith("."))
                            tokens[3] = "0" + tokens[4];
                        entry.Aji = Double.Parse(tokens[3], System.Globalization.NumberFormatInfo.InvariantInfo);

                        if (tokens[4].StartsWith("."))
                            tokens[4] = "0" + tokens[4];    
                        entry.Alpha = Double.Parse(tokens[4], System.Globalization.NumberFormatInfo.InvariantInfo);
                        nrtldb.Add(entry);
                    }

                }
            }
        }

        void FillNRTL(ThermodynamicSystem system)
        {
            if (!File.Exists(".\\Data\\nrtl.ipd"))
                throw new FileNotFoundException("NRTL parameter database not found. Please install ChemSep Lite and copy the nrtl.ipd file to the Data directory of this program.");

            if (nrtldb == null)
                LoadNRTLDB();

            var nrtlParamSet = new NRTL(system);
            if (system.BinaryParameters.Count(ps => ps.Name == "NRTL") == 0)
                system.BinaryParameters.Add(nrtlParamSet);

            //var Rcal = 1.9872;

            for (int i = 0; i < system.Components.Count; i++)
            {

                var c1 = system.Components[i];
                for (int j = i; j < system.Components.Count; j++)
                {
                    var c2 = system.Components[j];
                    
                    var entry = (from e in nrtldb
                                where (e.Cas1 == c1.CasNumber && e.Cas2 == c2.CasNumber) || (e.Cas1 == c2.CasNumber && e.Cas2 == c1.CasNumber)
                                select e).FirstOrDefault();
                    
                    if(entry!=null)
                    {
                        nrtlParamSet.SetParam("A", c1, c2, entry.Aij);
                        nrtlParamSet.SetParam("A", c2, c1, entry.Aji);

                        nrtlParamSet.SetParam("C", c1, c2, entry.Alpha);
                        nrtlParamSet.SetParam("C", c2, c1, entry.Alpha);
                        Write("Found NRTL parameter set for " + c1.ID + " / " + c2.ID+Environment.NewLine);
                    }
                }
            }

        }

    }
}
