using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    public sealed class HiTechnicAccelerationSensor: I2CSensor
    {

        const byte HTACC_I2C_ADDR = 0x02;       // Device address
        const byte HTACC_OFFSET = 0x42;         // Offset for data registers 

        public HiTechnicAccelerationSensor(SensorPort sensorPort): base(sensorPort)
        {
            ChangeEventThreshold = 2;
            SensorType = SensorType.I2C;
            Speed = 5;
            I2CData = new I2CDataCollection(1);

            I2CData[0].Setting = Const.BIT_I2C_MID;
            I2CData[0].Address = HTACC_I2C_ADDR;

            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 6;
            I2CData[0].WriteData[0] = HTACC_OFFSET;
        }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Z { get; private set; }

        public override void UpdateSensorRequest(ProtocolArray requestData)
        {
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 6;
            I2CData[0].WriteData[0] = HTACC_OFFSET;

            base.UpdateSensorRequest(requestData);
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            base.UpdateSensorResponse(responseData);
            int x = ((sbyte)I2CData[0].ReadData[0] << 2) | (I2CData[0].ReadData[3] | 0b11);
            int y = ((sbyte)I2CData[0].ReadData[1] << 2) | (I2CData[0].ReadData[4] | 0b11);
            int z = ((sbyte)I2CData[0].ReadData[2] << 2) | (I2CData[0].ReadData[5] | 0b11);

            if ((Math.Abs(X - x) >= ChangeEventThreshold) || (Math.Abs(Y - y) >= ChangeEventThreshold) || (Math.Abs(Z - z)>=ChangeEventThreshold))
            {
                X = x;
                Y = y;
                Z = z;

                base.OnChangedEventHandler(new AccelerationSensorChangedEventArgs() { X = this.X, Y = this.Y, Z = this.Z });
            }
        }
    }
}
