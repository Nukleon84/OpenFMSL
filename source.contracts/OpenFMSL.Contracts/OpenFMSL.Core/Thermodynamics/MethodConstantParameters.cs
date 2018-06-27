using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public enum MethodTypes { RKS, Uniquac, ModUniquac};

    public class MethodConstantParameters
    {
        MethodTypes _method;
        Dictionary<string, Variable> _parameters = new Dictionary<string, Variable>();

        public MethodTypes Method
        {
            get
            {
                return _method;
            }

            set
            {
                _method = value;
            }
        }

        public Dictionary<string, Variable> Parameters
        {
            get
            {
                return _parameters;
            }

            set
            {
                _parameters = value;
            }
        }
    }
}
