using System;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.NXT
{
    public sealed class NXTUltraSonicSensor: I2CSensor
    {
        public int Distance { get { return RawValue; } }

        public int Threshold { get; set; }
             
        public NXTUltraSonicSensor(SensorPort sensorPort) : this(sensorPort, SensorType.ULTRASONIC_CONT)
        {
        }

        public NXTUltraSonicSensor(SensorPort sensorPort, SensorType sensorType) : base(sensorPort)
        {
            if (sensorType != SensorType.ULTRASONIC_CONT && sensorType != SensorType.ULTRASONIC_SS)
                throw new ArgumentOutOfRangeException();
            SensorType = sensorType;

            I2CData = new I2CDataCollection(1);

            Speed = Const.US_I2C_SPEED;
            I2CData[0].Setting = Const.BIT_I2C_MID | Const.BIT_I2C_SAME;
            I2CData[0].Address = Const.LEGO_US_I2C_ADDR;
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 1;
            I2CData[0].WriteData[0] = Const.LEGO_US_I2C_DATA_REG;
        }

        public override TimeSpan? InitializeSensorRequest(ProtocolArray requestData)
        {
            TimeSpan? result;

            result = base.InitializeSensorRequest(requestData);

            if (SensorType == SensorType.ULTRASONIC_CONT)
            {
                //override sensortype to fake an I2C
                requestData[Const.SensorIndex + (int)SensorPort.ArduinoPort()] = (byte)SensorType.I2C;
            }

            return result;
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            int previous = RawValue;

            if (SensorType == SensorType.ULTRASONIC_CONT)
                base.UpdateSensorResponse(responseData);
            else //SensorType.ULTRASONIC_SS
                RawValue = (int)responseData.GetBits(1, 8);
            if (Math.Abs(previous - Distance) >= Threshold)
            {
                this.OnChangedEventHandler(new UltraSonicSensorEventArgs() { Distance = Distance });
            }
        }
    }
}
