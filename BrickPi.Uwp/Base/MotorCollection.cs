using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrickPi.Uwp.Motors;

namespace BrickPi.Uwp.Base
{
    public class MotorCollection : IEnumerable<Motor>
    {
        private Motor[] motors;
        private Brick brick;

        public MotorCollection(Brick brick)
        {
            this.brick = brick;
            motors = new Motor[4];
        }

        public Motor this[MotorPort port]
        {
            get { return motors[(int)port]; }
            internal set { motors[(int)port] = value; }
        }

        public Motor this[ArduinoTarget arduino, ArduinoPort port]
        {
            get { return motors[((int)arduino - 1) * 2 + (int)port]; }
            internal set { motors[((int)arduino - 1) * 2 + (int)port] = value; }
        }

        public IEnumerator<Motor> GetEnumerator()
        {
            foreach (Motor motor in motors)
                yield return motor;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
