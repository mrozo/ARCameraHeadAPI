using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARCameraHeadAPI
{
    /// <summary>
    /// A struct used to store angle value and convert it to/from its 2 byte representation.
    /// </summary>
    public struct Angle
    {
        double _val;

        /// <summary>
        /// Initilize object using given value
        /// </summary>
        /// <param name="value"></param>
        public Angle(double value=0)
        {
            _val = value;
        }

        /// <summary>
        /// Initilize object using byte representation of the angle
        /// </summary>
        /// <param name="value"></param>
        public Angle(byte[] value)
        {
            _val = 0;
            byteRepr = value;
        }

        /// <summary>
        /// Angle value
        /// </summary>
        public double value
        {
            get { return _val; }
            set { _val = value % (2 * Math.PI); }
        }

        /// <summary>
        /// Get byte representation of the angle or set the angle using its byte representation
        /// </summary>
        public byte[] byteRepr
        {
            get { return BitConverter.GetBytes(_val * ((1 << 16) - 1) / (2 * Math.PI)); }
            set { FromByteArray(value, 0); }
        }

        /// <summary>
        /// Get angle from its byte representation from the given array at the offset.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public void FromByteArray(byte[] array, int offset)
        {
            _val = (double) (BitConverter.ToUInt16(array, offset) * 2 * Math.PI / ((1 << 16) - 1)); 
        }

        /// <summary>
        /// Converts the angle to byte array and insert into the given array at specified offset.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] ToByteArray(byte[] array, int offset)
        {
            BitConverter.GetBytes((UInt16) (_val * ((1 << 16) - 1) / (2 * Math.PI))).CopyTo(array,offset);
            return array;
        }
    }
}
