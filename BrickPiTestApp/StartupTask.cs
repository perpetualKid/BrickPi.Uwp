﻿#define COLOR

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BrickPi.Uwp;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Motors;
using BrickPi.Uwp.Sensors;
using BrickPi.Uwp.Sensors.Hitechnic;
using BrickPi.Uwp.Sensors.NXT;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BrickPiTestApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferal;
        //Need a brick and a serial port
        private Brick brick = null;

        private NXTTouchSensor touch;
        private NXTColorSensor color;
        private NXTUltraSonicSensor ultrasonic;
        private GpioPin ledPin;
        private Motor motorA;
        private Motor motorB;
        private Motor motorD;
        private const int LED_PIN = 47;
        private HiTechnicTouchMultiplexer multiTouch;
        //private HiTechnicCompassSensor compass;
        //private HiTechnicGyroSensor gyro;
        //private HiTechnicEOPDSensor eopd;
        //private HiTechnicAccelerationSensor accel;
        //private HiTechnicIRSeeker irSeeker;
        private HiTechnicSensorMultiplexer smux;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferal = taskInstance.GetDeferral();
            brick = await Brick.InitializeInstance("Uart0");
            InitGpio(brick);
            brick.Arduino1Led.Toggle();
            if (!await brick.InitializeSensors())
                Debug.WriteLine("Something went wrong initializing sensors");
            int version = await brick.GetBrickVersion();
            Debug.WriteLine(string.Format("Brick Version: {0}", version));
            bool timeoutSuccess = await brick.SetTimeout(200);
            Debug.WriteLine(string.Format("Setting timeout succesfully: {0}", timeoutSuccess));

            //irSeeker = new HiTechnicIRSeeker(SensorPort.Port_S1);
            //irSeeker.OnChanged += Sensor_OnChanged;
            //await brick.Sensors.Add(irSeeker, true);
            //accel = new HiTechnicAccelerationSensor(SensorPort.Port_S1);
            //accel.OnChanged += Sensor_OnChanged;
            //await brick.Sensors.Add(accel, true);
            //eopd = new HiTechnicEOPDSensor(SensorPort.Port_S1);
            //eopd.OnChanged += Sensor_OnChanged;
            //await brick.Sensors.Add(eopd, true);
            //gyro = new HiTechnicGyroSensor(SensorPort.Port_S1);
            //gyro.OnChanged += Sensor_OnChanged;
            //await brick.Sensors.Add(gyro, true);
            //compass = new HiTechnicCompassSensor(SensorPort.Port_S1);
            //compass.OnChanged += Sensor_OnChanged;
            //await brick.Sensors.Add(compass, true);

            smux = new HiTechnicSensorMultiplexer(SensorPort.Port_S1);
            smux.OnChanged += Smux_OnChanged;
            await brick.Sensors.Add(smux, true);
            //multiTouch = new HiTechnicTouchMultiplexer(SensorPort.Port_S1);
            //multiTouch.OnChanged += MultiTouch_OnChanged;
            //await brick.Sensors.Add(multiTouch);

            //touch = new NXTTouchSensor(SensorPort.Port_S1, SensorType.TOUCH_DEBOUNCE);
            //touch.OnPressed += Touch_OnPressed;
            //touch.OnReleased += Touch_OnReleased;
            //touch.OnChanged += Touch_OnChanged;
            //await brick.Sensors.Add(touch, true);

            //ultrasonic = new NXTUltraSonicSensor(SensorPort.Port_S2, SensorType.ULTRASONIC_CONT);
            //ultrasonic.ChangeEventThreshold = 5;
            //await brick.Sensors.Add(ultrasonic, true);

#if COLOR
            color = new NXTColorSensor(SensorPort.Port_S4, SensorType.COLOR_FULL);
            await brick.Sensors.Add(color, true);
            color = new NXTColorSensor(SensorPort.Port_S3, SensorType.COLOR_FULL);
            color.OnChanged += Sensor_OnChanged;
            await brick.Sensors.Add(color, true);
#else
            color = new NXTColorSensor(SensorPort.Port_S3, SensorType.COLOR_RED);
            await brick.Sensors.Add(color, true);
