using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.ThermodynamicModels
{
    public enum ReactionType { CONV, EQLA, EQVM, EQLM };

    public class StoichiometryPair
    {
        MolecularComponent _component;
        double _stoichiometricFactor;
        int _index = -1;

        public MolecularComponent Component
        {
            get
            {
                return _component;
            }

            set
            {
                _component = value;
            }
        }

        public double StoichiometricFactor
        {
            get
            {
                return _stoichiometricFactor;
            }

            set
            {
                _stoichiometricFactor = value;
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }

            set
            {
                _index = value;
            }
        }

        public StoichiometryPair(int index, MolecularComponent component, double factor)
        {
            Index = index;
            Component = component;
            StoichiometricFactor = factor;
        }
    }
    public class Reaction
    {
        ReactionType _type = ReactionType.CONV;
        List<StoichiometryPair> _stoichiometry= new List<StoichiometryPair>();
        List<double> _coefficients = new List<double>();
        double _reactionEnthalpy = 0.0;
        public List<StoichiometryPair> Stoichiometry
        {
            get
            {
                return _stoichiometry;
            }

            set
            {
                _stoichiometry = value;
            }
        }

        public ReactionType Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public List<double> Coefficients
        {
            get
            {
                return _coefficients;
            }

            set
            {
                _coefficients = value;
            }
        }

        public double ReactionEnthalpy
        {
            get
            {
                return _reactionEnthalpy;
            }

            set
            {
                _reactionEnthalpy = value;
            }
        }

      
    }

    public class Chemistry
    {
        string _label;
        List<Reaction> _reactions = new List<Reaction>();

        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                _label = value;
            }
        }

        public List<Reaction> Reactions
        {
            get
            {
                return _reactions;
            }

            set
            {
                _reactions = value;
            }
        }
    }

}
