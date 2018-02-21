using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Numerics
{
    public class JacobianElement
    {
        int _equationIndex = -1;
        int _variableIndex = -1;
        double _value = Double.NaN;

        public int EquationIndex
        {
            get
            {
                return _equationIndex;
            }

            set
            {
                _equationIndex = value;
            }
        }

        public int VariableIndex
        {
            get
            {
                return _variableIndex;
            }

            set
            {
                _variableIndex = value;
            }
        }

        public double Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }
}
