using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

#if DEBUG
using DebugHelper;
#endif

namespace ARCameraHeadAPI
{
    public class COMAPI
    {
        //TODO read from cfg
        /// <summary>
        /// configuration variables
        /// </summary>
        protected int _baudRate = 9600;
        Parity _parity = Parity.None;
        StopBits _stopBits = StopBits.One;
        protected int _dataBits = 8;
        /// <summary>
        /// frames time of life
        /// </summary>
        protected TimeSpan _frameTOL;
        /// <summary>
        /// 
        /// </summary>
        protected TimeSpan _waitTime;
        /// <summary>
        /// status of the communication
        /// </summary>
        protected bool _isInitialized = false;

        public int receivedPackets
        {
            get { return _receivedPackets; }
            protected set { _receivedPackets = value; }
        }

        public int transmittedPackets
        {
            get { return _transmittedPackets; }
            protected set { _receivedPackets = value; }
        }

        protected int _receivedPackets=0;
        protected int _transmittedPackets=0;

        protected String _portName;
        protected SerialPort _port;
        public CameraHeadData transmitterData;
        public CameraHeadData receiverData;
        protected bool _isActive = false;

        protected Thread _transmitterThread;

        /// <summary>
        /// camera head horizontal angle shift
        /// </summary>
        public double horizontalAngle
        {
            get { return receiverData.horizontalAngle.value ;}
            set { transmitterData.horizontalAngle.value = value;}
        }

        /// <summary>
        /// camera head vertical angle shift
        /// </summary>
        public double verticalAngle
        {
            get { return receiverData.verticalAngle.value; }
            set { transmitterData.verticalAngle.value = value; }
        }

        /// <summary>
        /// Method that lists all avalible com ports in the system
        /// </summary>
        /// <returns>List of ports names</returns>
        public static List<string> ListComPorts()
        {
            List<string> portsNames = new List<string>();
            foreach (String name in SerialPort.GetPortNames())
                portsNames.Add(name);
            return portsNames;
        }

        /// <summary>
        /// construct the object and start communication
        /// </summary>
        /// <param name="portName">Name of the COM port to connect to</param>
        public COMAPI(String portName)
        {
            _portName = portName;
            receiverData = new CameraHeadData();
            transmitterData = new CameraHeadData();
            transmitterData.horizontalAngle.value = Math.PI;
            transmitterData.verticalAngle.value = Math.PI;
            _Connect();
            _InitCommunication();
        }




        virtual public void Disconnect(Action onDisconnect)
        {
#if DEBUG 
            DbgConsole.Message("");
#endif
            //inform threads to stop
            _isActive = false;
            //wait fot threads to stop
            while( _transmitterThread.IsAlive ) {Thread.Sleep(_frameTOL); }

            _port.DataReceived -= Receiver;
            if (_port.IsOpen)
                _port.Close();
            onDisconnect();
        }


        private void _Connect()
        {
            _port = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
            _port.DtrEnable = false;
            _port.RtsEnable = false;
            _port.Handshake = Handshake.None;

            if (!_port.IsOpen)
                _port.Open();
#if DEBUG
            DbgConsole.Message("Connected to port " + _portName 
                + ", baud = " + _baudRate 
                + ", parity = " +Enum.GetName(typeof(Parity), _parity )
                + ", data bits = " + _dataBits 
                + ", stop bits = "+ Enum.GetName(typeof(StopBits), _stopBits));
#endif
        }

        /// <summary>
        /// initialize some protected configuration variables and starts communication threads
        /// </summary>
        protected void _InitCommunication()
        {
            long ticksPerPackage = (long)(101 / 100) * (9*10^7 / _baudRate);
            _frameTOL = new TimeSpan(ticksPerPackage) ;
            _waitTime = new TimeSpan(ticksPerPackage / 4L);
            _port.WriteTimeout = (int) (ticksPerPackage * 10 ^ 4 * 2);

            _isActive = true;

            _transmitterThread = new Thread(new ThreadStart(Transmitter));
            _transmitterThread.Name = "ARCameraHeadAPI.transmitter";
            _transmitterThread.Start();

            _port.ReceivedBytesThreshold = CameraHeadData.packetBytesCount;
            _port.DataReceived+=Receiver;
        }

        /// <summary>
        /// method used as thread to periodically send data over the port
        /// </summary>
        virtual public void Transmitter()
        {
            try
            {
                while (_isActive)
                {
                    _port.Write(
                        transmitterData.Serilize(), 0,
                        CameraHeadData.packetBytesCount
                        ); //send a frame of data
                    _transmittedPackets++;
                    Thread.Sleep(_frameTOL);//wait for the frame to be send
                    while (0 != _port.BytesToWrite) Thread.Sleep(_waitTime); //just to be sure
                }
            }
            catch (TimeoutException e)
            {
                if (_isActive)
                    throw e;
            }
#if DEBUG
            DbgConsole.Message("Exitting transmitter thread");
#endif
        }

        /// <summary>
        /// System.IO.SerialPort.DataReceived event handler. Unserilizes received data to _receiverData object and sets _isInitilized flag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></para>m
        protected void Receiver(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] receivedRawData = new byte[CameraHeadData.packetBytesCount];
            _port.Read(receivedRawData, 0, CameraHeadData.packetBytesCount);
            receiverData.Unserilize(receivedRawData);
            _receivedPackets++;
            _isInitialized = true;
        }
    }
}
