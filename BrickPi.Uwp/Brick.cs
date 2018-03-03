using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Motors;
using BrickPi.Uwp.Sensors;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.SerialCommunication;

namespace BrickPi.Uwp
{
    public sealed partial class Brick
    {
        private static Brick instance;

        private SerialDevice serialPort;
        private I2cDevice mcp3021Sensor;
        private SensorCollection sensors;
        private MotorCollection motors;
        private BrickLed arduino1Led;
        private BrickLed arduino2Led;

        #region .ctor
        /// <summary>
        /// Initialize the brick and setup the serial port
        /// </summary>
        private Brick()
        {
            sensors = new SensorCollection(this);
            motors = new MotorCollection(this);
            foreach (SensorPort sensorPort in Enum.GetValues(typeof(SensorPort)))
            {
                //create new sensors
                sensors[sensorPort] = new RawSensor(sensorPort);
            }
            foreach (MotorPort motorPort in Enum.GetValues(typeof(MotorPort)))
            {
                motors[motorPort] = new Motor(motorPort);
            }

            handlerCancellationTokenSource = new CancellationTokenSource();

        }

        /// <summary>
        /// Initialize the brick and setup the serial port
        /// </summary>
        /// <param name="serialDevice">serial port to use</param>
        private Brick(SerialDevice serialDevice): this()
        {
            this.serialPort = serialDevice;
        }

        public static async Task<Brick> InitializeInstance()
        {
            return await InitializeInstance(string.Empty).ConfigureAwait(false);
        }

        public static async Task<Brick> InitializeInstance(string portName)
        {
            SerialDevice serialDevice = await FindSerialDevice(portName).ConfigureAwait(false);
            return await InitializeInstance(serialDevice).ConfigureAwait(false);
        }

        public static async Task<Brick> InitializeInstance(SerialDevice serialDevice)
        {
            instance = new Brick(serialDevice);
            Task configSerialTask = instance.ConfigureSerialPort();
            Task<GpioController> gpioControllerTask = GpioController.GetDefaultAsync().AsTask<GpioController>();
            Task configVoltage = instance.InitializeVoltageReader();
            await Task.WhenAll(configSerialTask, gpioControllerTask, configVoltage);

            instance.arduino1Led = new BrickLed(gpioControllerTask.Result, Arduino.Arduino1);
            instance.arduino2Led = new BrickLed(gpioControllerTask.Result, Arduino.Arduino2);
            return instance;
        }
        #endregion

        #region Serial Port
        public static async Task<SerialDevice> FindSerialDevice()
        {
            return await FindSerialDevice(null);
        }

        public static async Task<SerialDevice> FindSerialDevice(string portName)
        {
            string deviceQueryString = SerialDevice.GetDeviceSelector();
            DeviceInformationCollection deviceCollection = await DeviceInformation.FindAllAsync(deviceQueryString);
            SerialDevice serialPort = null;

            if (deviceCollection == null || deviceCollection.Count == 0)
                return serialPort;

            //if multiple serial found, use a filter
            Debug.WriteLine($"# of Serial devices found: {deviceCollection.Count}");
            if (deviceCollection.Count > 0 && !string.IsNullOrEmpty(portName))
            {
                foreach (DeviceInformation device in deviceCollection)
                {
                    serialPort = await SerialDevice.FromIdAsync(device.Id);                        ;
                    if (portName.ToLower().Equals(serialPort.PortName.ToLower()))
                    {
                        Debug.WriteLine($"Using Serial port {serialPort.PortName} @ {device.Name}");
                        return serialPort;
                    }
                }
                return null;
            }
            //if just one serial, or no filter, use the first one
            serialPort = await SerialDevice.FromIdAsync(deviceCollection[0].Id);
            Debug.WriteLine($"Using Serial port {serialPort.PortName} @ {deviceCollection[0].Name}");
            return serialPort;
        }

        private async Task ConfigureSerialPort()
        {
            try
            {
                if (this.serialPort != null)
                {
                    serialPort.BaudRate = 500000; //communication speed with the 2 arduinos.
                    // you may need to do the following steps into your RaspberryPi rinnong Windows IoT core:
                    //Reg add hklm\system\controlset001\services\serpl011\parameters /v MaxBaudRateNoDmaBPS /t REG_DWORD /d 921600
                    //Devcon restart acpi\bcm2837

                    serialPort.Parity = SerialParity.None;
                    serialPort.StopBits = SerialStopBitCount.One;
                    serialPort.DataBits = 8;

                    serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception initializing Serial port: {ex.Message}");
            }
        }

        /// <summary>
        /// Debug only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SerialPort_ErrorReceived(SerialDevice sender, ErrorReceivedEventArgs args)
        {
            Debug.WriteLine($"Serial port error {args.Error}");
        }
        #endregion

        #region Voltage
        private async Task InitializeVoltageReader()
        {
            try
            {
                //// 0x48 is address for the I2C Connection using the MCP3021 sensor connected to the BrickPi+
                DeviceInformation i2cController = (await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector("I2C1")))[0];
                mcp3021Sensor = await I2cDevice.FromIdAsync(i2cController.Id, new I2cConnectionSettings(0x48));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception initializing MCP3021 Voltage Chip: {ex.Message}");
            }
        }
        #endregion

        public static Brick Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = InitializeInstance().Result;
                }
                return instance;
            }
        }

        public SensorCollection Sensors { get { return this.sensors; } }

        public MotorCollection Motors { get { return this.motors; } }

        public BrickLed Arduino1Led { get { return arduino1Led; } }

        public BrickLed Arduino2Led { get { return arduino2Led; } }

        public float Voltage
        {
            /* 
             * The ten-bit output code of MCP3021Sensor is composed of the lower 4-bit of the first byte and the upper 6-bit of the second byte. 
             * The chip has a 3.3V reference and there is a 45k/10k voltage divider, so value of 1024 represents 18.15V. 
             */
            get
            {
                if (mcp3021Sensor != null)
                {
                    byte[] buffer = new byte[2];
                    mcp3021Sensor.Read(buffer);
                    return (float)Math.Round((((buffer[0] & 0b1111) << 6) | (buffer[1] >> 2)) * 0.01815, 2);

                }
                return float.NaN;
            }
        }


    }

}
