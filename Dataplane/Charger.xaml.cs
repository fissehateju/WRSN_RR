using RR.Computations;
using RR.Dataplane.PacketRouter;
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

namespace RR.Dataplane
{
    /// <summary>
    /// Interaction logic for Charger.xaml
    /// </summary>
    public partial class Charger : UserControl
    {
        private static DispatcherTimer timer_move = new DispatcherTimer();
        private static DispatcherTimer timer_Recharging = new DispatcherTimer();
        private static DispatcherTimer timer_checkSinkGoingOut = new DispatcherTimer();
        private static DispatcherTimer timer_getNewDirection = new DispatcherTimer();
        public BaseStation Bstation { get; set; }
        public Queue<Packet> Requests;
        public List<Packet> packets_in_order = new List<Packet>();
        public int ID { get; set; }
        public bool isFree { get; set; }
        public bool isgoingBack { get; set; }
        public double ResidualEnergy { get; set; }

        //
        private Point depotPoint { get; set; }
        private Point next_position { get; set; }
        private Point destination { get; set; }
        public Point Position
        {
            get
            {
                double x = Margin.Left;
                double y = Margin.Top;
                Point p = new Point(x, y);
                return p;
            }
            set
            {
                Point p = value;
                Margin = new Thickness(p.X, p.Y, 0, 0);
            }
        }

        public Charger()
        {
            InitializeComponent();
            int id = 1;
            //lbl_charger_id.Text = id.ToString();
            ID = id;
            isFree = true;
            ResidualEnergy = 100;
            setPosition();

            //Bstation = PublicParamerters.BS;

        }
        /// <summary>
        /// Mobile charger tour starts here.
        /// </summary>
        /// <param name="task"></param>
        public void startTour(Queue<Packet> Pack)
        {
            Requests = Pack;
            isFree = false;
            isgoingBack = false;
            depotPoint = Position;
            if (Requests.Count > 0 && Requests != null)
            {
                destination = Requests.Peek().Source.CenterLocation;
            }
            else
            {
                destination = Bstation.Position;
                isgoingBack = true;
                Requests.Clear();
            }

            timer_move.Interval = TimeSpan.FromSeconds(0.1);
            timer_move.Start();
            timer_move.Tick += MC_moving;

        }
        private double Rechargingtime = 3;
        private int count = 0;
        private void MC_moving(Object sender, EventArgs e)
        {

            if (Operations.DistanceBetweenTwoPoints(Position, destination) < 5)
            {

                if (destination != Bstation.Position)
                {
                    timer_move.Stop();

                    // start recharging
                    //Thread.Sleep(2000);

                    timer_Recharging.Interval = TimeSpan.FromSeconds(Rechargingtime);
                    timer_Recharging.Start();
                    timer_Recharging.Tick += Recharging;

                }
                else
                {
                    PublicParamerters.BatteryIntialEnergyForMC = 500;
                    endTour();
                    return;
                }

                //timer_move.Stop();
                //startTour(myTask);
            }
            next_position = get_NextPosition();
            Position = next_position;
        }

        private void Recharging(Object sender, EventArgs e)
        {
            count++;
            if (count >= Rechargingtime && Requests.Count > 0 && Requests != null)
            {
                timer_Recharging.Stop();
                count = 0;

                Requests.Peek().Source.ResidualEnergy = PublicParamerters.BatteryIntialEnergy;
                PublicParamerters.requestedList.Remove(Requests.Dequeue().Source);

                startTour(Requests); // go to the next node
            }
            else if (Requests.Count > 0 && Requests != null)
            {
                System.Console.WriteLine("Still recharging sensor {0}", Requests.Peek().Source.ID);
            }

        }
        private Point get_NextPosition()
        {
            double EDs = Operations.DistanceBetweenTwoPoints(Position, destination);
            if (EDs == 0) EDs = 1;
            double Next_X = Position.X + (destination.X - Position.X) / EDs;
            double Next_Y = Position.Y + (destination.Y - Position.Y) / EDs;
            Point nextPoint = new Point(Next_X, Next_Y);

            return nextPoint;
        }

        private void endTour()
        {
            if (Operations.DistanceBetweenTwoPoints(Position, Bstation.Position) < PublicParamerters.CommunicationRangeRadius / 10)
            {
                ResidualEnergy = 100;
                isFree = true;
            }
            timer_move.Stop();
        }

        public void setPosition()
        {
            //Point pt = PublicParamerters.MainWindow.myNetWork[1].CenterLocation;
            Position = new Point(PublicParamerters.MainWindow.Canvas_SensingFeild.ActualWidth / 2, PublicParamerters.MainWindow.Canvas_SensingFeild.ActualHeight / 2);
        }
    }
}
