using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.Sensors.Hitechnic
{
    [Flags]
    public enum SMUX_Status
    {
        Normal  = 0x0,       //Nothing going on, everything's fine
        LowBat  = 1 << 0,    //Low batttery voltage or no battery 
        Busy    = 1 << 1,      //Auto-dected in progress
        Halt    = 1 << 2,      //Multiplexer is halted
        Error   = 1 << 3,     //Command error detected
    }

    public class HiTechnicSensorMultiplexer: I2CSensor
    {
        const byte HTSMUX_I2C_ADDR = 0x10;      //Device address
        const byte HTSMUX_CMD_REG = 0x20;       //Command register
        const byte HTSMUX_STATUS = 0x21;        //Status register

        const byte HTSMUX_STATUS_PORT1 = HTSMUX_STATUS + 1;        //Port 1 Status register
        const byte HTSMUX_STATUS_PORT2 = HTSMUX_STATUS + 6;        //Port 2 Status register
        const byte HTSMUX_STATUS_PORT3 = HTSMUX_STATUS + 11;       //Port 3 Status register
        const byte HTSMUX_STATUS_PORT4 = HTSMUX_STATUS + 16;       //Port 4 Status register

        const byte HTSMUX_CMD_HALT = 0x00;
        const byte HTSMUX_CMD_AUTODETECT = 0x01;
        const byte HTSMUX_CMD_RUN = 0x02;

        const byte HTSMUX_CHAN_NONE = 0x00;
        const byte HTSMUX_CHAN_DIG0_HIGH = 0x04;

        private DateTime requestTimeout;
        private SMUX_Status status;

        public HiTechnicSensorMultiplexer(SensorPort sensorPort): base(sensorPort)
        {
            SensorType = SensorType.I2C;
            Speed = 5;
            I2CData = new I2CDataCollection(1);

            foreach (I2CData device in this.I2CData)
            {
//                device.Setting = Const.BIT_I2C_MID;
                device.Address = HTSMUX_I2C_ADDR;
            }

            //I2CData[0].Setting = Const.BIT_I2C_MID;
            //I2CData[0].Address = HTSMUX_I2C_ADDR;

            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTSMUX_CMD_REG;
            I2CData[0].WriteData[1] = HTSMUX_CMD_HALT;

            ReadStatus();
        }

        public SMUX_Status Status { get { return this.status; } }

        public void ReadStatus()
        {
            requestAction = ReadStatusRequest;
            responseAction = ReadStatusResponse;
        }

        public void AutoDetect()
        {
            if (!status.HasFlag(SMUX_Status.Halt))
                throw new InvalidOperationException("Multiplexer needs to be in HALTED state first");

            requestAction = SetAutoDetectRequest;
            responseAction = null;
        }

        public void Run()
        {
            if (!status.HasFlag(SMUX_Status.Halt))
                throw new InvalidOperationException("Multiplexer needs to be in HALTED state first");

            requestAction = SetRunningRequest;
            responseAction = null;
        }

        public void Halt()
        {
            requestAction = SetHaltRequest;
            responseAction = null;
        }

        private void FollowUpResponseRequest(SensorActionDelegate followUpRequest)
        {
        }

        private void SetAutoDetectRequest(ProtocolArray request)
        {
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTSMUX_CMD_REG;
            I2CData[0].WriteData[1] = HTSMUX_CMD_AUTODETECT;

            this.requestAction = EndAutoDetectRequest;
            this.responseAction = ReadStatusResponse;
            this.requestTimeout = DateTime.UtcNow.AddMilliseconds(500);
        }

        private void EndAutoDetectRequest(ProtocolArray request)
        {
            if (DateTime.UtcNow > requestTimeout)
            {
                this.requestAction = SetHaltRequest;
            }
            else
            {
                ReadStatusRequest(request);
            }
        }

        private void SetHaltRequest(ProtocolArray request)
        {
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTSMUX_CMD_REG;
            I2CData[0].WriteData[1] = HTSMUX_CMD_HALT;

            this.requestAction = EndHaltRequest;
            this.responseAction = ReadStatusResponse;
            this.requestTimeout = DateTime.UtcNow.AddMilliseconds(50);
        }

        private void EndHaltRequest(ProtocolArray request)
        {
            if (DateTime.UtcNow > requestTimeout)
            {
                this.requestAction = ReadStatusRequest;
                this.responseAction = ReadStatusResponse;
            }
            else
            {
                ReadStatusRequest(request);
            }
        }

        private void SetRunningRequest(ProtocolArray request)
        {
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTSMUX_CMD_REG;
            I2CData[0].WriteData[1] = HTSMUX_CMD_RUN;

            this.requestAction = ReadStatusRequest;
            this.responseAction = ReadStatusResponse;
        }

        private void ResponseAction(ProtocolArray response)
        {
        }

        private void ReadStatusRequest(ProtocolArray request)
        {
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 1;
            I2CData[0].WriteData[0] = HTSMUX_STATUS;

            //I2CData[1].WriteBytes = 1;
            //I2CData[1].ReadBytes = 5;
            //I2CData[1].WriteData[0] = HTSMUX_STATUS_PORT1;
            //I2CData[2].WriteBytes = 1;
            //I2CData[2].ReadBytes = 5;
            //I2CData[2].WriteData[0] = HTSMUX_STATUS_PORT2;
            //I2CData[3].WriteBytes = 1;
            //I2CData[3].ReadBytes = 5;
            //I2CData[3].WriteData[0] = HTSMUX_STATUS_PORT3;
            //I2CData[4].WriteBytes = 1;
            //I2CData[4].ReadBytes = 5;
            //I2CData[4].WriteData[0] = HTSMUX_STATUS_PORT4;
        }

        private void ReadPort4Request(ProtocolArray request)
        {
            I2CData[1].WriteBytes = 1;
            I2CData[1].ReadBytes = 5;
            I2CData[1].WriteData[0] = 0x31;
        }

        private void ReadPort4Response(ProtocolArray response)
        {
            byte mode = I2CData[1].ReadData[0];
            byte type = I2CData[1].ReadData[1];

            //if (mode != 0 && type > 1)
            //    base.OnChangedEventHandler(null);

            this.requestAction = ReadStatusRequest;
            this.responseAction = ReadStatusResponse;
        }

        private void ReadStatusResponse(ProtocolArray response)
        {
            if (I2CData[0].ReadData[0] != (byte)status)
            {
                status = (SMUX_Status)I2CData[0].ReadData[0];
                base.OnChangedEventHandler(null);

                //if (status.HasFlag(SMUX_Status.Normal))
                //{
                //    byte mode = I2CData[4].ReadData[0];
                //    byte type = I2CData[4].ReadData[1];

                //    //this.requestAction = ReadPort4Request;
                //    //this.responseAction = ReadPort4Response;
                //}
            }
        }
    }
}