#endif
            //            color.PropertyChanged += Color_PropertyChanged;
            if (!await brick.InitializeSensors())
                Debug.WriteLine("Something went wrong initializing sensors");

            brick.Start();

            //while (true)
            //{
            //    //                await brick.UpdateValues();
            //    Debug.WriteLine($"Voltage {brick.Voltage}V");
            //    await Task.Delay(1000);
            //}

            //motorA.SetTachoCount(motorA.GetTachoCount());
            //await brick.Stop();
            smux.Halt();
            await Task.Delay(5000);

            //Debug.WriteLine("Setting Angle Sensor zero");
            //angle.ResetAccumulatedAngle();

            smux.AutoDetect();
            await Task.Delay(5000);
            smux.Run();
            await Task.Delay(10000);
            smux.Halt();
            //Debug.WriteLine("Starting Compass Calibration");
            //compass.BeginCompassCalibration();
            //for (int j = 0; j < 4; j++)
            //{
            //    if (j % 2 == 0)
            //        Debug.WriteLine("Change Direction");
            //    for (int i = 0; i < 20; i++)
            //    {
            //        Debug.WriteLine($"Turn {j} Step {i}");
            //        await Task.Delay(1000);
            //    }
            //}
            //compass.EndCompassCalibration();
            //Debug.WriteLine("Done Compass Calibration");

        }

        private void Smux_OnChanged(object sender, SensorChangedEventArgs e)
        {
            Debug.WriteLine($"SMux:{smux.Status}");
        }

        private void Sensor_OnChanged(object sender, SensorChangedEventArgs e)
        {
            //            Debug.WriteLine($"AC:{irSeeker.ACDirection} DC:{irSeeker.DCDirection} DC Mean:{irSeeker.DCMean} ACValues:{irSeeker.ACValues} DC Values:{irSeeker.DCValues}");
        }

        private void MultiTouch_OnChanged(object sender, SensorChangedEventArgs e)
        {
            TouchMultiplexerChangedEventArgs data = (e as TouchMultiplexerChangedEventArgs);
            Debug.WriteLine($"Pressed {data.PressedButtons.HasFlag(MultiplexerButtons.Button1)} {data.PressedButtons.HasFlag(MultiplexerButtons.Button2)} {data.PressedButtons.HasFlag(MultiplexerButtons.Button3)} {data.PressedButtons.HasFlag(MultiplexerButtons.Button4)}");
            Debug.WriteLine($"Changed {data.ChangedButtons.HasFlag(MultiplexerButtons.Button1)} {data.ChangedButtons.HasFlag(MultiplexerButtons.Button2)} {data.ChangedButtons.HasFlag(MultiplexerButtons.Button3)} {data.ChangedButtons.HasFlag(MultiplexerButtons.Button4)}");
        }

        private void Touch_OnReleased(object sender, SensorChangedEventArgs e)
        {
            //motorA.Velocity = 0;
            //motorB.Velocity = 0;
            //motorD.Velocity = 0;
            motorA.Enabled = false;
            motorB.Enabled = false;
            motorD.Enabled = false;

        }

        private void Touch_OnPressed(object sender, SensorChangedEventArgs e)
        {
            motorA.Velocity = -50;
            motorA.Enabled = true;
            motorB.Velocity = 100;
            motorB.Enabled = true;
            motorD.Velocity = -100;
            motorD.Enabled = true;
        }


        private void Touch_OnChanged(object sender, SensorChangedEventArgs e)
        {
            //            ledPin.Write(touch.IsPressed() ? GpioPinValue.High : GpioPinValue.Low);
            Debug.WriteLine(string.Format("NXT US, Distance: {0}, ", ultrasonic.Distance));
#if COLOR
            Debug.WriteLine(string.Format("NXT Color: {0}, RGB: {1}", color.ColorName, color.ColorData.ToString()));
#else
            Debug.WriteLine(string.Format("NXT Color, Raw: {0}, RGB: {1}", color.RawValue, color.ColorData.ToString()));
#endif
            if ((e as TouchSensorChangedEventArgs).Pressed)
            {
                Debug.WriteLine("Motor A Encoder: {0} Motor B Encoder: {1} Motor D Encoder: {2}", motorA.Encoder, motorB.Encoder, motorD.Encoder);
                brick.Arduino2Led.Light = true;
                brick.Arduino1Led.Toggle();
                //motorA.EncoderOffset = motorA.Encoder;
            }
            else
            {
                brick.Arduino2Led.Light = false;
            }
        }

        private void InitGpio(Brick brick)
        {
            GpioController gpioController = GpioController.GetDefault();
            if (null == gpioController)
                return;
            ledPin = gpioController.OpenPin(LED_PIN);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);
            ledPin.Write(GpioPinValue.High);

            motorA = brick.Motors[MotorPort.Port_MA];
            motorB = brick.Motors[MotorPort.Port_MB];
            motorD = brick.Motors[MotorPort.Port_MD];
        }

    }
}
