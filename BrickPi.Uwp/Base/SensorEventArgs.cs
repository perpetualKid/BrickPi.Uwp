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

    public class AccelerationSensorChangedEventArgs: SensorChangedEventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class IRSeekerSensorChangedEventArgs: SensorChangedEventArgs
    {
        public int ACDirection { get; set; }
        public int DCDirection { get; set; }
        public int DCMean { get; set; }
    }

    public class GyroSensorChangedEventArgs: SensorChangedEventArgs
    {
        public int AbsoluteRotationRate { get { return Math.Abs(DirectionalRotationRate); } }
        public int DirectionalRotationRate { get; set; }
        public RotationDirection Direction { get { return (RotationDirection)Math.Sign(DirectionalRotationRate); } }
    }

    public class AngleSensorChangedEventArgs: SensorChangedEventArgs
    {
        public int Angle { get; set; }
        public long AccumulatedAngle { get; set; }
        public int RPM { get; set; }
    }

    public class CompassSensorChangedEventArgs: SensorChangedEventArgs
    {
        public int Heading { get; set; }
    }

    public interface IPropertyChangedEvent
    {
        event EventHandler<SensorChangedEventArgs> OnChanged;
    }
}
