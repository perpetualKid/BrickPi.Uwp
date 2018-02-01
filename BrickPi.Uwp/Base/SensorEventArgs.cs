using System;

namespace BrickPi.Uwp.Base
{
    public class SensorChangedEventArgs: EventArgs
    {

    }

    public class TouchSensorChangedEventArgs: SensorChangedEventArgs
    {
        public bool Pressed { get; set; }
    }

    public class UltraSonicSensorEventArgs: SensorChangedEventArgs
    {
        public int Distance { get; set; }             
    }

    public interface IPropertyChangedEvent
    {
        event EventHandler<SensorChangedEventArgs> OnChanged;
    }
}
