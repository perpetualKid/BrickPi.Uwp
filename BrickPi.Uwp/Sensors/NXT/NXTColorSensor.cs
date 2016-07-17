using System;
using System.Linq;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.NXT
{
    public sealed class NXTColorSensor: RawSensor
    {
        private readonly TimeSpan? setupTime = TimeSpan.FromMilliseconds(300);
        private ARGBColor colorData;

        public NXTColorSensor(SensorPort sensorPort) : base(sensorPort)
        {
            SensorType = SensorType.COLOR_FULL;
        }

        public NXTColorSensor(SensorPort sensorPort, SensorType sensorType) : base(sensorPort)
        {
            if (sensorType < SensorType.COLOR_FULL && sensorType > SensorType.COLOR_NONE)
                throw new ArgumentOutOfRangeException();
            SensorType = sensorType;
        }

        public ARGBColor ColorData
        {
            get { return colorData; }
        }

        public string ColorName
        {
            get { return Enum.GetName(typeof(Color), SensorType == SensorType.COLOR_FULL ? (Color)RawValue : Color.None); }
        }
        

        public override TimeSpan? InitializeSensorRequest(ProtocolArray requestData)
        {
            //ColorSensor needs some extra time for setup (due to calibration?)
            return (base.InitializeSensorRequest(requestData)).Max(setupTime);
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            if (SensorType == SensorType.COLOR_FULL)
            {
                RawValue = (int)responseData.GetBits(1, 3);
                colorData = new ARGBColor(
                    (int)responseData.GetBits(1, 10), 
                    (int)responseData.GetBits(1, 10), 
                    (int)responseData.GetBits(1, 10), 
                    (int)responseData.GetBits(1, 10));
            }
            else
                base.UpdateSensorResponse(responseData);
        }
    }
}
