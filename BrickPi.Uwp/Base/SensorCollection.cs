using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrickPi.Uwp.Sensors;

namespace BrickPi.Uwp.Base
{
    public class SensorCollection : IEnumerable<RawSensor>
    {
        private RawSensor[] sensors;
        private Brick brick;

        public SensorCollection(Brick brick)
        {
            this.brick = brick;
            sensors = new RawSensor[4];
        }

        public RawSensor this[SensorPort port]
        {
            get { return sensors[(int)port]; }
            internal set { sensors[(int)port] = value; }
        }

        public RawSensor this[ArduinoTarget arduino, ArduinoPort port]
        {
            get { return sensors[((int)arduino - 1) * 2 + (int)port]; }
            internal set { sensors[((int)arduino - 1) * 2 + (int)port] = value; }
        }

        public IEnumerator<RawSensor> GetEnumerator()
        {
            foreach (RawSensor sensor in sensors)
                yield return sensor;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public async Task Add(RawSensor sensor, bool deferInitialization = false)
        {
            this[sensor.SensorPort] = sensor;
            if (!deferInitialization)
            {
                await brick.InitializeSensors();
            }
        }

        public async Task Remove(SensorPort port, bool deferInitialization = false)
        {
            this[port] = new RawSensor(port);
            if (!deferInitialization)
            {
                await brick.InitializeSensors();
            }
        }
    }

}
