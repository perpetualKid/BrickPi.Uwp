using System;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.NXT
{
    public sealed class NXTTouchSensor: RawSensor
    {
        public event EventHandler<SensorChangedEventArgs> OnPressed;

        public event EventHandler<SensorChangedEventArgs> OnReleased;

        public bool Pressed { get; set; }

        public NXTTouchSensor(SensorPort sensorPort): base(sensorPort)
        {
            SensorType = SensorType.TOUCH_DEBOUNCE;
        }

        public NXTTouchSensor(SensorPort sensorPort, SensorType sensorType) : base(sensorPort)
        {
            if (sensorType != SensorType.TOUCH && sensorType != SensorType.TOUCH_DEBOUNCE)
                throw new ArgumentOutOfRangeException();
            SensorType = sensorType;
        }

        internal protected override void UpdateSensorResponse(ProtocolArray responseData)
        {
            bool state = Pressed;
            if (SensorType == SensorType.TOUCH) //Touch has single bit response, vs Touch_Debounce reads a analog raw value
            {
                RawValue = (int)responseData.GetBits(1, 1);
            }
            else
                base.UpdateSensorResponse(responseData);
            Pressed = (RawValue != 0);
            if (state != Pressed)
            {
                this.OnChangedEventHandler(new TouchSensorChangedEventArgs() { Pressed = this.Pressed});
                if (Pressed)
                {
                    OnPressed?.Invoke(this, new SensorChangedEventArgs());
                }
                else
                {
                    OnReleased?.Invoke(this, new SensorChangedEventArgs());
                }
            }
        }
    }
}
