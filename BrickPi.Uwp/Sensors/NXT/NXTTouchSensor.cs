using System;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.NXT
{
    public sealed class NXTTouchSensor: RawSensor
    {
        public event EventHandler<SensorEventArgs> OnPressed;

        public event EventHandler<SensorEventArgs> OnReleased;

        public bool State { get; set; }

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

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            bool previous = State;
            if (SensorType == SensorType.TOUCH)
            {
                RawValue = (int)responseData.GetBits(1, 1);
            }
            else
                base.UpdateSensorResponse(responseData);
            State = (RawValue != 0);
            if (previous != State)
            {
                this.OnChangedEventHandler(new TouchSensorEventArgs() { Pressed = State});
                if (State)
                {
                    if (null != OnPressed)
                        Task.Run(() => OnPressed(this, new SensorEventArgs()));
                }
                else
                {
                    if (null != OnReleased)
                        Task.Run(() => OnReleased(this, new SensorEventArgs()));
                }
            }
        }
    }
}
