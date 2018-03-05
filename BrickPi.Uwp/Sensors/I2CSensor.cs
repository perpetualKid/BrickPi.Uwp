using System;
using System.Collections.Generic;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors
{
    public class I2CSensor : RawSensor
    {
        protected I2CDataCollection I2CData = new I2CDataCollection();

        protected SensorActionDelegate requestAction;
        protected SensorActionDelegate responseAction;

        public I2CSensor(SensorPort sensorPort) : base(sensorPort)
        {
            SensorType = SensorType.I2C;
        }

        public I2CSensor(SensorPort sensorPort, SensorType sensorType) : base(sensorPort)
        {
            SensorType = sensorType;
        }

        public uint Speed { get; set; }

        internal protected override TimeSpan? InitializeSensorRequest(ProtocolArray requestData)
        {
            TimeSpan? result = base.InitializeSensorRequest(requestData);

            //I2C setup
            if (SensorType == SensorType.I2C || SensorType == SensorType.I2C_9V || SensorType == SensorType.ULTRASONIC_CONT)
            {
                requestData.SetBits(3, 8, Speed);

                requestData.SetBits(3, 3, (uint)(I2CData.Count > 8 ? 8 : (I2CData.Count < 1 ? 1 : I2CData.Count)) - 1);

                foreach(I2CData device in I2CData)
                {
                    requestData.SetBits(3, 7, (uint)(device.Address >> 1));
                    requestData.SetBits(3, 2, (uint)device.Setting);
                    if ((device.Setting & Const.BIT_I2C_SAME) == Const.BIT_I2C_SAME)
                    {
                        requestData.SetBits(3, 4, (uint)device.WriteBytes);
                        requestData.SetBits(3, 4, (uint)device.ReadBytes);

                    for (int out_byte = 0; out_byte < device.WriteBytes; out_byte++)
                            requestData.SetBits(3, 8, (uint)device.WriteData[out_byte]);
                    }

                }
            }
            return result;
        }

        internal protected override void UpdateSensorRequest(ProtocolArray requestData)
        {
            requestAction?.Invoke(requestData);

            base.UpdateSensorRequest(requestData);

            int arduinoPort = (int)SensorPort.ArduinoPort();
            if ((SensorType == SensorType.I2C) ||
                (SensorType == SensorType.I2C_9V) ||
                (SensorType == SensorType.ULTRASONIC_CONT))
            {

                foreach (I2CData device in I2CData)
                {
                    if ((device.Setting & Const.BIT_I2C_SAME) != Const.BIT_I2C_SAME)
                    {
                        requestData.SetBits(1, 4, (uint)device.WriteBytes);
                        requestData.SetBits(1, 4, (uint)device.ReadBytes);
                        for (int out_byte = 0; out_byte < device.WriteBytes; out_byte++)
                            requestData.SetBits(1, 8, (uint)device.WriteData[out_byte]);
                    }
                }
            }
        }

        internal protected override void UpdateSensorResponse(ProtocolArray responseData)
        {
            RawValue = (int)responseData.GetBits(1, (byte)I2CData.Count);

            int deviceIndex = 0;
            foreach (I2CData device in I2CData)
            {
                if ((RawValue & (0x01 << deviceIndex)) != 0)
                    for (int in_byte = 0; in_byte < device.ReadBytes; in_byte++)
                        device.ReadData[in_byte] = (byte)responseData.GetBits(1, 8);
                deviceIndex++;
            }
            if (SensorType == SensorType.ULTRASONIC_CONT)
            {
                if (((int)RawValue & 0x01) != 0)
                    RawValue = I2CData[Const.US_I2C_IDX].ReadData[0];
                else
                    RawValue = -1;
            }

            responseAction?.Invoke(responseData);
        }
    }
}
