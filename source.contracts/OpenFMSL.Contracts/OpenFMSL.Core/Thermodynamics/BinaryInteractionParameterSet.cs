using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Thermodynamics
{
    public class NRTL : BinaryInteractionParameterSet
    {
        public NRTL(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "NRTL";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
            Matrices.Add("E", new double[NC, NC]);
            Matrices.Add("F", new double[NC, NC]);

        }
    }

    public class ModifiedUNIQUAC : BinaryInteractionParameterSet
    {
        public ModifiedUNIQUAC(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "MODUNIQUAC";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
            Matrices.Add("E", new double[NC, NC]);
            Matrices.Add("F", new double[NC, NC]);

        }
    }
    public class UNIQUAC : BinaryInteractionParameterSet
    {
        public UNIQUAC(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "UNIQUAC";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
            Matrices.Add("E", new double[NC, NC]);
            Matrices.Add("F", new double[NC, NC]);

        }
    }

    public class HENRY : BinaryInteractionParameterSet
    {
        public HENRY(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "HENRY";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);           

        }
    }
    public class WILSON : BinaryInteractionParameterSet
    {
        public WILSON(ThermodynamicSystem system)
        {
            _system = system;
            NC = _system.Components.Count;

            Name = "WILSON";
            Matrices.Add("A", new double[NC, NC]);
            Matrices.Add("B", new double[NC, NC]);
            Matrices.Add("C", new double[NC, NC]);
            Matrices.Add("D", new double[NC, NC]);
         
        }
    }

    public class BinaryInteractionParameterSet
    {
        protected int NC;
        protected ThermodynamicSystem _system;
        string _name;
        Dictionary<string, double[,]> _matrices = new Dictionary<string, double[,]>();


        public void SetParam(string matrix, int i, int j, double value)
        {
            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
                Matrices[matrix][i, j] = value;
        }
        public void SetParam(string matrix, MolecularComponent c1, MolecularComponent c2, double value)
        {
            var i = _system.Components.IndexOf(c1);
            var j = _system.Components.IndexOf(c2);

            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
                Matrices[matrix][i, j] = value;
        }
        public double GetParam(string matrix, MolecularComponent c1, MolecularComponent c2)
        {
            var i = _system.Components.IndexOf(c1);
            var j = _system.Components.IndexOf(c2);

            if (i >= 0 && j >= 0 && Matrices.ContainsKey(matrix))
               return Matrices[matrix][i, j];
            return 0;
        }

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

        public Dictionary<string, double[,]> Matrices
        {
            get
            {
                return _matrices;
            }

            set
            {
                _matrices = value;
            }
        }
    }
}
