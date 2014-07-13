using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using ARCameraHeadAPI;
using DebugHelper;
using System.Diagnostics;

namespace APITest
{
    /// <summary>
    /// Class that simulates camera head
    /// </summary>
    class COMSimulator : COMAPI
    {

        Thread _angleThread;

        /// <summary>
        /// Construct the object and start communication on the given port
        /// </summary>
        /// <param name="portName">name of the port to connect tx</param>
        public COMSimulator(string portName) : base(portName)
        {
            _angleThread = new Thread(new ThreadStart(_mechanicsModel));
            _angleThread.Start();
        }

        override public void Disconnect(Action onDisconnect)
        {
            _angleThread.Abort();
            base.Disconnect(onDisconnect);
        }

        /// <summary>
        /// a simple method to simulate the machanics of the camera head
        /// </summary>
        private void _mechanicsModel()
        {
            Random generator = new Random();
            while (true)
            {
                Thread.Sleep(300);
                //copy angle values from receiverData to transmitterData and add a random value 
                horizontalAngle = horizontalAngle + (0.5 - generator.NextDouble()) * 0.2;
                verticalAngle = verticalAngle + (0.5 - generator.NextDouble()) * 0.2;
            }
        }

        /// <summary>
        /// transmitter thread method that starts transmitting right after receiving first packet from the host
        /// </summary>
        override public void Transmitter()
        {
            while(!_isInitialized)  //wait for communication initialization
                Thread.Sleep(_frameTOL);
            base.Transmitter();
        }
    }
}