using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    public sealed class HiTechnicGyroSensor: RawSensor
    {
        private bool calibrationMode;
        int rotationRate;
        int calibrationCounter;

        public int Offset { get; private set; }

        public int DirectionalRotationRate { get; private set; }

        public HiTechnicGyroSensor(SensorPort sensorPort): base(sensorPort)
        {
            SensorType = SensorType.RAW;
            ChangeEventThreshold = 2;
            calibrationMode = true;
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            base.UpdateSensorResponse(responseData);

            if (calibrationMode)
            {
                rotationRate += RawValue;
                calibrationCounter++;
                if (Offset == 0 && calibrationCounter > 10) //Initial Calibration, taking the first 10 readings
                {
                    EndCalibration();
                }
            }
            else if (Math.Abs(RawValue - rotationRate) >= ChangeEventThreshold)
            {
                rotationRate = RawValue;
                DirectionalRotationRate = rotationRate - Offset;
                base.OnChangedEventHandler(new GyroSensorChangedEventArgs() { DirectionalRotationRate = this.DirectionalRotationRate });
            }
        }

        public void BeginCalibration()
        {
            calibrationCounter = 0;
            rotationRate = 0;
            calibrationMode = true;
        }

        public void EndCalibration()
        {
            calibrationMode = false;
            Offset = rotationRate / calibrationCounter;
            rotationRate = RawValue;
        }
    }
}
