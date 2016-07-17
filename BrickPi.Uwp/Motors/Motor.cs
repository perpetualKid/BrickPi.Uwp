using System;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Motors
{
    /// <summary>
    /// Holds all motor information
    /// </summary>
    public class Motor
    {
        private int velocity;
        private int encoderOffsetCache;
        private byte encoderOffsetBits;
        private byte encoderOffsetMask;

        private readonly MotorPort motorPort;

        public Motor(MotorPort motorPort)
        {
            this.motorPort = motorPort;
        }

        public MotorPort MotorPort { get { return this.motorPort; } }

        /// <summary>
        /// Set the velocity of motors, max is 255 and min is -255, 0 is stopped
        /// </summary>
        public int Velocity { get { return velocity; } set { velocity = Math.Min(Math.Abs(value), 255) * Math.Sign(value); } }

        /// <summary>
        /// Enable motors with 1, stop with 0
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Change the encoder offset
        /// </summary>
        public int EncoderOffset { get; set; }

        /// <summary>
        /// Encoder of the motors, 1 = 0.5 degreese, 720 = 360 degrees
        /// </summary>
        public int Encoder { get; set; }


        public virtual void UpdateEncoderRequest(ProtocolArray requestData)
        {
            if (EncoderOffset != 0)
            {
                if (0 == encoderOffsetCache)
                {
                    encoderOffsetBits = ProtocolArray.SignificantBits((uint)Math.Abs(EncoderOffset));
                    encoderOffsetMask = (byte)((encoderOffsetBits << 1) + 1);
                    encoderOffsetBits++;
                    encoderOffsetCache = (Math.Abs(EncoderOffset) << 1) | -(Math.Sign(EncoderOffset) >> 1); //odd numbers are negative speed/backward direction
                }
                requestData.SetBits(1, 6, encoderOffsetMask);
                requestData.SetBits(1, encoderOffsetBits, (uint)encoderOffsetCache);
            }
            else
                requestData.SetBits(1, 1, 0);

        }

        public virtual void UpdateVelocityRequest(ProtocolArray requestData)
        {
            int speed = Velocity;
            speed = ((Math.Abs(speed) & 0xFF) << 1 | -(Math.Sign(speed) >> 1)) << 1 | (Enabled.ToInt() & 0x01);
            requestData.SetBits(1, 10, (uint)speed);
        }

        public virtual void UpdateResponse(ProtocolArray responseData, byte encoderBits)
        {
            uint tacho = responseData.GetBits(1, encoderBits);
            Encoder = (int)((tacho >> 1) * (1 + (-(tacho & 0x01) << 1))); //odd numbers are negative speed/backward direction
            EncoderOffset = 0;
        }

    }
}
