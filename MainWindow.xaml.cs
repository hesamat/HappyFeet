using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using ViconDataStreamSDK.DotNET;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Windows.Automation.Peers;

namespace HappyFeet
{    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Constants
        /// </summary>               
        private const double CCIT_ROOM_WIDTH = 1.6;
        private const double CCIT_ROOM_LENGTH = 1.2;
        private const double CCIT_ROOM_CENTER_X = 0.6;
        private const double CCIT_ROOM_CENTER_Y = 1.38;

        private const double ILAB_ROOM_WIDTH = 1.8;
        private const double ILAB_ROOM_LENGTH = 1.5;
        private const double ILAB_ROOM_CENTER_X = -0.7;
        private const double ILAB_ROOM_CENTER_Y = -0.5;

        private const double ILAB_LEFT_SHOE_Z_BIAS = 0.082;
        private const double ILAB_RIGHT_SHOE_Z_BIAS = 0.087;

        private const int ILAB_3D_X_OFFSET = 550;
        private const int ILAB_3D_Y_OFFSET = -50;
        private const int CCIT_3D_Y_OFFSET = 900;

        private const int ILAB_TOWARDS_X_DISPLACEMENT = -150;
        private const int ILAB_TOWARDS_Y_DISPLACEMENT = 200;
        private const int ILAB_PARALLEL_X_DISPLACEMENT = 0;
        private const int ILAB_PARALLEL_Y_DISPLACEMENT = 400;

        private const int CCIT_TOWARDS_X_DISPLACEMENT = -650;
        private const int CCIT_TOWARDS_Y_DISPLACEMENT = 500;
        private const int CCIT_PARALLEL_Y_DISPLACEMENT = 400;


        // *********************
        private const int FROM_ILAB_CCIT_TOWARDS_X_DISPLACEMENT = 100;
        private const int FROM_ILAB_CCIT_TOWARDS_Y_DISPLACEMENT = -900;
        private const int FROM_ILAB_CCIT_PARALLEL_X_DISPLACEMENT = 0;
        private const int FROM_ILAB_CCIT_PARALLEL_Y_DISPLACEMENT = 1800;


        private const double ILAB_3D_RIGHT_Y_OFFSET = -40;        
        private const double ILAB_3D_RIGHT_Z_OFFSET = 1;
        private const double ILAB_3D_LEFT_Z_OFFSET = 4.5;

        private const double CCIT_3D_RIGHT_Z_OFFSET = 20;
        private const double CCIT_3D_LEFT_Z_OFFSET = 70;

        private const double SHADOW_Z_OFFSET = 10;

        private const int ILAB_3D_Z_FACTOR = 20;
        private const int CCIT_3D_Z_FACTOR = 20;
        private const int CCIT_3D_X_OFFSET = 550;

        private const int HOMESPACE_CAMERA_Z_OFFSET = 3500;
        private const int CCIT_CAMERA_Z_OFFSET = 4000;
//        private const int HOMESPACE_PLANE_DISTANCES = 1350;
//        private const int CCIT_PLANE_DISTANCES = 1750;
        private const int HOMESPACE_PLANE_DISTANCES = 1350;
        private const int CCIT_PLANE_DISTANCES = 750;

        private const double INITIAL_CCIT_GRID_HEIGHT = -200;
        private const double INITIAL_CCIT_GRID_WIDTH = 2500;
        private const double INITIAL_CCIT_GRID_LENGTH = 2350;

        private const double INITIAL_ILAB_GRID_HEIGHT = -500;
        private const double INITIAL_ILAB_GRID_WIDTH = 2000;
        private const double INITIAL_ILAB_GRID_LENGTH = 2450;       

        private const double CCIT_LEFT_SHOE_Z_BIAS = 0.079;
        private const double CCIT_RIGHT_SHOE_Z_BIAS = 0.084;

        private const double CCIT_X_PIXEL_BIAS = 385;
        private const double CCIT_Y_PIXEL_BIAS = 325;

        private const int LEFT_SHOE_INDEX = 0;
        private const int LEFT_SHOE_YAW_OFFSET = 40;

        private const int MAX_SHOE_PLANE_DEGREE = 90;

        private const int RIGHT_SHOE_INDEX = 1;
        private const int RIGHT_SHOE_YAW_OFFSET = 6;

        private const double MINIMUM_SHADOW_OPACITY = 0.05;
//        private static Quaternion RIGHT_SHOE_CORRECTION = new Quaternion(0.94,0.035,-0.31,0.121);
//        private static Quaternion LEFT_SHOE_CORRECTION = new Quaternion(-0.181,-0.922,-0.084,0.331);

        private const double SHOEPLANE_Z_BIAS = -30;

        private static Quaternion rightInitialInverseQuaternion;
        private static Quaternion leftInitialInverseQuaternion;


        private static double[] gridCenter;

        private static Matrix3D localDancerPlaneTransform;

        /// <summary>
        /// Program options
        /// /// </summary>
        private string HostName = "192.168.137.1:801";
        private string CCITHostName = "localhost:801";
        public static bool isInHomeSpace = false;   //If true the 
        private int objectNum = 2;       //Number of objects being tracked
        private static Model3DGroup remote_rshoe3D;
        private static Model3DGroup remote_lshoe3D;
        private static Model3DGroup local_rshoe3D;
        private static Model3DGroup local_lshoe3D;
        
        /// <summary>
        /// Flags
        /// </summary>
        private bool handshakeRequest = false;  //Raised when handshake is done
        private static bool initialOrientationRight = true;
        private static bool initialOrientationLeft = true;
        private static bool programStarted = false;
        private static bool parallelDancing = true;
        public static bool recordingFinishedAck = false;
        public static bool fileLoaded = false;
        public static bool receivingOtherSidesData = true;

