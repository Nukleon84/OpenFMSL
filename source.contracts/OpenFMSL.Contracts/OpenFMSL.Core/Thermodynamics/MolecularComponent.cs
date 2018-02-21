using OpenFMSL.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public class MolecularComponent
    {
        string _name;
        string casNo;
        string _identifier;
        List<Variable> _constants = new List<Variable>();
        List<PropertyFunction> _functions = new List<PropertyFunction>();
        
        /// <summary>
        /// Systematic name of the component
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }
        /// <summary>
        /// CAS-registration number of the component
        /// </summary>
        public string CasNumber
        {
            get
            {
                return casNo;
            }

            set
            {
                casNo = value;
            }
        }

        /// <summary>
        /// Abbreviated name of the component
        /// </summary>
        public string ID
        {
            get
            {
                return _identifier;
            }

            set
            {
                _identifier = value;
            }
        }
        /// <summary>
        /// List of all pure component constants for this molecular component
        /// </summary>
        public List<Variable> Constants
        {
            get
            {
                return _constants;
            }

            set
            {
                _constants = value;
            }
        }

        /// <summary>
        /// List of all pure component temperature-dependent functions for this molecular component
        /// </summary>
        public List<PropertyFunction> Functions
        {
            get
            {
                return _functions;
            }

            set
            {
                _functions = value;
            }
        }

        /// <summary>
        /// Retrieve the constant for a given constant ID 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Variable GetConstant(ConstantProperties id)
        {
            var constant = Constants.FirstOrDefault(c => c.Name == id.ToString());

            if (constant != null)
                return constant;
            else
                throw new ArgumentException("Constant ID not found");
        }

        /// <summary>
        /// Retrieve the temperature dependent function for a given property id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PropertyFunction GetFunction(EvaluatedProperties id)
        {
            var function = Functions.FirstOrDefault(c => c.Property == id);

            if (function != null)
                return function;
            else
                throw new ArgumentException("Property function ID not found");
        }
    }
}
