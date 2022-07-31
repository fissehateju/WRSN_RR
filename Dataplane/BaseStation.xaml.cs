using RR.Computations;
using RR.Dataplane.PacketRouter;
using RR.Intilization;
using RR.Comuting.Routing;
using RR.Models.Charging;
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
using RR.Dataplane.NOS;

namespace RR.Dataplane
{
    /// <summary>
    /// Interaction logic for BaseStation.xaml
    /// </summary>
    /// 

    public partial class BaseStation : UserControl
    {
        public DispatcherTimer timer_checkingMC= new DispatcherTimer();
        public DispatcherTimer QueuTimer = new DispatcherTimer();
        public int ID { get; set; }
        public Queue<Packet> Arriving_reqPackets = new Queue<Packet>();
        public List<Packet> reqPackets_Unsorted = new List<Packet>();
        public Queue<Packet> SortedreqPackets = new Queue<Packet>();

        //public List<int> firstKreq = new List<int>();
        public Charger charger { get; set; }
        public BaseStation()
        {
            InitializeComponent();
            int id = 1;
            //lbl_BaseStation_id.Text = id.ToString();
            ID = id;                  
            Width = 20;
            Height = 30;
            setPosition();

            //QueuTimer.Interval = PublicParamerters.ChargingQueueTime;
            //QueuTimer.Tick += RechargingScheduling_Tick;
        }

        /// <summary>
        /// Real postion of object.
        /// </summary>
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

        private int MC_breakTime = 5;
        private int breakTime
        {
            get
            {
                return MC_breakTime;
            }
            set
            {
                MC_breakTime = value;
            }
        }

        public void setPosition ()
        {
            //Point pt = PublicParamerters.MainWindow.myNetWork[1].CenterLocation;
            Position = new Point(PublicParamerters.MainWindow.Canvas_SensingFeild.ActualWidth / 2 - Width, PublicParamerters.MainWindow.Canvas_SensingFeild.ActualHeight / 2 - Height);
        }

        public void SaveToQueue(Packet packet)
        {            

            Arriving_reqPackets.Enqueue(packet);
            PublicParamerters.TotalWaitingTimeRechargeQueue += 1; // total;
            packet.Recharg_WaitingTimes += 1;

            QueuTimer.Interval = TimeSpan.FromSeconds(5);
            QueuTimer.Tick += RechargingScheduling_Tick;
            QueuTimer.Start();

        }

        /// <summary>
        ///  add your schedule here before forwarding to the charger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RechargingScheduling_Tick(Object sender, EventArgs e)
        {
            
            if (Arriving_reqPackets.Count < 5)
            {
                QueuTimer.Stop();
            }
            else
            {
                forwardingTasks(); 
            }
        }
        
        
        private void forwardingTasks()
        {
            timer_checkingMC.Interval = TimeSpan.FromSeconds(breakTime);
            timer_checkingMC.Start();
            timer_checkingMC.Tick += timer_tick_checkingMC;
           
        }

        private void timer_tick_checkingMC(Object sender, EventArgs e)
        {
            //
            charger = PublicParamerters.MC;
            Queue<Packet> packets = new Queue<Packet>();
            QueuTimer.Stop();
            timer_checkingMC.Stop();

            while (packets.Count < 5)
            {
                if (Arriving_reqPackets.Count == 0)
                {
                    break;
                }
                packets.Enqueue(Arriving_reqPackets.Dequeue());
            }
                       
            if (packets.Count > 0 && charger.isFree)
            {
                charger.Bstation = this;
                charger.initialize(packets);
            }
            
        }

       

    }
}
