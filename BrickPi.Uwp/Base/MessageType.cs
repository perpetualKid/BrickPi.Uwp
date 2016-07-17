namespace BrickPi.Uwp.Base
{
    public enum MessageType : byte
    {
        ChangeAddress = 0x01,               // Change the UART address.
        ChangeSensorType = 0x02,            // Change/set the sensor type.
        Datagram = 0x03,                    // Set the motor speed and direction, and return the sensors and encoders data
        MotorStop = 0x04,                   // Float motors immediately
        ChangeTimeout = 0x05,               // Set the timeout

    }
}
