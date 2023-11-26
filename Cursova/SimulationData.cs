using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursova
{
    public class SimulationData
    {
        public int Day { get; set; }
        public double Susceptible { get; set; }
        public double Infectious { get; set; }
        public double Recovered { get; set; }
    }
}
