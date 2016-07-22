using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Motors;
using BrickPi.Uwp.Sensors;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;

namespace BrickPi.Uwp
{
    public sealed partial class Brick
    {
        private static Brick instance;

        private SerialDevice serialPort;

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
            Task configSerialTask = Task.Run(() => instance.ConfigureSerialPort());
            Task<GpioController> gpioControllerTask = GpioController.GetDefaultAsync().AsTask<GpioController>();
            await Task.WhenAll(configSerialTask, gpioControllerTask);

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
            Debug.WriteLine(string.Format("# of Serial devices found: {0}", deviceCollection.Count));
            if (deviceCollection.Count > 0 && !string.IsNullOrEmpty(portName))
            {
                foreach (DeviceInformation device in deviceCollection)
                {
                    serialPort = await SerialDevice.FromIdAsync(device.Id);                        ;
                    if (portName.ToLower().Equals(serialPort.PortName.ToLower()))
                    {
                        Debug.WriteLine(string.Format("Using Serial port {1} @ {0}", device.Name, serialPort.PortName));
                        return serialPort;
                    }
                }
                return null;
            }
            //if just one serial, or no filter, use the first one
            serialPort = await SerialDevice.FromIdAsync(deviceCollection[0].Id);
            Debug.WriteLine(string.Format("Using Serial port {1} @ {0}", deviceCollection[0].Name, serialPort.PortName));
            return serialPort;
        }

        private void ConfigureSerialPort()
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
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Exception initializing Serial port: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Debug only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SerialPort_ErrorReceived(SerialDevice sender, ErrorReceivedEventArgs args)
        {
            Debug.WriteLine(string.Format("Serial port error {0}", args.Error));
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


    }

}