        /// <summary>
        /// Variables
        /// </summary>
        private static System.Drawing.Rectangle screenResolution;
        private static ViconDataStreamSDK.DotNET.Client ViconClient;
        private static NodeClient nodeClient;
        private static Video videoPlayer;
        private System.Windows.Threading.DispatcherTimer socketTimer;
        private System.Windows.Threading.DispatcherTimer animationTimer;        
        private ViconStreamThread viconThread;
        private static DanceFloor danceWindow;
        private string playbackFileIndex;
        



        public class StreamsData
        {
            public static string[] liveData = new String[2];            
            public static string[] networkData = new String[2];            
        }

        class ViconStreamThread
        {
            public Thread th;
            public ViconStreamThread()
            {
                th = new Thread(this.Go);
            }
            void Go()
            {
                while (true)
                {                    
                    // Get a frame
                    while (ViconClient.GetFrame().Result != ViconDataStreamSDK.DotNET.Result.Success)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    // Get the latency
                    //Console.WriteLine("Latency: {0}s", ViconClient.GetLatencyTotal().Total);


                    // Count the number of subjects
                    uint SubjectCount = ViconClient.GetSubjectCount().SubjectCount;
                    for (uint SubjectIndex = 0; SubjectIndex < SubjectCount; ++SubjectIndex)
                    {
                        // Get the subject name
                        string SubjectName = ViconClient.GetSubjectName(SubjectIndex).SubjectName;
                        //Console.WriteLine("    Name: {0}", SubjectName);

                        // Count the number of segments
                        uint SegmentCount = ViconClient.GetSegmentCount(SubjectName).SegmentCount;
                        for (uint SegmentIndex = 0; SegmentIndex < SegmentCount; ++SegmentIndex)
                        {                            
                            // Get the segment name
                            string SegmentName = ViconClient.GetSegmentName(SubjectName, SegmentIndex).SegmentName;
                            if (!SegmentName.Equals("RightShoe", StringComparison.InvariantCultureIgnoreCase) && !SegmentName.Equals("LeftShoe", StringComparison.InvariantCultureIgnoreCase))
                                continue;

                            // Get the global segment translation
                            Output_GetSegmentGlobalTranslation _Output_GetSegmentGlobalTranslation =
                                ViconClient.GetSegmentGlobalTranslation(SubjectName, SegmentName);

                            // Get the global segment rotation in EulerXYZ co-ordinates
                            /*Output_GetSegmentGlobalRotationEulerXYZ _Output_GetSegmentGlobalRotationEulerXYZ =
                                ViconClient.GetSegmentGlobalRotationEulerXYZ(SubjectName, SegmentName);
                            */

                            Output_GetSegmentGlobalRotationQuaternion _Output_GetSegmentGlobalRotationQuaternion =
                                ViconClient.GetSegmentGlobalRotationQuaternion(SubjectName, SegmentName);

                            if (!_Output_GetSegmentGlobalRotationQuaternion.Occluded)
                            {
                                double Xcord = _Output_GetSegmentGlobalTranslation.Translation[0] / 1000;
                                double Ycord = _Output_GetSegmentGlobalTranslation.Translation[1] / 1000;
                                double Zcord = _Output_GetSegmentGlobalTranslation.Translation[2] / 1000;
                                double[] PixelCoord = convertWorldCoordToPixel(!Convert.ToBoolean(SubjectIndex), Xcord, Ycord, Zcord);
                                //Console.WriteLine("    FolanCoord: {0} {1} {2} {3}", SegmentName, Xcord, Ycord, Zcord);
                                //Console.WriteLine("    PixelCoord: {0} {1} {2} {3}", SegmentName, PixelCoord[0], PixelCoord[1], PixelCoord[2]);

                                double[] q = _Output_GetSegmentGlobalRotationQuaternion.Rotation;


                                Quaternion currentQuaternion = new Quaternion(q[0], q[1], q[2], q[3]);
                                currentQuaternion.Normalize();

                                Quaternion goalOrienatation = new Quaternion(danceWindow.Quaternion_X, danceWindow.Quaternion_Y, danceWindow.Quaternion_Z, danceWindow.Quaternion_W);
                                goalOrienatation.Normalize();

                                int whichShoe = -1;
                                if (SegmentName.Equals("RightShoe", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (initialOrientationRight)
                                    {
                                        rightInitialInverseQuaternion = new Quaternion(q[0], q[1], q[2], q[3]);
                                        rightInitialInverseQuaternion.Normalize();
                                        rightInitialInverseQuaternion.Invert();

                                        initialOrientationRight = false;
                                    }
                                    Quaternion conversionMaterix = rightInitialInverseQuaternion * goalOrienatation;                                    
                                    //if (isInHomeSpace)
                                    //{
                                        //currentQuaternion = Quaternion.Multiply(currentQuaternion, RIGHT_SHOE_CORRECTION);
                                        //Quaternion correctionQ = new Quaternion();
                                  currentQuaternion = Quaternion.Multiply(currentQuaternion, conversionMaterix);
                                    //}
                                    //else
                                    //    currentQuaternion = Quaternion.Multiply(currentQuaternion, new Quaternion(danceWindow.Quaternion_X, danceWindow.Quaternion_Y, danceWindow.Quaternion_Z, danceWindow.Quaternion_W));

                                    q[0] = currentQuaternion.X;
                                    q[1] = currentQuaternion.Y;
                                    q[2] = currentQuaternion.Z;
                                    q[3] = currentQuaternion.W;
                                    
                                    whichShoe = RIGHT_SHOE_INDEX;
                                }
                                else if (SegmentName.Equals("LeftShoe", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (initialOrientationLeft)
                                    {
                                        leftInitialInverseQuaternion = new Quaternion(q[0], q[1], q[2], q[3]);
                                        leftInitialInverseQuaternion.Normalize();
                                        leftInitialInverseQuaternion.Invert();

                                        initialOrientationLeft = false;
                                    }
                                    Quaternion conversionMaterix = leftInitialInverseQuaternion * goalOrienatation;
                                //    if (isInHomeSpace)
                                        //currentQuaternion = Quaternion.Multiply(currentQuaternion, LEFT_SHOE_CORRECTION);
                                        currentQuaternion = Quaternion.Multiply(currentQuaternion, conversionMaterix);
                                  //  else
                                    //    currentQuaternion = Quaternion.Multiply(currentQuaternion, conversionMaterix);

//                                        currentQuaternion = Quaternion.Multiply(currentQuaternion, new Quaternion(danceWindow.Quaternion_X, danceWindow.Quaternion_Y, danceWindow.Quaternion_Z, danceWindow.Quaternion_W));
                                    

                                    q[0] = currentQuaternion.X;
                                    q[1] = currentQuaternion.Y;
                                    q[2] = currentQuaternion.Z;
                                    q[3] = currentQuaternion.W;

                                    whichShoe = LEFT_SHOE_INDEX;
                                }                                

                                //yaw = isInHomeSpace ? -yaw : yaw;

                                StreamsData.liveData[SubjectIndex] = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                                                    whichShoe,
                                                    PixelCoord[0],
                                                    PixelCoord[1],
                                                    PixelCoord[2],
                                                    currentQuaternion.X,
                                                    currentQuaternion.Y,
                                                    currentQuaternion.Z,
                                                    currentQuaternion.W);

                                nodeClient.sendData(StreamsData.liveData[SubjectIndex]);
                                
                            }
                        }
                    }                    
                }
            }
        }

