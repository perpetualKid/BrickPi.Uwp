namespace BrickPi.Uwp.Base
{
    public class Const
    {
        public const int MessageTypeIndex = 0;
        //# New UART address (MSG_TYPE_CHANGE_ADDR)
        public const int NewAddressIndex = 1;

        public const int US_I2C_SPEED = 10; //#tweak this value
        public const int US_I2C_IDX = 0;
        public const int LEGO_US_I2C_ADDR = 0x02;
        public const int LEGO_US_I2C_DATA_REG = 0x42;


        //# Sensor setup (MSG_TYPE_SENSOR_TYPE)
        public const int SensorIndex = 1;
        public const int TimeoutIndex = 1;

        //public int TYPE_MOTOR_PWM = 0;
        //public const int TYPE_MOTOR_SPEED = 1;
        //public const int TYPE_MOTOR_POSITION = 2;

        public const int BIT_I2C_MID = 0x01; //  # Do one of those funny clock pulses between writing and reading. defined for each device.
        public const int BIT_I2C_SAME = 0x02; //  # The transmit data, and the number of bytes to read and write isn't going to change. defined for each device.

    }
}
