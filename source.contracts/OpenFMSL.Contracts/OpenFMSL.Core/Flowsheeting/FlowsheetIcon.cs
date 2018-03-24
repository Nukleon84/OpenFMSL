using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Flowsheeting
{
    public enum IconTypes
    {
        Block,
        Vessel,
        Column,
        Stream,
        Mixer,
        Splitter,
        TwoPhaseFlash,
        ThreePhaseFlash,
        Decanter,
        Heater,
        HeatExchanger,
        CSTR,
        PFR,
        Pump,
        Compressor,
        Thermodynamics,
        Script,
        Feed,
        Product,
        Text,
        Image,
        Spreadsheet,
        Button,
        Valve,
        ColumnSection,
        FallingFilm,
        Breaker,
        Variable,
        Equation,
        PIAdapter,
        None,
        UserModel,
        ComponentSplitter
    }


    public class FlowsheetIcon
    {
        double _x=100;
        double _y=100;
        IconTypes _iconType=IconTypes.Block;
        string _borderColor = "DimGray";
        string _fillColor = "White";

        public double X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }

        public IconTypes IconType
        {
            get
            {
                return _iconType;
            }

            set
            {
                _iconType = value;
            }
        }

        public string FillColor
        {
            get
            {
                return _fillColor;
            }

            set
            {
                _fillColor = value;
            }
        }

        public string BorderColor
        {
            get
            {
                return _borderColor;
            }

            set
            {
                _borderColor = value;
            }
        }
    }
}
