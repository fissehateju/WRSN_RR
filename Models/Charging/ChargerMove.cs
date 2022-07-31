using RR.Intilization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using RR.Dataplane;
using RR.Dataplane.PacketRouter;
using RR.Comuting.Routing;
using RR.Models.Charging;
using RR.Properties;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using RR.Dataplane.NOS;
using static RR.Computations.RandomvariableStream;

namespace RR.Models.Charging
{
    internal class ChargerMove
    {
        private UserControl ObjectToMove; // the object to be moved
        private DispatcherTimer SelectDistinationLocation = new DispatcherTimer();
        private DispatcherTimer MovmentScheduler = new DispatcherTimer();
        private DispatcherTimer timer_Recharging = new DispatcherTimer();
        public Packet Request;

        private Charger charger { get; set; }
        /// <summary>
        /// get the position of the object
        /// </summary>
        public Point ObjectPosition
        {
            get
            {
                double x = ObjectToMove.Margin.Left;
                double y = ObjectToMove.Margin.Top;
                Point p = new Point(x, y);
                return p;
            }
            set
            {
                Point p = value;
                ObjectToMove.Margin = new Thickness(p.X, p.Y, 0, 0);
            }
        }

        Point velocityVector;
        Point m_current;
        Point destination;
        double speed;
        double MaxX, MaxY;
        private bool isBound = false;


        /// <summary>
        /// set the object to be moved.
        /// no Bounds of the area to cruise.
        /// </summary>
        /// <param name="_charr"></param>
        /// 
        public ChargerMove(UserControl _charr, double _MaxX, double _MaxY, Charger _charger)
        {
            MaxX = _MaxX;
            MaxY = _MaxY;
            isBound = true;
            ObjectToMove = _charr;
            charger = _charger;
        }

        /// <summary>
        /// start moving
        /// </summary>
        public void StartMove(Packet req)
        {
            Request = req;
            SelectDistinationLocation.Tick += SelectDistinationLocation_Tick;
            SelectDistinationLocation.Start();
            MovmentScheduler.Tick += Scheduler_Tick;
        }

        /// <summary>
        /// stop moving
        /// </summary>
        public void StopMoving()
        {
            SelectDistinationLocation.Stop();
            MovmentScheduler.Stop();

            if (Request != null)
            {
                NodeCharging();
            }
            else
            {
                charger.isFree = true;
                charger.ResidualEnergy = PublicParamerters.BatteryIntialEnergyForMC;
            }

        }


        public void NodeCharging()
        {
            RechargingModel recharge = new RechargingModel(charger, Request);
            recharge.startRecharing();
        }

        private void Scheduler_Tick(object sender, EventArgs e)
        {
            ScheduleMobility();
        }

        private void SetTravelDelay(TimeSpan timeSpan)
        {
            SelectDistinationLocation.Interval = timeSpan;
            MovmentScheduler.Start();
        }

        private void SelectDistinationLocation_Tick(object sender, EventArgs e)
        {
            BeginWalk();
        }

        /// <summary>
        /// A random variable used to pick the speed of a random waypoint model.
        /// ns3::UniformRandomVariable[Min=0.3|Max=0.7]"
        /// ns3: m_speed
        /// </summary>
        public double Speed
        {
            get
            {
                double speed = UniformRandomVariable.GetDoubleValue(0.3, 0.7);
                return speed;
            }
        }      

    
        /// <summary>
        /// get the 
        /// </summary>
        public void BeginWalk()
        {

            m_current = ObjectPosition;
            if (Request != null)
            {
                destination = Request.Source.CenterLocation;
                speed = 5;
            }
            else
            {
                destination = new Point(PublicParamerters.NetworkSquareSideLength / 2, PublicParamerters.NetworkSquareSideLength / 2);
                speed = 7;
            }

            //speed = 2; //Speed; // random distrubiton
            double dx = destination.X - m_current.X;
            double dy = destination.Y - m_current.Y;
            double k = speed / Math.Sqrt((dx * dx) + (dy * dy));//

            if (!double.IsInfinity(k))
            {

                velocityVector = new Point(k * dx, k * dy); // speed +direction
                TimeSpan travelDelay = TimeSpan.FromSeconds(Operations.DistanceBetweenTwoPoints(m_current, destination) / speed);
                SetTravelDelay(travelDelay); // Timer.
            }



        }

        public void ScheduleMobility()
        {

            MovmentScheduler.Interval = TimeSpan.FromSeconds(1); //TimeSpan.FromSeconds(speed); it was 1
            m_current = ObjectPosition;
            double x = (m_current.X + velocityVector.X);
            double y = (m_current.Y + velocityVector.Y);

            Point moveto = new Point(x, y);


            ObjectPosition = moveto;
            ObjectToMove.Width += 0.00001; // just to trigger an event. dont remove this line.


            // Operations.DrawLine(PublicParamerters.MainWindow.Canvas_SensingFeild, m_current, moveto);

            if (Operations.DistanceBetweenTwoPoints(ObjectPosition, destination) < 5)
            {
                ObjectPosition = destination;
                StopMoving();

            }

        }
    }
}
