using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCameraHeadAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class CameraHeadData
    {
        /// <summary>
        /// Size of the serialized to byte array CameraHeadData object
        /// </summary>
        public static int packetBytesCount = 10;

        public Angle horizontalAngle;
        public Angle verticalAngle;
        public Angle platformAngle;
        public double zoom;
        public byte cmdData;
        public Commands cmd;

        public CameraHeadData()
        {
            horizontalAngle.value = 0;
            verticalAngle.value = 0;
            platformAngle.value = 0;
            zoom = 0;
            cmdData = 0;
            cmd = Commands.Nop;
        }

        /// <summary>
        /// convert all data to byte array ready to send to the head
        /// </summary>
        /// <returns>Byte array ready to send to the device</returns>
        public byte[] Serilize()
        {
            byte[] byteRepr = new byte[packetBytesCount];
            horizontalAngle.ToByteArray(byteRepr, 0);
            verticalAngle.ToByteArray(byteRepr, 2);
            platformAngle.ToByteArray(byteRepr, 4);
            byte[] rawZoom = _serilizeDouble(zoom);
            byteRepr[9] = BitConverter.GetBytes((int)cmd)[0];

            return byteRepr;
        }

        /// <summary>
        /// Convert raw data from the device to a usable format
        /// </summary>
        /// <param name="rawData">Raw stream ob bytes received from the device</param>
        public void Unserilize(byte[] rawData)
        {
            horizontalAngle.FromByteArray(rawData, 0);
            verticalAngle.FromByteArray(rawData, 2);
            platformAngle.FromByteArray(rawData, 4);
            zoom =  _unserilizeDoubleVar(rawData[6],rawData[7]);
            cmd = (Commands)rawData[8];
            cmdData = rawData[9];
        }


        /// <summary>
        /// converts given double variable to byte array
        /// </summary>
        /// <param name="var"></param>
        /// <returns>byte array</returns>
        private byte[] _serilizeDouble(double var)
        {
            return BitConverter.GetBytes(var * ((1 << 16) - 1) / (2 * Math.PI));
        }

        /// <summary>
        /// converts given 2 bytes to double variable
        /// </summary>
        /// <param name="lsB">less significant Byte</param>
        /// <param name="msB">most significant Byte</param>
        /// <returns></returns>
        private double _unserilizeDoubleVar(byte lsB,byte msB)
        {
            return BitConverter.ToUInt16(new byte[] { lsB, msB }, 0);
        }
    }
}
