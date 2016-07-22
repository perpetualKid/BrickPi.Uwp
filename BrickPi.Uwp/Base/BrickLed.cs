using Windows.Devices.Gpio;

namespace BrickPi.Uwp.Base
{
    public sealed class BrickLed
    {
        private Arduino arduino;
        private GpioPin gpioPin;

        internal BrickLed(GpioController gpioController, Arduino arduino)
        {
            this.arduino = arduino;
            if (null != gpioController)
            {
                switch (arduino)
                {
                    case Arduino.Arduino1:
                        gpioPin = gpioController.OpenPin(18);
                        break;
                    case Arduino.Arduino2:
                        gpioPin = gpioController.OpenPin(27);
                        break;
                }
                gpioPin.SetDriveMode(GpioPinDriveMode.Output);
            }
            Light = false;
        }

        public bool Light
        {
            get { return gpioPin != null ? (gpioPin.Read() == GpioPinValue.High) : false; }
            set { if (gpioPin != null) gpioPin.Write(value ? GpioPinValue.High : GpioPinValue.Low);  }
        }

        public void Toggle()
        {
            this.Light = !this.Light;
        }

        public Arduino Arduino
        {
            get { return this.arduino; }
        }
    }
}