        private static double[] convertWorldCoordToPixel(bool leftShoe, double Xcord, double Ycord, double Zcord)
        {
            double[] convertedCoord = new double[3];

            if (isInHomeSpace)          //if in Home Space
            {                
                convertedCoord[0] = ((-Xcord + ILAB_ROOM_CENTER_X) / ILAB_ROOM_WIDTH * screenResolution.Width);
                convertedCoord[1] = ((-Ycord - ILAB_ROOM_CENTER_Y) / ILAB_ROOM_LENGTH * screenResolution.Height);

                if (leftShoe)
                    convertedCoord[2] = (Zcord - ILAB_LEFT_SHOE_Z_BIAS) * ILAB_3D_Z_FACTOR;
                else
                    convertedCoord[2] = (Zcord - ILAB_RIGHT_SHOE_Z_BIAS) * ILAB_3D_Z_FACTOR;
            }
            else                          //if in CCIT
            {                
                convertedCoord[0] = ((Xcord + CCIT_ROOM_CENTER_X) / CCIT_ROOM_WIDTH * screenResolution.Width) - CCIT_X_PIXEL_BIAS;
                convertedCoord[1] = ((Ycord - CCIT_ROOM_CENTER_Y) / CCIT_ROOM_LENGTH * screenResolution.Height) - CCIT_Y_PIXEL_BIAS;
                //convertedCoord[1] = ((Xcord + CCIT_ROOM_CENTER_X) / CCIT_ROOM_WIDTH * screenResolution.Height) - CCIT_X_PIXEL_BIAS;
                //convertedCoord[0] = ((-Ycord + CCIT_ROOM_CENTER_Y) / CCIT_ROOM_LENGTH * screenResolution.Width) - CCIT_Y_PIXEL_BIAS;

                if (leftShoe)
                    convertedCoord[2] = (Zcord - CCIT_LEFT_SHOE_Z_BIAS) * CCIT_3D_Z_FACTOR;
                else
                    convertedCoord[2] = (Zcord - CCIT_RIGHT_SHOE_Z_BIAS) * CCIT_3D_Z_FACTOR;
            }
            //System.Console.WriteLine("begoo binam Zet chande: " + convertedCoord[2]);
            return convertedCoord;
        }

        public bool ViconInit()
        {

            // Make a new client
            ViconClient = new ViconDataStreamSDK.DotNET.Client();

            // Connect to a server
            //Console.Write("Connecting to {0} ...", HostName);
            if (!ViconClient.IsConnected().Connected)
            {
                // Direct connection
                if(isInHomeSpace)
                    ViconClient.Connect(HostName);
                else
                    ViconClient.Connect(CCITHostName);
                // Multicast connection
                // MyClient.ConnectToMulticast( HostName, "224.0.0.0" );

                System.Threading.Thread.Sleep(200);
                if (!ViconClient.IsConnected().Connected)
                    return false;
                //Console.Write(".");
            }
            //Console.WriteLine();

            // Enable some different data types
            ViconClient.EnableSegmentData();


            //Console.WriteLine("Segment Data Enabled: {0}", MyClient.IsSegmentDataEnabled().Enabled);

            // Set the streaming mode
            ViconClient.SetStreamMode(ViconDataStreamSDK.DotNET.StreamMode.ClientPull);

            // Set the global up axis

/*            ViconClient.SetAxisMapping( ViconDataStreamSDK.DotNET.Direction.Left,
                                      ViconDataStreamSDK.DotNET.Direction.Forward,
                                      ViconDataStreamSDK.DotNET.Direction.Up ); // Y-up
            */
            ViconClient.SetAxisMapping(ViconDataStreamSDK.DotNET.Direction.Right,
                          ViconDataStreamSDK.DotNET.Direction.Up,
                          ViconDataStreamSDK.DotNET.Direction.Forward); // Y-up


            Output_GetAxisMapping _Output_GetAxisMapping = ViconClient.GetAxisMapping();
            //Console.WriteLine("Axis Mapping: X-{0} Y-{1} Z-{2}", Adapt(_Output_GetAxisMapping.XAxis), Adapt(_Output_GetAxisMapping.YAxis), Adapt(_Output_GetAxisMapping.ZAxis));

            // Discover the version number
            Output_GetVersion _Output_GetVersion = ViconClient.GetVersion();
            //Console.WriteLine("Version: {0}.{1}.{2}", _Output_GetVersion.Major, _Output_GetVersion.Minor, _Output_GetVersion.Point);
            return true;
        }        
      
