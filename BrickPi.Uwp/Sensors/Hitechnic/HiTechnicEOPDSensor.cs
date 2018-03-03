using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{

    public sealed class HiTechnicEOPDSensor: RawSensor
    {
        public HiTechnicEOPDSensor(SensorPort sensorPort): this(sensorPort, SensorRange.Long)
        {
        }

        public HiTechnicEOPDSensor(SensorPort sensorPort, SensorRange range) : base(sensorPort)
        {
            switch (range)
            {
                case SensorRange.Short:
                SensorType = SensorType.LIGHT_OFF;
                break;
            case SensorRange.Long:
                SensorType = SensorType.LIGHT_ON;
                break;
            }
        }

        public int EOPDValue { get; private set; }

        public int ProcessedValue { get; private set; }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            base.UpdateSensorResponse(responseData);

            if (Math.Abs(EOPDValue - (1023 - RawValue)) >= ChangeEventThreshold)
            {
                EOPDValue = 1023 - RawValue;
                ProcessedValue = (int)Math.Round(Math.Sqrt(EOPDValue * 10));
                base.OnChangedEventHandler(new SensorChangedEventArgs());
            }
        }
    }
}
