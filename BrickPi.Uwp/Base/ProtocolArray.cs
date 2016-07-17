using System;

namespace BrickPi.Uwp.Base
{
    /// <summary>
    /// Bit-Buffer for communication protocol RPI and BrickPi
    /// </summary>
    public sealed class ProtocolArray
    {
        private byte[] array;

        public int Position { get; set; }

        public byte[] Data { get { return array; } }

        #region .ctor
        public ProtocolArray(): this (256)
        {
        }

        public ProtocolArray(int size)
        {
            array = new byte[size];
        }

        public ProtocolArray(byte[] value)
        {
            array = value;
        }

        public byte this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
        #endregion

        /// <summary>
        /// Number of bits required to store the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Number of bits need to store the value</returns>
        public static byte SignificantBits(uint value)
        {
            return (byte)(1 + Math.Floor(Math.Log(value, 2)));
        }

        /// <summary>
        /// Number of Bytes actually used
        /// </summary>
        /// <returns></returns>
        public int Bytes
        {
            get { return (int)Math.Ceiling(Position / 8.0); }
        }

        /// <summary>
        /// Get bits, so convert number of bits into a long
        /// </summary>
        /// <param name="offset">where to start in byte from the array</param>
        /// <param name="bits">number of bits to read</param>
        /// <returns></returns>
        public uint GetBits(int offset, byte bits)
        {
            uint result = 0;

            int i = 0;
            while (i < bits)
            {
                result |= (uint)(((array[((offset + ((Position + i) >> 3)))] >> ((Position + i) % 8))) & (0x01)) << i++;
            }

            Position += bits;
            return result;

        }

        /// <summary>
        /// Sets bits in the data buffer
        /// </summary>
        /// <param name="offset">where to start in byte from the array</param>
        /// <param name="bits">number of bits to write</param>
        /// <param name="value">the data to transform as bit</param>
        public void SetBits(byte offset, byte bits, uint value)
        {
            byte[] bytes = BitConverter.GetBytes((value & ((0x01 << bits) - 1)) << Position);
            foreach(byte byteValue in bytes)
            {
                array[offset++] |= byteValue;
            }
            Position += bits;
        }
    }
}
