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

namespace ChemSepDBAdapter
{
    public class Adapter : IPureComponentPropertyDatabase
    {

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
            switch(eqno)
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
                MinimumX = GetVariable(xElement.Element("Tmin"),"Tmin"),
                MaximumX = GetVariable(xElement.Element("Tmax"), "Tmax"),
                XUnit = GetVariable(xElement.Element("Tmax"), "Tmax").InternalUnit,
                YUnit = uom
            };

            var a = xElement.Element("A");
            if (a != null)
                tfunc.Coefficients.Add(new Variable("A", (double)a.Attribute("value")) { IsConstant = true, IsFixed = true });
            var b = xElement.Element("B");
            if (b!= null)
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




        /*
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
            */



        /* newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("MolarWeight", 0.018, SI.kg / SI.mol));
             newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("CriticalTemperature", 600, SI.K));
             newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("CriticalPressure", 221e5, SI.Pa));
             newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("CriticalDensity", 0.1, SI.kmol / SI.cum));
             newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("HeatOfFormation", 0, SI.J / SI.kmol));
             newComp.Constants.Add(new OpenFMSL.Core.Expressions.Variable("AcentricFactor", 0.3, SI.nil));*/
        XElement db = null;

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
                component.Functions.Add(GetFunction(c.Element("LiquidDensity"), EvaluatedProperties.LiquidDensity));
                component.Functions.Add(GetFunction(c.Element("LiquidHeatCapacityCp"), EvaluatedProperties.LiquidHeatCapacity));
                component.Functions.Add(GetFunction(c.Element("RPPHeatCapacityCp"), EvaluatedProperties.IdealGasHeatCapacity));
                component.Functions.Add(GetFunction(c.Element("HeatOfVaporization"), EvaluatedProperties.HeatOfVaporization));

                //CPIG, HVAP, CL, DENL, VP
                return component;

            }
            else
            {
                throw new ArgumentException("No compound found for name " + name);
            }


        }
    }
}
