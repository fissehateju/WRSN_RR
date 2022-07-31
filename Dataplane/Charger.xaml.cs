using RR.Computations;
using RR.Dataplane.PacketRouter;
using RR.Intilization;
using RR.Comuting.Routing;
using RR.Models.Mobility;
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
using System.Threading;
using RR.Dataplane.NOS;

namespace RR.Dataplane
{
    /// <summary>
    /// Interaction logic for Charger.xaml
    /// </summary>
    public partial class Charger : UserControl
    {
        private DispatcherTimer timer_move = new DispatcherTimer();
        private DispatcherTimer timer_Recharging = new DispatcherTimer();

        public BaseStation Bstation { get; set; }
        private Queue<Packet> Requests;
        public List<Packet> packets_in_order = new List<Packet>();
        private ChargerMove Nextpoint;
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

        public void initialize(Queue<Packet> Pack)
        {
            Requests = Pack;
            isFree = false;

            startTour();
        }
        /// <summary>
        /// Mobile charger tour starts here.
        /// </summary>
        /// <param name="task"></param>
        public void startTour()
        {            
            Nextpoint = new ChargerMove(this, PublicParamerters.NetworkSquareSideLength, PublicParamerters.NetworkSquareSideLength, this);

            depotPoint = Position;
            if (Requests.Count > 0)
            {
                Nextpoint.StartMove(Requests.Dequeue());
            }
            else
            {
                Nextpoint.StartMove(null);
            }

            

        }       

        
        public void setPosition()
        {
            //Point pt = PublicParamerters.MainWindow.myNetWork[1].CenterLocation;
            Position = new Point(PublicParamerters.MainWindow.Canvas_SensingFeild.ActualWidth / 2, PublicParamerters.MainWindow.Canvas_SensingFeild.ActualHeight / 2);
        }
    }
}
