namespace BrickPi.Uwp.Base
{
    /// <summary>
    /// All possible type of sensors
    /// </summary>
    public enum SensorType
    {
        // https://github.com/DexterInd/BrickPi_Python/blob/master/BrickPi.py
        RAW = 0,
        LIGHT_OFF = 0,
        LIGHT_ON = SensorTypeMask.D0_M | SensorTypeMask.D0_S,
        TOUCH = 32,
        ULTRASONIC_CONT = 33,
        ULTRASONIC_SS = 34,
        RCX_LIGHT = 35, //tested minimally
        COLOR_FULL = 36,
        COLOR_RED = 37,
        COLOR_GREEN = 38,
        COLOR_BLUE = 39,
        COLOR_NONE = 40,
        I2C = 41,
        I2C_9V = 42,

        EV3_US_M0 = 43, //Continuous measurement, distance, cm
        EV3_US_M1 = 44, //Continuous measurement, distance, in
        EV3_US_M2 = 45, //Listen // 0 r 1 depending on presence of another US sensor.
        EV3_US_M3 = 46,
        EV3_US_M4 = 47,
        EV3_US_M5 = 48,
        EV3_US_M6 = 49,

        EV3_COLOR_M0 = 50, //Reflected Light.  Shine against a surface to see the effect.
        EV3_COLOR_M1 = 51, //Ambient.  Detects ambient light, hold up to a bright light to see the effect, dark area to see the effect.
        EV3_COLOR_M2 = 52, //Color  // Min is 0, max is 7 (brown).  Returns a value for each color it sees
        EV3_COLOR_M3 = 53, //Raw reflected
        EV3_COLOR_M4 = 54, //Raw Color Components
        EV3_COLOR_M5 = 55, //Calibration???  Not currently implemented.

        EV3_GYRO_M0 = 56, //Angle
        EV3_GYRO_M1 = 57, //Rotational Speed
        EV3_GYRO_M2 = 58, //Raw sensor value ???
        EV3_GYRO_M3 = 59, //Angle and Rotational Speed?
        EV3_GYRO_M4 = 60, //Calibration ???

        EV3_INFRARED_M0 = 61, //Proximity, 0 to 100
        EV3_INFRARED_M1 = 62, //IR Seek, -25 (far left) to 25 (far right)
        EV3_INFRARED_M2 = 63, //IR Remote Control, 0 - 11
        EV3_INFRARED_M3 = 64,
        EV3_INFRARED_M4 = 65,
        EV3_INFRARED_M5 = 66,

        EV3_TOUCH_0 = 67,

        EV3_TOUCH_DEBOUNCE = 68, //EV3 Touch sensor, debounced.
        TOUCH_DEBOUNCE = 69, //	NXT Touch sensor, debounced.

        FW_VERSION = 70,    //Returns firmware version.
    }

}