        public MainWindow()
        {       
            InitializeComponent();
            //screenResolution = Screen.PrimaryScreen.Bounds;
            videoPlayer = new Video();
            danceWindow = new DanceFloor();
            if (Screen.AllScreens.Count() > 1)
            {
                screenResolution = Screen.AllScreens[1].Bounds;
                Screen secondSCR = Screen.AllScreens[1];
                System.Drawing.Rectangle rect2ndSCR = secondSCR.WorkingArea;                
                danceWindow.Top = rect2ndSCR.Top;
                danceWindow.Left = rect2ndSCR.Left;
                danceWindow.Width = rect2ndSCR.Width;
                danceWindow.Height = rect2ndSCR.Height;
                videoPlayer.Top = rect2ndSCR.Top;
                videoPlayer.Left = rect2ndSCR.Left + 1;
                videoPlayer.Width = rect2ndSCR.Width + 9;
                videoPlayer.Height = rect2ndSCR.Height;
                
            }
            else 
            {
                screenResolution = Screen.AllScreens[0].Bounds;
                Screen secondSCR = Screen.AllScreens[0];
                System.Drawing.Rectangle rect2ndSCR = secondSCR.WorkingArea;
                danceWindow.Top = rect2ndSCR.Top;
                danceWindow.Left = rect2ndSCR.Left;
                danceWindow.Width = rect2ndSCR.Width;
                danceWindow.Height = rect2ndSCR.Height;
            }
            videoPlayer.WindowState = System.Windows.WindowState.Normal;
            danceWindow.WindowState = System.Windows.WindowState.Normal;
            videoPlayer.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            danceWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            //danceWindow.Show();
            //danceWindow.WindowState = System.Windows.WindowState.Maximized;
            //danceWindow.Topmost = true;

            //Create3DViewPort();
            nodeClient = new NodeClient();
            socketTimer = new System.Windows.Threading.DispatcherTimer();
            socketTimer.Tick += new EventHandler(connection_Tick);
            socketTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            startRec.Click += startRec_Click;
            finishRec.Click += finishRec_Click;
            startPlayback.Click += startPlayback_Click;
            finishPlayback.Click += finishPlayback_Click;
            exit.Click += exit_Click;
            
            animationTimer = new System.Windows.Threading.DispatcherTimer();
            animationTimer.Tick += new EventHandler(loadAnimation_Tick);
            animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
           
            Console.WriteLine("Ready to sort one or more text lines...");

            participant_ID.Focus();
            //System.Windows.Application.Current.MainWindow.WindowState = WindowState.Maximized;
            //System.Windows.Application.Current.MainWindow.WindowStyle = WindowStyle.None;            
       }

        private void Create3DViewPort()
        {
            ObjReader CurrentHelixObjReader = new ObjReader();
            remote_rshoe3D = CurrentHelixObjReader.Read("right.obj");
            ObjReader CurrentHelixObjReader2 = new ObjReader();
            remote_lshoe3D = CurrentHelixObjReader2.Read("left.obj");
            CurrentHelixObjReader = new ObjReader();
            local_rshoe3D = CurrentHelixObjReader.Read("right.obj");
            CurrentHelixObjReader2 = new ObjReader();
            local_lshoe3D = CurrentHelixObjReader2.Read("left.obj");

            foreach (var m in remote_lshoe3D.Children)
            {
                var mGeo = m as GeometryModel3D;

                mGeo.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Tomato));
            }

