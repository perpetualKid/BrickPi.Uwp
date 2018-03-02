using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickPi.Uwp.Sensors
{
    public sealed class I2CData
    {
        public int Setting { get; set; }
        public int Address { get; set; }

        public int WriteBytes { get; set; }
        public int ReadBytes { get; set; }

        public byte[] ReadData = new byte[16];
        public byte[] WriteData = new byte[16];
    }

    public class I2CDataCollection : List<I2CData>
    {
        public I2CDataCollection() : this(8)
        {

        }

        public I2CDataCollection(int size)
        {
            for (int i = 0; i < size; i++)
            {
                this.Add(new I2CData());
            }
        }
    }

}
