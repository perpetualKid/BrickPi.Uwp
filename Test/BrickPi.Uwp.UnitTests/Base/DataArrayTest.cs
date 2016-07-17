using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;

namespace BrickPi.Uwp.UnitTests.Base
{
    [TestClass]
    public class DataArrayTest
    {
        [TestMethod]
        public void DataArrayPositionInitialTest()
        {
            ProtocolArray array = new ProtocolArray();
            Assert.AreEqual(0, array.Position);
        }

        [TestMethod]
        public void DataArrayPositionAfterAddNoOffsetTest()
        {
            ProtocolArray array = new ProtocolArray();
            array.SetBits(0, 4, 123456);
            Assert.AreEqual(4, array.Position);
        }

        [TestMethod]
        public void DataArrayPositionAfterAddWithOffSetTest()
        {
            ProtocolArray array = new ProtocolArray();
            array.SetBits(1, 4, 123456);
            Assert.AreEqual(4, array.Position);
        }

        [TestMethod]
        public void DataArrayPositionAfterGetNoOffsetTest()
        {
            ProtocolArray array = new ProtocolArray();
            array.GetBits(0, 4);
            Assert.AreEqual(4, array.Position);
        }

        [TestMethod]
        public void DataArrayPositionAfterGetWithOffSetTest()
        {
            ProtocolArray array = new ProtocolArray();
            array.GetBits(1, 4);
            Assert.AreEqual(4, array.Position);
        }

        [TestMethod]
        public void DataArraySignificantBitsTestMaxInt()
        {
            Assert.AreEqual(31, ProtocolArray.SignificantBits(int.MaxValue));
        }

        [TestMethod]
        public void DataArraySignificantBitsTestMaxUInt()
        {
            Assert.AreEqual(32, ProtocolArray.SignificantBits(uint.MaxValue));
        }

        [TestMethod]
        public void DataArraySignificantBitsTest0()
        {
            Assert.AreEqual(0, ProtocolArray.SignificantBits(uint.MinValue));
        }

        [TestMethod]
        public void DataArrayAddBitsCorrectRange()
        {
            uint value = 15;
            ProtocolArray array = new ProtocolArray();
            array.SetBits(0, 4, value);
            Assert.AreEqual(value, BitConverter.ToUInt32(array.Data, 0));
        }

        [TestMethod]
        public void DataArrayAddBitsRangeOverflow()
        {
            uint value = 16;
            ProtocolArray array = new ProtocolArray();
            array.SetBits(0, 4, value);
            Assert.AreNotEqual(value, BitConverter.ToInt32(array.Data, 0));
        }

        [TestMethod()]
        public void DataArraySetGetSingleBitTest()
        {
            ProtocolArray array = new ProtocolArray(8);
            array.SetBits(0, 1, 1);
            Assert.AreEqual(1, BitConverter.ToInt32(array.Data, 0));
            array.Position = 0;
            Assert.AreEqual(1, (int)array.GetBits(0, 1));
        }

        [TestMethod]
        public void DataArrayAddMultipleBitsCorrectRange2()
        {
            ProtocolArray array = new ProtocolArray();
            array.SetBits(0, 1, 1);
            array.SetBits(0, 5, 20);
            Assert.AreEqual(41, BitConverter.ToInt32(array.Data, 0));
        }

        [TestMethod]
        public void DataArrayAddMultipleBitsCorrectRange()
        {
            //byte offset = 3;
            for (byte offset = 1; offset < 16; offset++)
            {
                UInt64 result = 0;
                ProtocolArray array = new ProtocolArray();
                for (int i = 0; i < 8; i++)
                {
                    array.SetBits(0, offset, 1);
                    //array.AddBits(0, 0, offset, 1);
                    result += ((ulong)1 << (offset * i));
                    Assert.AreEqual(result, BitConverter.ToUInt64(array.Data, 0), string.Format("Failed at offset {0} in step {1}", offset, i));
                }
            }
        }


        [TestMethod]
        public void DataArrayGetBits()
        {
            ProtocolArray array = new ProtocolArray(new byte[] { 165, 189, 219, 231});   //10100101, 10111101, 11011011, 11100111

            Assert.AreEqual(array.GetBits(0, 4), (uint)5);
            Assert.AreEqual(array.GetBits(0, 4), (uint)10);
            Assert.AreEqual(array.GetBits(0, 4), (uint)13);
            Assert.AreEqual(array.GetBits(0, 4), (uint)11);
            Assert.AreEqual(array.GetBits(0, 4), (uint)11);
            Assert.AreEqual(array.GetBits(0, 4), (uint)13);
            Assert.AreEqual(array.GetBits(0, 4), (uint)7);
            Assert.AreEqual(array.GetBits(0, 4), (uint)14);

            array.Position = 0;
            Assert.AreEqual(array.GetBits(0, 10), (uint)421);
            Assert.AreEqual(array.GetBits(0, 10), (uint)751);
            Assert.AreEqual(array.GetBits(0, 10), (uint)637);
            Assert.AreEqual(array.GetBits(0, 2), (uint)3);
        }


        [TestMethod]
        public void DataArrayAddGetBits()
        {
            ProtocolArray array = new ProtocolArray();
            array.SetBits(0, 10, 421);
            array.SetBits(0, 10, 751);
            array.SetBits(0, 10, 637);
            array.SetBits(0, 2, 3);
            Assert.AreEqual(32, array.Position);

            array.Position = 0;
            Assert.AreEqual(array.GetBits(0, 4), (uint)5);
            Assert.AreEqual(array.GetBits(0, 4), (uint)10);
            Assert.AreEqual(array.GetBits(0, 4), (uint)13);
            Assert.AreEqual(array.GetBits(0, 4), (uint)11);
            Assert.AreEqual(array.GetBits(0, 4), (uint)11);
            Assert.AreEqual(array.GetBits(0, 4), (uint)13);
            Assert.AreEqual(array.GetBits(0, 4), (uint)7);
            Assert.AreEqual(array.GetBits(0, 4), (uint)14);

            array.Position = 0;
            Assert.AreEqual(array.GetBits(0, 10), (uint)421);
            Assert.AreEqual(array.GetBits(0, 10), (uint)751);
            Assert.AreEqual(array.GetBits(0, 10), (uint)637);
            Assert.AreEqual(array.GetBits(0, 2), (uint)3);

            Assert.AreEqual(array[0], 165);
            Assert.AreEqual(array[1], 189);
            Assert.AreEqual(array[2], 219);
            Assert.AreEqual(array[3], 231);
        }

    }
}
