using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    public sealed class HiTechnicTouchMultiplexer: RawSensor
    {
        private int state = 0;

        public HiTechnicTouchMultiplexer(SensorPort sensorPort) : base(sensorPort)
        {
            SensorType = SensorType.RAW;
        }

        public MultiplexerButtons Buttons { get { return (MultiplexerButtons)(state); } }

        internal protected override void UpdateSensorResponse(ProtocolArray responseData)
        {
            base.UpdateSensorResponse(responseData);
            int scaled = 346797 - RawValue * 342; //other doc says 339 https://github.com/botbench/robotcdriversuite/blob/master/include/hitechnic-touchmux.h
            scaled /= RawValue;
            scaled += 5;
            scaled /= 10;
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
