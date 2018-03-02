using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    public sealed class HiTechnicAngleSensor : I2CSensor
    {

        const byte HTANG_I2C_ADDR = 0x02;        //Device address
        const byte HTANG_CMD_REG = 0x41;         //Command register
        const byte HTANG_OFFSET = 0x42;          //Offset for data registers

        //Commands
        const byte HTANG_CMD_MEASURE = 0x00;         //Normal angle measurement mode */
        const byte HTANG_CMD_RST_ANG = 0x43;         //Resets 0 position to current shaft angle, non-volatile setting
        const byte HTANG_CMD_RST_ACC_ANG = 0x52;     //Resets the accumulated angle

        private byte resetCommand;

        public int Angle { get; private set; }

        public long AccumulatedAngle { get; private set; }

        public int RPM { get; private set; }

        public HiTechnicAngleSensor(SensorPort sensorPort): base(sensorPort)
        {
            ChangeEventThreshold = 2;
            SensorType = SensorType.I2C;
            Speed = 5;
            I2CData = new I2CDataCollection(1);

            I2CData[0].Setting = Const.BIT_I2C_MID;
            I2CData[0].Address = HTANG_I2C_ADDR;
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTANG_CMD_REG;
            I2CData[0].WriteData[1] = HTANG_CMD_MEASURE;

            requestAction = this.ReadSensorDataRequest; //default action is Data read
            responseAction = this.ReadSensorDataResponse;
        }

        public override TimeSpan? InitializeSensorRequest(ProtocolArray requestData)
        {
            return base.InitializeSensorRequest(requestData);
        }


        public void SetZeroAngle()
        {
            resetCommand = HTANG_CMD_RST_ANG;
            requestAction = this.SetResetRequest;
        }

        public void ResetAccumulatedAngle()
        {
            resetCommand = HTANG_CMD_RST_ACC_ANG;
            requestAction = this.SetResetRequest;
        }

        private void ReadSensorDataRequest(ProtocolArray requestData)
        {
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 8;
            I2CData[0].WriteData[0] = HTANG_OFFSET;
        }

        private void ReadSensorDataResponse(ProtocolArray responseData)
        {
            // Populate the struct with the newly retrieved data
            long accumulatedAngle = ((sbyte)I2CData[0].ReadData[2] << 24) +
                ((byte)I2CData[0].ReadData[3] << 16) +
                ((byte)I2CData[0].ReadData[4] << 8) +
                I2CData[0].ReadData[5];

            if (Math.Abs(accumulatedAngle - AccumulatedAngle) >= ChangeEventThreshold)
            {
                Angle = ((int)I2CData[0].ReadData[0] << 1) + I2CData[0].ReadData[1];
                AccumulatedAngle = accumulatedAngle;
                RPM = ((sbyte)I2CData[0].ReadData[6] << 8) + I2CData[0].ReadData[7];

                base.OnChangedEventHandler(null);
            }
        }

        private void SetResetRequest(ProtocolArray requestData)
        {
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTANG_CMD_REG;
            I2CData[0].WriteData[1] = resetCommand;
            responseAction = this.SetZeroAngleResponse;
        }

        private void SetZeroAngleResponse(ProtocolArray responseData)
        {
            responseAction = this.ReadSensorDataResponse;
            requestAction = this.ReadSensorDataRequest;
        }

    }
}