            foreach (var m in remote_rshoe3D.Children)
            {
                var mGeo = m as GeometryModel3D;

                mGeo.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Tomato));
            }

            foreach (var m in local_lshoe3D.Children)
            {
                var mGeo = m as GeometryModel3D;

                mGeo.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
            }

            foreach (var m in local_rshoe3D.Children)
            {
                var mGeo = m as GeometryModel3D;

                mGeo.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
            }

            danceWindow.remote_rshoe.Content = remote_rshoe3D;
            danceWindow.remote_lshoe.Content = remote_lshoe3D;
            danceWindow.local_rshoe.Content = local_rshoe3D;
            danceWindow.local_lshoe.Content = local_lshoe3D;            
                      
            danceWindow.viewport.CameraController.DefaultCamera = new PerspectiveCamera();
            if(isInHomeSpace)
                danceWindow.viewport.CameraController.CameraPosition = new Point3D(0, 0, HOMESPACE_CAMERA_Z_OFFSET);
            else
                danceWindow.viewport.CameraController.CameraPosition = new Point3D(0, 0, CCIT_CAMERA_Z_OFFSET);
            danceWindow.viewport.CameraController.CameraTarget = new Point3D(0, 0, 0);
            danceWindow.viewport.CameraController.CameraUpDirection = new Vector3D(0, 1, 0);

            //danceWindow.viewport.Children.Add(new GridLinesVisual3D());

            TranslateTransform3D tt3d = new TranslateTransform3D(0, 0, 0);            
            danceWindow.local_lshoe.Transform = tt3d;
            danceWindow.local_rshoe.Transform = tt3d;
            danceWindow.remote_lshoe.Transform = tt3d;
            danceWindow.remote_rshoe.Transform = tt3d;
            //danceWindow.light.Brightness = 1;
            //danceWindow.light.Position = new Point3D(0, 0, CAMERA_Z_OFFSET);
            //danceWindow.localDancerPlane.Visible = false;
        }
        
        void startProgram(object sender, RoutedEventArgs e)
        {            
            if (homespaceRadio.IsChecked == true)
                isInHomeSpace = true;
            else
                isInHomeSpace = false;

            if (participant_ID.Text.Equals(""))
            {
                System.Windows.MessageBox.Show("Please enter the participant ID");
                return;
            }


            this.Title = "Wait for a couple of seconds...";


            bool viconConnected = ViconInit();            
            if (!viconConnected)
            {
                System.Windows.MessageBox.Show("Can not connect to the Vicon Server");
                System.Windows.Application.Current.Shutdown();
                return;                                
            }

            bool socketIsConnecting = socketIOInit();
            if (!socketIsConnecting)
            {
                System.Windows.MessageBox.Show("Can not connect to the Web Server");
                Console.WriteLine();
                System.Windows.Application.Current.Shutdown();
                return;                                
            }

            startNetworkingButton.IsEnabled = false;
            this.Title = "Control Panel";

            viconThread = new ViconStreamThread();            

            if(!isInHomeSpace)
                gridCenter = new double[] { 200, 600, 0};   
            else
                gridCenter = new double[] { 0, 300, 0 };   

            socketTimer.Start();

            danceWindow.Show();
            danceWindow.WindowState = System.Windows.WindowState.Maximized;
            danceWindow.Topmost = true;            
            Create3DViewPort();

            if (!isInHomeSpace)
            {
                danceWindow.GridHeight = INITIAL_CCIT_GRID_HEIGHT;
                danceWindow.GridWidth = INITIAL_CCIT_GRID_WIDTH;
                danceWindow.GridLength = INITIAL_CCIT_GRID_LENGTH;
            }
            else
            {
                danceWindow.GridHeight = INITIAL_ILAB_GRID_HEIGHT;
                danceWindow.GridWidth = INITIAL_ILAB_GRID_WIDTH;
                danceWindow.GridLength = INITIAL_ILAB_GRID_LENGTH;
            }
            finishRec.IsEnabled = true;
            startRec.IsEnabled = true;
            startPlayback.IsEnabled = true;
            programStarted = true;            
            //danceWindow.localDancerPlane. = 0.6;
            //exit.Visibility = System.Windows.Visibility.Visible;            
            //danceWindow.localDancerPlane.Visible = true;
        }

        private void connection_Tick(object sender, EventArgs e)
        {
            if (nodeClient.connectionMade)
            {
                if (!nodeClient.handshook && !handshakeRequest)
                {
                    if (isInHomeSpace)
                        nodeClient.handshake("homeSpace", participant_ID.Text);
                    else
                        nodeClient.handshake("ccit", participant_ID.Text);
                    handshakeRequest = true;
                }
                else if (nodeClient.handshook)
                {
                    //danceWindow.Show();
                    
                    socketTimer.Stop();
                    viconThread.th.Start();
                    animationTimer.Start();
                }
            }
        }

        private bool socketIOInit()
        {            
            return nodeClient.connectToServer();
        }        
        
        private void loadAnimation_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                bool remoteShoes = false;                      //whether we are rendering remote dancer shoes or local dancer shoes
                if (showLocalFeet.IsChecked == true)
                {
                    for (int objectIndex = 0; objectIndex < objectNum; objectIndex++)
                    {
                        if (StreamsData.liveData[objectIndex] != null)
                        {
                            processString(StreamsData.liveData[objectIndex], remoteShoes);
                        }
                    }
                }

                remoteShoes = true;
                for (int objectIndex = 0; objectIndex < objectNum; objectIndex++)
                {
                    if (StreamsData.networkData[objectIndex] != null)
                    {
                        processString(StreamsData.networkData[objectIndex], remoteShoes);
                    }                  
                }                
            }));
            
            localDancerPlaneTransform = quaternionToMatrix(new Quaternion(danceWindow.Quaternion_X, danceWindow.Quaternion_Y, danceWindow.Quaternion_Z, danceWindow.Quaternion_W));
            localDancerPlaneTransform.OffsetX = gridCenter[0];
            localDancerPlaneTransform.OffsetY = gridCenter[1];            
            localDancerPlaneTransform.OffsetZ = gridCenter[2] + SHOEPLANE_Z_BIAS + danceWindow.GridHeight;                
            danceWindow.localDancerPlane.Transform = new MatrixTransform3D(localDancerPlaneTransform);
            
            //Asking why? That's how it is!
            danceWindow.localDancerPlane.Length = danceWindow.GridWidth;
            danceWindow.localDancerPlane.Width = danceWindow.GridLength;
            
            /*
            //Here, we are changing offestY in order to use it for the remote dancer plane transformation
            if(isInHomeSpace)
                localDancerPlaneTransform.OffsetY = gridCenter[1] + HOMESPACE_PLANE_DISTANCES;
            else
                localDancerPlaneTransform.OffsetY = gridCenter[1] + CCIT_PLANE_DISTANCES;
            danceWindow.remoteDancerPlane.Transform = new MatrixTransform3D(localDancerPlaneTransform);
            danceWindow.remoteDancerPlane.Length = danceWindow.GridLength;
            danceWindow.remoteDancerPlane.Width = danceWindow.GridWidth;
             */
            if (showPlane.IsChecked == true)
            {
                SolidColorBrush localPlane = new SolidColorBrush(Colors.Gray);
                localPlane.Opacity = danceWindow.GridOpacity;
                danceWindow.localDancerPlane.Fill = localPlane;
            }
            /*
            SolidColorBrush remotePlane = new SolidColorBrush(Colors.Gray);
            remotePlane.Opacity = danceWindow.GridOpacity;
            danceWindow.remoteDancerPlane.Fill = remotePlane;
<<<<<<< .mine
            if (recordingFinishedAck == true)
                finishRecordingAck();

=======
            */
            if (recordingFinishedAck == true)
                finishRecordingAck();

            if (fileLoaded == true)
            {
                videoPlayer.start();
                fileLoaded = false;
            }
        }

        private void processString(String line, bool remoteShoes)
        {
            string[] COORDs = line.Split(',');
            bool leftShoe = Convert.ToInt32(COORDs[0]) == LEFT_SHOE_INDEX;
            
            double Xcord = Convert.ToDouble(COORDs[1]);
            double Ycord = Convert.ToDouble(COORDs[2]);
            double Zcord = Convert.ToDouble(COORDs[3]);
            double[] q = { Convert.ToDouble(COORDs[4]), Convert.ToDouble(COORDs[5]), Convert.ToDouble(COORDs[6]), Convert.ToDouble(COORDs[7])};
            
            animateShoes(leftShoe, remoteShoes, Xcord, Ycord, Zcord, q);
            
        }

        private void animateShoes(bool leftShoe, bool remoteShoes, double current_x, double current_y, double current_z, double[] q)
        {
            Matrix3D m3d; 
            if (isInHomeSpace)
            {
                Point3D currentCameraPos = danceWindow.viewport.CameraController.CameraPosition;
                currentCameraPos.Y = -HOMESPACE_CAMERA_Z_OFFSET * danceWindow.ShoePlane / (MAX_SHOE_PLANE_DEGREE + 1);
                currentCameraPos.Z = Math.Sqrt(HOMESPACE_CAMERA_Z_OFFSET * HOMESPACE_CAMERA_Z_OFFSET - currentCameraPos.Y * currentCameraPos.Y);
                danceWindow.viewport.CameraController.CameraPosition = currentCameraPos;
                //danceWindow.light.Position = currentCameraPos;
                danceWindow.viewport.CameraController.CameraTarget = new Point3D(0, 0, 0);
                Quaternion correctedRotation = new Quaternion(q[0], q[1], -q[2], q[3]);
                m3d = generateTheTransformMatrix(correctedRotation, current_x, current_y, current_z, leftShoe, remoteShoes);
            }
            else
            {
                Point3D currentCameraPos = danceWindow.viewport.CameraController.CameraPosition;
                currentCameraPos.Y = -CCIT_CAMERA_Z_OFFSET * danceWindow.ShoePlane / (MAX_SHOE_PLANE_DEGREE + 1);
                currentCameraPos.Z = Math.Sqrt(CCIT_CAMERA_Z_OFFSET * CCIT_CAMERA_Z_OFFSET - currentCameraPos.Y * currentCameraPos.Y);
                danceWindow.viewport.CameraController.CameraPosition = currentCameraPos;
                //danceWindow.light.Position = currentCameraPos;
                danceWindow.viewport.CameraController.CameraTarget = new Point3D(0, 0, 0);
                Quaternion correctedRotation = new Quaternion(-q[0], -q[1], -q[2], q[3]);
                m3d = generateTheTransformMatrix(correctedRotation, current_x, current_y, current_z, leftShoe, remoteShoes);
            }
            
            
            //Shadow orientation and translation
            double roll = Math.Atan2(2.0 * (q[0] * q[1] + q[2] * q[3]), 1.0 - (2.0 * (q[1] * q[1] + q[2] * q[2]))) * 180 / Math.PI;
            // double yaw = Math.Atan2(2.0 * (q[0] * q[3] + q[1] * q[2]), 1.0 - (2.0 * (q[2] * q[2] + q[3] * q[3]))) * 180 / PI;
            //  double pitch = Math.Asin(2.0 * (q[0] * q[2] - q[3] * q[1]));
            RotateTransform3D rt;
            if(!parallelDancing && remoteShoes)
                rt = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), roll));
            else
                rt = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180 + roll));
            TranslateTransform3D tt;
            if(remoteShoes)
                tt = new TranslateTransform3D(m3d.OffsetX, m3d.OffsetY, localDancerPlaneTransform.OffsetZ + SHADOW_Z_OFFSET);
            else
                tt = new TranslateTransform3D(m3d.OffsetX, m3d.OffsetY, localDancerPlaneTransform.OffsetZ + SHADOW_Z_OFFSET);
            Transform3DGroup myTransformGroup = new Transform3DGroup();
            myTransformGroup.Children.Add(rt);
            myTransformGroup.Children.Add(tt);

            double shadow_opacity = 1 - (current_z / 2);
            
            //Apply orientations and translations 
            if (remoteShoes)
            {
                if (leftShoe)
                {
                    danceWindow.remote_lshoe.Transform = new MatrixTransform3D(m3d);                    
                    danceWindow.remote_lshadow.Transform = myTransformGroup;                    
                    if(shadow_opacity > MINIMUM_SHADOW_OPACITY)
                        danceWindow.remote_lshadow_ellipse.Opacity = shadow_opacity;                                        
                }
                else
                {
                    danceWindow.remote_rshoe.Transform = new MatrixTransform3D(m3d);
                    danceWindow.remote_rshadow.Transform = myTransformGroup;
                    if (shadow_opacity > MINIMUM_SHADOW_OPACITY)
                        danceWindow.remote_rshadow_ellipse.Opacity = shadow_opacity;                    
                }
            }
            else
            {
                if (leftShoe)
                {
                    danceWindow.local_lshoe.Transform = new MatrixTransform3D(m3d);
                    danceWindow.local_lshadow.Transform = myTransformGroup;
                    if (shadow_opacity > MINIMUM_SHADOW_OPACITY)
                        danceWindow.local_lshadow_ellipse.Opacity = shadow_opacity;      
                }
                else
                { 
                    danceWindow.local_rshoe.Transform = new MatrixTransform3D(m3d);
                    danceWindow.local_rshadow.Transform = myTransformGroup;
                    if (shadow_opacity > MINIMUM_SHADOW_OPACITY)
                        danceWindow.local_rshadow_ellipse.Opacity = shadow_opacity;
                }
            }
        }

        public static Matrix3D generateTheTransformMatrix(Quaternion orientation, double x, double y, double z, bool leftShoe, bool remote_shoes)
        {
            Matrix3D m3d;

            if(remote_shoes && !parallelDancing && !isInHomeSpace)
                //m3d = quaternionToMatrix(Quaternion.Multiply(orientation, new Quaternion(0.008, 0, 0.999, 0)));
                m3d = quaternionToMatrix(Quaternion.Multiply(new Quaternion(0.008, 0, 0.999, 0),orientation));
            else if (remote_shoes && !parallelDancing && isInHomeSpace)
                m3d = quaternionToMatrix(Quaternion.Multiply(orientation, new Quaternion(0.008, 0, 0.999, 0)));
            else
                m3d = quaternionToMatrix(orientation);
           
            if (isInHomeSpace)
            {
                if (!parallelDancing && remote_shoes)
                {
                    if (!receivingOtherSidesData)
                    {
                        m3d.OffsetX = ILAB_TOWARDS_X_DISPLACEMENT - (x + ILAB_3D_X_OFFSET);
                        if (leftShoe)
                        {
                            m3d.OffsetY = ILAB_TOWARDS_Y_DISPLACEMENT - (y + ILAB_3D_Y_OFFSET);
                            m3d.OffsetZ = (z + ILAB_3D_LEFT_Z_OFFSET) * ILAB_3D_Z_FACTOR + danceWindow.GridHeight;
                        }
                        else
                        {
                            m3d.OffsetY = ILAB_TOWARDS_Y_DISPLACEMENT - (y + ILAB_3D_Y_OFFSET + ILAB_3D_RIGHT_Y_OFFSET);
                            m3d.OffsetZ = (z + ILAB_3D_RIGHT_Z_OFFSET) * ILAB_3D_Z_FACTOR + danceWindow.GridHeight;
                        }
                    }
                    else            //Data coming from CCIT and feet facing towards dancer
                    {
                        m3d.OffsetX = FROM_ILAB_CCIT_TOWARDS_X_DISPLACEMENT - x;
                        m3d.OffsetY = FROM_ILAB_CCIT_TOWARDS_Y_DISPLACEMENT - y;
                        if (leftShoe)
                            m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_LEFT_Z_OFFSET) + danceWindow.GridHeight;
                        else
                            m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_RIGHT_Z_OFFSET) + danceWindow.GridHeight;
                    }
                }
                else if (remote_shoes )
                {
                    if (!receivingOtherSidesData)
                    {
                        m3d.OffsetX = x + ILAB_3D_X_OFFSET;
                        if (leftShoe)
                        {
                            m3d.OffsetY = y + ILAB_3D_Y_OFFSET + ILAB_PARALLEL_Y_DISPLACEMENT;
                            m3d.OffsetZ = (z + ILAB_3D_LEFT_Z_OFFSET) * ILAB_3D_Z_FACTOR + danceWindow.GridHeight;
                        }
                        else
                        {
                            m3d.OffsetY = y + ILAB_3D_Y_OFFSET + ILAB_PARALLEL_Y_DISPLACEMENT + ILAB_3D_RIGHT_Y_OFFSET;
                            m3d.OffsetZ = (z + ILAB_3D_RIGHT_Z_OFFSET) * ILAB_3D_Z_FACTOR + danceWindow.GridHeight;
                        }
                    }
                    else            //Data coming from CCIT and feet next to dancer
                    {
                        m3d.OffsetX = x + FROM_ILAB_CCIT_PARALLEL_X_DISPLACEMENT;
                        m3d.OffsetY = y + FROM_ILAB_CCIT_PARALLEL_Y_DISPLACEMENT;
                        if (leftShoe)
                            m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_LEFT_Z_OFFSET) + danceWindow.GridHeight;
                        else
                            m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_RIGHT_Z_OFFSET) + danceWindow.GridHeight; 
                    }
               }
                else
                {
                    m3d.OffsetX = x + ILAB_3D_X_OFFSET + ILAB_PARALLEL_X_DISPLACEMENT;
                    if (leftShoe)
                    {
                        m3d.OffsetY = y + ILAB_3D_Y_OFFSET;
                        m3d.OffsetZ = (z + ILAB_3D_LEFT_Z_OFFSET) * ILAB_3D_Z_FACTOR + danceWindow.GridHeight;
                    }
                    else
                    {
                        m3d.OffsetY = y + ILAB_3D_Y_OFFSET + ILAB_3D_RIGHT_Y_OFFSET;
                        m3d.OffsetZ = (z + ILAB_3D_RIGHT_Z_OFFSET) * ILAB_3D_Z_FACTOR + danceWindow.GridHeight;
                    }
                }
                
            }
            else            //In CCIT
            {
                if (!parallelDancing && remote_shoes)
                {
                    m3d.OffsetX = CCIT_TOWARDS_X_DISPLACEMENT - x;
                    m3d.OffsetY = CCIT_TOWARDS_Y_DISPLACEMENT - y;
                    if(leftShoe)
                        m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_LEFT_Z_OFFSET) + danceWindow.GridHeight;
                    else
                        m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_RIGHT_Z_OFFSET) + danceWindow.GridHeight;
                }
                else
                {
                    m3d.OffsetX = x;
                    m3d.OffsetY = y + CCIT_3D_Y_OFFSET;
                    if (leftShoe)
                        m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_LEFT_Z_OFFSET) + danceWindow.GridHeight;
                    else
                        m3d.OffsetZ = (z * ILAB_3D_Z_FACTOR + CCIT_3D_RIGHT_Z_OFFSET) + danceWindow.GridHeight;                   
                }
                
                //m3d.OffsetX = -y + CCIT_3D_X_OFFSET;
                //m3d.OffsetY = -z;
                //m3d.OffsetZ = 0;
            }
            return m3d;
        }

        public static Matrix3D quaternionToMatrix(Quaternion input)
        {
            double X = input.X, Y = input.Y, Z = input.Z, W = input.W;
            double xx = X * X;
            double xy = X * Y;
            double xz = X * Z;
            double xw = X * W;

            double yy = Y * Y;
            double yz = Y * Z;
            double yw = Y * W;

            double zz = Z * Z;
            double zw = Z * W;

            return new Matrix3D( 1 - 2 * (yy + zz), 2 * (xy - zw), 2 * (xz + yw), 0,
            2 * (xy + zw), 1 - 2 * (xx + zz), 2 * (yz - xw), 0,
            2 * (xz - yw), 2 * (yz + xw), 1 - 2 * (xx + yy), 0,
            0, 0, 0, 1);
        }

        void exit_Click(object sender, RoutedEventArgs e)
        {
            if (programStarted)
            {
                // Disconnect and dispose
                ViconClient.Disconnect();
                ViconClient = null;

                viconThread.th.Abort();
                animationTimer.Stop();
                nodeClient.Close();
            }
            System.Windows.Application.Current.Shutdown();
        }

        void finishRec_Click(object sender, RoutedEventArgs e)
        {
            finishRec.IsEnabled = false;
            nodeClient.finishRec();
            this.Title = "Saving the recording...";
        }

        void startRec_Click(object sender, RoutedEventArgs e)
        {
            startRec.IsEnabled = false;
            this.Title = "Recording...";
            nodeClient.startRec();            
        }

        public void finishRecordingAck()
        {
            this.Title = "Recording success";
            startRec.IsEnabled = true;
            finishRec.IsEnabled = true;
            recordingFinishedAck = false;
        }

        void startPlayback_Click(object sender, RoutedEventArgs e)
        {            
            videoPlayer.Show();
            //videoPlayer.WindowState = System.Windows.WindowState.Maximized;
            videoPlayer.WindowStyle = System.Windows.WindowStyle.None;
            videoPlayer.playbackInit(playbackFileIndex);

            nodeClient.startPlayback(playbackFileIndex);

            startPlayback.IsEnabled = false;
            playbackBox.IsEnabled = false;
            finishPlayback.IsEnabled = true;
        }
         
        void finishPlayback_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.stopPlayback();
            videoPlayer.Hide();

            startPlayback.IsEnabled = true;
            playbackBox.IsEnabled = true;
            finishPlayback.IsEnabled = false;

            nodeClient.finishPlayback(playbackFileIndex);
        }


        private void showLocalFeetChanged(object sender, RoutedEventArgs e)
        {
            if (danceWindow == null) return;

            if (showLocalFeet.IsChecked == false)
            {
                danceWindow.viewport.Children.Remove(danceWindow.local_lshoe);
                danceWindow.viewport.Children.Remove(danceWindow.local_rshoe);
                danceWindow.local_rshadow_ellipse.Visibility = Visibility.Hidden;
                danceWindow.local_lshadow_ellipse.Visibility = Visibility.Hidden;
             //   danceWindow.local_lshoe.Content. Visibility = Visibility.Hidden;
              //  danceWindow.leftRect.Visibility = Visibility.Hidden;
            }
            else
            {
                danceWindow.viewport.Children.Add(danceWindow.local_lshoe);
                danceWindow.viewport.Children.Add(danceWindow.local_rshoe);
                danceWindow.local_rshadow_ellipse.Visibility = Visibility.Visible;
                danceWindow.local_lshadow_ellipse.Visibility = Visibility.Visible;
               // danceWindow.local_lshoe.Visibility = Visibility.Visible;
                //danceWindow.leftRect.Visibility = Visibility.Visible;
            }
        }

        private void showRemoteFeetChanged(object sender, RoutedEventArgs e)
        {
            if (danceWindow == null) return;

            if (showRemoteFeet.IsChecked == false)
            {
                danceWindow.viewport.Children.Remove(danceWindow.remote_lshoe);
                danceWindow.viewport.Children.Remove(danceWindow.remote_rshoe);
                danceWindow.remote_rshadow_ellipse.Visibility = Visibility.Hidden;
                danceWindow.remote_lshadow_ellipse.Visibility = Visibility.Hidden;
            }
            else
            {
                danceWindow.viewport.Children.Add(danceWindow.remote_lshoe);
                danceWindow.viewport.Children.Add(danceWindow.remote_rshoe);
                danceWindow.remote_rshadow_ellipse.Visibility = Visibility.Visible;
                danceWindow.remote_lshadow_ellipse.Visibility = Visibility.Visible;
            }
        }

        private void showDancePlane(object sender, RoutedEventArgs e)
        {
            if (danceWindow == null) return;

            if (showPlane.IsChecked == false)
            {                
                SolidColorBrush localPlane = new SolidColorBrush(Colors.Gray);
                localPlane.Opacity = 0;
                danceWindow.localDancerPlane.Fill = localPlane;
            }            
        }
        private void orientationComboBox(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("Parallel to dancer"); 
            data.Add("Towards dancer");                       

            // ... Get the ComboBox reference.
            var comboBox = sender as System.Windows.Controls.ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the second item selected.
            comboBox.SelectedIndex = 1;
        }

        private void remoteOrientationChanged(object sender, SelectionChangedEventArgs e)
        {
	        var comboBox = sender as System.Windows.Controls.ComboBox;

	        // ... Set SelectedItem as Window Title.
	        string value = comboBox.SelectedItem as string;
            if (value.Equals("Towards dancer"))
            {
                parallelDancing = false;
            }
            else
            {
                parallelDancing = true;
            }
        }

        private void playbackBox_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("1");
            data.Add("2");
            data.Add("3");

            // ... Get the ComboBox reference.
            var comboBox = sender as System.Windows.Controls.ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the second item selected.
            comboBox.SelectedIndex = 0;
        }

        private void playbackBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;            
	        string value = comboBox.SelectedItem as string;

            // ... Set SelectedItem as the playback file number.      
            playbackFileIndex = value;
        }

        private void participant_ID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                startNetworkingButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        }
       
    }
}
 