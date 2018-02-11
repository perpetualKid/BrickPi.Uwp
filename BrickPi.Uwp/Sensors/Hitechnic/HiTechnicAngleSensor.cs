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

        const int HTANG_I2C_ADDR = 0x02;      /*!< HTANG I2C device address */
        const int HTANG_CMD_REG = 0x41;      /*!< Command register */
        const int HTANG_OFFSET = 0x42;      /*!< Offset for data registers */

        // Values contained by registers in active mode
        const int HTANG_ANG2 = 0x00;      /*!< Current angle (2 deg increments) */
        const int HTANG_ANG1 = 0x01;      /*!< Current angle (1 deg adder) */
        const int HTANG_ACC_ANG_B4 = 0x02;      /*!< 32 bit accumulated angle 4th byte */
        const int HTANG_ACC_ANG_B3 = 0x03;      /*!< 32 bit accumulated angle 3rd byte */
        const int HTANG_ACC_ANG_B2 = 0x04;      /*!< 32 bit accumulated angle 2nd byte */
        const int HTANG_ACC_ANG_B1 = 0x05;      /*!< 32 bit accumulated angle 1st byte */
        const int HTANG_RPM_H = 0x06;      /*!< 16 bit rpms, high byte */
        const int HTANG_RPM_L = 0x07;      /*!< 16 bit rpms, low byte */

        // Various commands
        const int HTANG_CMD_MEASURE = 0x00;      /*!< Normal angle measurement mode */
        const int HTANG_CMD_RST_ANG = 0x43;      /*!< Resets 0 position to current shaft angle, non-volatile setting */
        const int HTANG_CMD_RST_ACC_ANG = 0x52;      /*!< Resets the accumulated angle */

        public int Angle { get; private set; }

        public long AccumulatedAngle { get; private set; }

        public int RPM { get; private set; }

        public HiTechnicAngleSensor(SensorPort sensorPort): base(sensorPort)
        {
            SensorType = SensorType.I2C;
            I2CData = new I2CDataCollection(1);

            I2CData[0].Setting = Const.BIT_I2C_MID;
            I2CData[0].Address = HTANG_I2C_ADDR;
            I2CData[0].WriteBytes = 2;
            I2CData[0].ReadBytes = 0;
            I2CData[0].WriteData[0] = HTANG_CMD_REG;
            I2CData[0].WriteData[0] = HTANG_CMD_MEASURE;
        }

        public override TimeSpan? InitializeSensorRequest(ProtocolArray requestData)
        {
            return base.InitializeSensorRequest(requestData);
        }


        public override void UpdateSensorRequest(ProtocolArray requestData)
        {
            I2CData[0].WriteBytes = 1;
            I2CData[0].ReadBytes = 8;
            I2CData[0].WriteData[0] = HTANG_OFFSET;

            base.UpdateSensorRequest(requestData);
        }

        public override void UpdateSensorResponse(ProtocolArray responseData)
        {
            // Populate the struct with the newly retrieved data
            base.UpdateSensorResponse(responseData);

            long accumulatedAngle = ((sbyte)I2CData[0].ReadData[2] << 24) + 
                ((byte)I2CData[0].ReadData[3] << 16) +
                ((byte)I2CData[0].ReadData[4] << 8) +
                I2CData[0].ReadData[5];

            if (accumulatedAngle != AccumulatedAngle)
            {
                Angle= ((int)I2CData[0].ReadData[0] << 1) + I2CData[0].ReadData[1];
                AccumulatedAngle = accumulatedAngle;
                RPM = ((sbyte)I2CData[0].ReadData[6] << 8) + I2CData[0].ReadData[7];

                base.OnChangedEventHandler(null);
            }
        }

    }
}
