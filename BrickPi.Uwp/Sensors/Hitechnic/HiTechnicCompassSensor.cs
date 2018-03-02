using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    public sealed class HiTechnicCompassSensor : I2CSensor
    {
        const byte HTMC_I2C_ADDR = 0x02; //Device address
        const byte HTMC_CMD_REG = 0x41;         //Command register
        const byte HTMC_OFFSET = 0x42;          //Offset for data registers


        //Commands
        const byte HTMC_CMD_MEASURE = 0x00;         //Normal measurement mode */
        const byte HTMC_CMD_CALIBRATE = 0x43;         //Calibration mode

        public int Heading { get; private set; }

        public HiTechnicCompassSensor(SensorPort sensorPort) : base(sensorPort)
        {
            ChangeEventThreshold = 2;
            SensorType = SensorType.I2C;
            Speed = 5;
            I2CData = new I2CDataCollection(1);

            I2CData[0].Setting = Const.BIT_I2C_MID;
            I2CData[0].Address = HTMC_I2C_ADDR;
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTMC_CMD_REG;
            I2CData[0].WriteData[1] = HTMC_CMD_MEASURE;

            requestAction = this.ReadSensorDataRequest; //default action is Data read
            responseAction = this.ReadSensorDataResponse;
        }

        public void BeginCompassCalibration()
        {
            requestAction = this.SetCalibrationRequest;
        }

        public void EndCompassCalibration()
        {
            requestAction = this.EndCalibrationRequest;
        }

        private void ReadSensorDataRequest(ProtocolArray requestData)
        {
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 2;
            I2CData[0].WriteData[0] = HTMC_OFFSET;
        }

        private void ReadSensorDataResponse(ProtocolArray responseData)
        {
            int heading = ((int)I2CData[0].ReadData[0] << 1) + I2CData[0].ReadData[1];

            if (Math.Abs(heading - Heading) >= ChangeEventThreshold)
            {
                Heading = heading;

                base.OnChangedEventHandler(null);
            }
        }

        private void SetCalibrationRequest(ProtocolArray request)
        {
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTMC_CMD_REG;
            I2CData[0].WriteData[1] = HTMC_CMD_CALIBRATE;

            responseAction = null;     
        }

        private void EndCalibrationRequest(ProtocolArray request)
        {
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTMC_CMD_REG;
            I2CData[0].WriteData[1] = HTMC_CMD_MEASURE;

            requestAction = this.ReadSensorDataRequest;
            responseAction = this.ReadSensorDataResponse;
        }

    }
}
