using System;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors
{
    public class RawSensor: IPropertyChangedEvent
    {
        private readonly SensorPort sensorPort;

        public event EventHandler<SensorChangedEventArgs> OnChanged;

        protected virtual void OnChangedEventHandler(SensorChangedEventArgs eventArgs)
        {
            OnChanged?.Invoke(this, eventArgs ?? new SensorChangedEventArgs());
        }

        public RawSensor(SensorPort sensorPort)
        {
            this.sensorPort = sensorPort;
        }

        public SensorPort SensorPort { get { return this.sensorPort; } }

        public SensorType SensorType { get; set; }

        public int RawValue { get; set; }

        public int ChangeEventThreshold { get; set; } = 1;

        internal protected virtual TimeSpan? InitializeSensorRequest(ProtocolArray requestData)
        {
            requestData[Const.SensorIndex + (int)sensorPort.ArduinoPort()] = (byte)SensorType;
            return null;
        }

        internal protected virtual void UpdateSensorRequest(ProtocolArray requestData)
        {
            //nothing to do here
        }

        internal protected virtual void UpdateSensorResponse(ProtocolArray responseData)
        {
            //any specific implementations should be done in dedicated sensor classes
            switch (SensorType)
            {
                case SensorType.RAW:
                    //this is 0 value, LIGHT_OFF is 0 as well
                    //case SensorType.LIGHT_OFF:
                    RawValue = (int)responseData.GetBits(1, 10);
                    break;
                //case SensorType.LIGHT_ON:     //required for EOPD sensor, acting as RAW Sensor
                //    throw new NotImplementedException();
                //case SensorType.RCX_LIGHT:
                //    throw new NotImplementedException();

                case SensorType.EV3_INFRARED_M2:
                case SensorType.EV3_GYRO_M3:
                case SensorType.EV3_COLOR_M3:
                    RawValue = (int)responseData.GetBits(1, 32);
                    break;
                case SensorType.EV3_US_M0:
                case SensorType.EV3_US_M1:
                case SensorType.EV3_US_M2:
                case SensorType.EV3_US_M3:
                case SensorType.EV3_US_M4:
                case SensorType.EV3_US_M5:
                case SensorType.EV3_US_M6:
                case SensorType.EV3_COLOR_M0:
                case SensorType.EV3_COLOR_M1:
                case SensorType.EV3_COLOR_M2:
                case SensorType.EV3_COLOR_M4:
                case SensorType.EV3_COLOR_M5:
                case SensorType.EV3_GYRO_M0:
                case SensorType.EV3_GYRO_M1:
                case SensorType.EV3_GYRO_M2:
                case SensorType.EV3_GYRO_M4:
                case SensorType.EV3_INFRARED_M0:
                case SensorType.EV3_INFRARED_M1:
                case SensorType.EV3_INFRARED_M3:
                case SensorType.EV3_INFRARED_M4:
                case SensorType.EV3_INFRARED_M5:
                    RawValue = (int)responseData.GetBits(1, 16);
                    //# EV3 Gyro Mode 0, Adjust sign
                    if (SensorType == SensorType.EV3_GYRO_M0)
                    {
                        if (RawValue >= short.MaxValue)        //# Negative number.  This seems to return a 2 byte number.
                            RawValue = RawValue - 65535;
                    }
                    //# EV3 Gyro Mode 1, Adjust sign
                    else if (SensorType == SensorType.EV3_GYRO_M1)
                    {
                        if (RawValue >= short.MaxValue) //		# Negative number.  This seems to return a 2 byte number.
                            RawValue = RawValue - 65535;
                    }
                    break;
                case SensorType.EV3_TOUCH_DEBOUNCE:
                case SensorType.COLOR_RED:
                case SensorType.COLOR_GREEN:
                case SensorType.COLOR_BLUE:
                case SensorType.COLOR_NONE:
                default:
                    RawValue = (int)responseData.GetBits(1, 10);
                    break;
            }
        }
    }
}
