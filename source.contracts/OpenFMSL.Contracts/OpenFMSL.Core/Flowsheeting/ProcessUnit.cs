using OpenFMSL.Core.Numerics;
using OpenFMSL.Core.Numerics.Solvers;
using OpenFMSL.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFMSL.Core.Flowsheeting
{
    public abstract class ProcessUnit : FlowsheetObject
    {
        List<Port<MaterialStream>> _materialPorts = new List<Port<MaterialStream>>();
        List<Port<HeatStream>> _heatPorts = new List<Port<HeatStream>>();

        public List<Port<MaterialStream>> MaterialPorts
        {
            get
            {
                return _materialPorts;
            }

            set
            {
                _materialPorts = value;
            }
        }

        public List<Port<HeatStream>> HeatPorts
        {
            get
            {
                return _heatPorts;
            }

            set
            {
                _heatPorts = value;
            }
        }


        public ProcessUnit(string name, ThermodynamicSystem system) : base(name, system)
        {

        }

        public ProcessUnit Connect(string portName, Stream stream)
        {
            if (stream is MaterialStream)
            {
                var materialPort = FindMaterialPort(portName);
                if (materialPort != null)
                    materialPort.Connect(stream as MaterialStream);
                else
                    throw new InvalidOperationException("Port " + portName + " not found");
            }

            if (stream is HeatStream)
            {
                var heatPort = FindHeatPort(portName);
                if (heatPort != null)
                    heatPort.Connect(stream as HeatStream);
                else
                    throw new InvalidOperationException("Port " + portName + " not found");
            }
            return this;
        }

        public virtual ProcessUnit Initialize()
        {
            return this;
        }
        /// <summary>
        /// Solves the unit together with the output material streams as a single flowsheet. When using this method, the unit has to be specified fully.
        /// </summary>
        public virtual ProcessUnit Solve()
        {
            var decomp = new Decomposer();

            var flowsheet = new Flowsheet(Name);
            flowsheet.AddUnit(this);
            foreach (var stream in MaterialPorts.Where(p => p.Direction == PortDirection.Out && p.IsConnected).Select(p => p.Streams.ToArray()))
            {
                flowsheet.AddMaterialStreams(stream);
            }
            var problem = new EquationSystem();
            flowsheet.FillEquationSystem(problem);
            decomp.Solve(problem);

            return this;

        }


        public Port<MaterialStream> FindMaterialPort(string portName)
        {
            return MaterialPorts.FirstOrDefault(p => p.Name == portName);
        }

        public Port<HeatStream> FindHeatPort(string portName)
        {
            return HeatPorts.FirstOrDefault(p => p.Name == portName);
        }

    }
}
