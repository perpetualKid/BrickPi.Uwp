using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    [System.Flags]
    public enum MultiplexerButtons
    {
        None = 0,
        Button1 = 1 << 0,
        Button2 = 1 << 1,
        Button3 = 1 << 2,
        Button4 = 1 << 3,
    }

    public sealed class TouchMultiplexerChangedEventArgs : SensorChangedEventArgs
    {
        public MultiplexerButtons ChangedButtons { get; internal set; }

        public MultiplexerButtons PressedButtons { get; internal set; }
    }

    public sealed class HiTechnicTouchMultiplexer: RawSensor
    {
        MultiplexerButtons buttonState;
        private int state = 0;

        public HiTechnicTouchMultiplexer(SensorPort sensorPort) : base(sensorPort)
        {
            SensorType = SensorType.LIGHT_OFF;
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            base.UpdateSensorResponse(responseData);
            int scaled = 346797 - RawValue * 342; //other doc says 339 https://github.com/botbench/robotcdriversuite/blob/master/include/hitechnic-touchmux.h
            scaled /= RawValue;
            scaled += 5;
            scaled /= 10;
            int changed = 0;
            changed ^= scaled;
            if (state != scaled)
            {
                this.OnChangedEventHandler(new TouchMultiplexerChangedEventArgs() {
                    PressedButtons = (MultiplexerButtons)scaled,
                    ChangedButtons = (MultiplexerButtons)(state ^ scaled)} );
                state = scaled;
            }
        }
    }
}
