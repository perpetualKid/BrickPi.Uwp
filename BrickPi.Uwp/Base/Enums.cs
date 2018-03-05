namespace BrickPi.Uwp.Base
{
    public enum RotationDirection
    {
        Clockwise = 1,
        CounterClockwise = -1,
        None = 0,
    }

    public enum SensorSensitivity
    {
        Low,
        High,
    }

    public enum SensorRange
    {
        Short,
        Long,
    }

    [System.Flags]
    public enum MultiplexerButtons
    {
        None = 0,
        Button1 = 1 << 0,
        Button2 = 1 << 1,
        Button3 = 1 << 2,
        Button4 = 1 << 3,
    }


}
