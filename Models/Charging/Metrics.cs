using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Models.Charging
{
    public class Metrics
    {
        public double remainEnergy;
        public double TrasmitRate;
        public double distFromMC;
        public double distFromAvgLocation;
        
        public Metrics(double remainEnergy, double trasmitRate, double distFromMC, double distFromAvgLocation)
        {
            this.remainEnergy = remainEnergy;
            this.TrasmitRate = trasmitRate;
            this.distFromMC = distFromMC;
            this.distFromAvgLocation = distFromAvgLocation;
        }

    }
}
