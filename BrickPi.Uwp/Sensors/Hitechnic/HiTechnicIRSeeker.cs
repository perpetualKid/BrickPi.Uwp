using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    public sealed class HiTechnicIRSeeker: I2CSensor
    {
        public enum ACModulationMode
        {
            AC_1200 = 0,
            AC_0600 = 1,
        }

        const byte HTIRS_I2C_ADDR = 0x10;      //Device address
        const byte HTIRS_DSP_MODE = 0x41;      //AC DSP mode - 0 = 1200Hz, 1 = 600Hz, written directly to Command Register
        const byte HTIRS_OFFSET =   0x42;      //Offset for data registers

        private ACModulationMode modulationMode;

        public HiTechnicIRSeeker(SensorPort sensorPort): this(sensorPort, ACModulationMode.AC_1200)
        {
        }

        public HiTechnicIRSeeker(SensorPort sensorPort, ACModulationMode modulationMode) : base(sensorPort)
        {
            this.modulationMode = modulationMode;
            SensorType = SensorType.I2C;
            Speed = 5;
            I2CData = new I2CDataCollection(1);

            I2CData[0].Setting = Const.BIT_I2C_MID;
            I2CData[0].Address = HTIRS_I2C_ADDR;
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTIRS_DSP_MODE;
            I2CData[0].WriteData[1] = (byte)modulationMode;
        }

        public int ACDirection { get; private set; }
        public int DCDirection { get; private set; }
        public int DCMean { get; private set; }

        public byte[] ACValues { get; } = new byte[5];
        public byte[] DCValues { get; } = new byte[5];

        public override void UpdateSensorRequest(ProtocolArray requestData)
        {
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 13;
            I2CData[0].WriteData[0] = HTIRS_OFFSET;

            base.UpdateSensorRequest(requestData);
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            base.UpdateSensorResponse(responseData);
            int acDirection = I2CData[0].ReadData[7];
            int dcDirection = I2CData[0].ReadData[0];
            if (Math.Abs(ACDirection - acDirection) >= ChangeEventThreshold || (Math.Abs(DCDirection - dcDirection) >= ChangeEventThreshold))
            {
                ACDirection = acDirection;
                DCDirection = dcDirection;
                for (int i= 0;i<5; i++)
                {
                    ACValues[i] = I2CData[0].ReadData[1 + i];
                }
                for (int i= 0;i<5; i++)
                {
                    DCValues[i] = I2CData[0].ReadData[8 + i];
                }
                DCMean = I2CData[0].ReadData[6];
                base.OnChangedEventHandler(new IRSeekerSensorChangedEventArgs() { ACDirection = this.ACDirection, DCDirection = this.DCDirection, DCMean = this.DCMean });
            }

        }
    }
}
