# BrickPi.Uwp

[Windows 10 IoT Core](https://developer.microsoft.com/en-us/windows/iot) [Universal Windows Platform (UWP)](https://msdn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide) on [Raspberry Pi](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) implementation for [Dexter BrickPi](http://www.dexterindustries.com/BrickPi/) board enabling [LEGO MINDSTORMS](http://www.lego.com/mindstorms/) 

### Getting Started


#### RaspberryPi setup

BrickPi requires a fixed non-standard Baud rate of 500.000 baud. As high speed onboard serial is not supported in early versions of Windows 10 IoT Core, you need to manually change the device registry and enable the highspeed serial. 
On the Raspberry Pi, open a console window and use the following command to enable highspeed serial. 

```CMD
Reg add hklm\system\controlset001\services\serpl011\parameters /v MaxBaudRateNoDmaBPS /t REG_DWORD /d 921600
Devcon restart acpi\bcm2837
```

Note: This only has to be done once, and should survive subsequent Windows Updates. If you reimage Windows on your Raspberry Pi to start from scratch, you may need to reapply above patch.

#### Setup of the project

The application will use the Background Application template for Windows IoT Core. 

 <img src="./media/image001.png" />

Once the project is created, you need to configure support for serial port capability. This can't be achieved through the UI editor, so you need to text edit the Package.appmanifest file and add (there will be an internetClient capability by default):

```XML
  <Capabilities>
	<Capability Name="internetClient" />
	<DeviceCapability Name="serialcommunication">
	  <Device Id="any">
		<Function Type="name:serialPort" />
	  </Device>
	</DeviceCapability>
  </Capabilities>
```

Add a reference to the BrickPi.Uwp library to your project (Nuget package TODO). First you need to implement a 
deferal for the background task instance

```C#
public sealed class StartupTask : IBackgroundTask
{
	BackgroundTaskDeferral deferal;

	public async void Run(IBackgroundTaskInstance taskInstance)
	{
		deferal = taskInstance.GetDeferral();
		//....
	}
}
```
More details about this can be found in the 
[Developing Background Applications](https://developer.microsoft.com/en-us/windows/iot/win10/backgroundapplications) guide.


### BrickPi

Next you need to get a reference to the BrickPi instance. The BrickPi is connected to an UART port on the Raspberry, which could be specified by name like "UART0", but as the current Raspberry Pi only have one UART available, this could be dropped and the first available UART is used


```C#
//Need a brick and a serial port
Brick brick = await Brick.InitializeInstance("Uart0");
```

To check basic comnunication with the BrickPi works, and see if a correct firmware version is installed on the BrickPi itself, call the `GetBrickVersion()` like this:
```C#
int version = await brick.GetBrickVersion();
Debug.WriteLine(string.Format("Brick Version: {0}", version));
```

The BrickPi should always return version **2**. You should always call `GetBrickVersion()` before you configure any sensors, as this resets any sensor information. This is due to the fact how the firmware version is queried internally by setting all sensors to a specific (non-existing) sensor type.

Internally, the BrickPi runs two Arduinos on the board. Each Arduino has a tiny blue LED connected, which could be controlled through corresponding GPIO-pins from the RaspBerry Pi. Arduino1 Led is controlled through GPIO port 18, corresponding to pin 12 on the RaspBerry Pi 40-pin header, and Arduino2 Led is controlled through GPIO port 27, corresponding to pin 13 on the RaspBerry Pi 40-pin header. Further details on GPIO pin mappings can be found at the [Raspberry Pi 2 & 3 Pin Mappings](https://developer.microsoft.com/en-us/windows/iot/win10/samples/pinmappingsrpi2) overview.

To use the LED in code, you can either set the status (ON or OFF) explicitely, or just toggle from current status

```C#
brick.Arduino1Led.Toggle();	//Just change status from ON to OFF or vice versa
brick.Arduino2Led.Light= true;	//explicitely set status to ON
```

### Motors

### Sensors

### Nuget Package

Planned, not yet created