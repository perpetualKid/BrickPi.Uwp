using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using BrickPi.Uwp.Sensors;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BrickPi.Uwp.UnitTests
{
    [TestClass]
    public class SensorCollectionTest
    {
        [TestMethod()]
        public void SimpleEnumeratorTest()
        {
            SensorCollection collection = new SensorCollection(null);
            Assert.IsNull(collection[Arduino.Arduino1, ArduinoPort.Port1]);
        }

        [TestMethod()]
        public void SimpleEnumerationTest()
        {
            SensorCollection collection = new SensorCollection(null);
            foreach(RawSensor sensor in collection)
                Assert.IsNull(sensor);
        }

    }
}
