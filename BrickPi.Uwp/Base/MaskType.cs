using System;

namespace BrickPi.Uwp.Base
{

    [Flags()]
    public enum SensorTypeMask
    {
        D0_M = 0x01,
        D1_M = 0x02,
        I2C_9V = 0x04,
        D0_S = 0x08,
        D1_S = 0x10,

    }
}
