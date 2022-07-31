using RR.Computations;
using RR.Dataplane;
using RR.Intilization;
using RR.Comuting.Routing;
using RR.Models.Mobility;
using RR.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using RR.Dataplane.NOS;

namespace RR.Models.Charging
{
    internal class RechargingModel
    {
        private DispatcherTimer timer_Recharging = new DispatcherTimer(); // Making this static will restric the values inside the fuction called by Tick
        private Packet tobeCharged;
        public Charger charger;
        public RechargingModel(Charger charg, Packet pack)
        {
            charger = charg;
            tobeCharged = pack;
        }

        public void startRecharing()
        {
           
            timer_Recharging.Interval = TimeSpan.FromSeconds(3);
            timer_Recharging.Start();
            timer_Recharging.Tick += Recharging;
        }
        private void Recharging(Object sender, EventArgs e)
        {
            timer_Recharging.Stop();
            tobeCharged.Source.ResidualEnergy = PublicParamerters.BatteryIntialEnergy;
            PublicParamerters.requestedList.Remove(tobeCharged.Source);

            charger.startTour();
        }

    }
}
