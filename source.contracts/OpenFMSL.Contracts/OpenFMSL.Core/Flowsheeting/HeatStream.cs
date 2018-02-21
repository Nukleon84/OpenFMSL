using OpenFMSL.Core.Expressions;
using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Flowsheeting
{
    public class HeatStream : Stream
    {
        Variable q;

        public HeatStream(string name, Thermodynamics.ThermodynamicSystem system) : base(name, system)
        {
            Class = "HeatStream";
            Q = system.VariableFactory.CreateVariable("Q", "Heat Duty", PhysicalDimension.HeatFlow);
            AddVariable(Q);
        }

        public Variable Q
        {
            get
            {
                return q;
            }

            set
            {
                q = value;
            }
        }

        public override void FillEquationSystem(EquationSystem problem)
        {
            base.FillEquationSystem(problem);
        }
    }

}
