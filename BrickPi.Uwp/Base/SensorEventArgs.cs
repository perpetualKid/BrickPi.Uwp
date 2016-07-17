using System;

namespace BrickPi.Uwp.Base
{
    public class SensorEventArgs: EventArgs
    {

    }

    public class TouchSensorEventArgs: SensorEventArgs
    {
        public bool Pressed { get; set; }
    }

    public class UltraSonicSensorEventArgs: SensorEventArgs
    {
        public int Distance { get; set; }             
    }

    public interface IPropertyChangedEvent
    {
        event EventHandler<SensorEventArgs> OnChanged;
    }
}
