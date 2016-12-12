using System;

namespace BrickPi.Uwp.Base
{
    public sealed class ARGBColor
    {
        private int[] data = new int[4];

        public ARGBColor(int ambient, int red, int green, int blue)
        {
            data[0] = red;
            data[1] = green;
            data[2] = blue;
            data[3] = ambient;
        }

        public int Red { get { return data[0]; } }

        public int Green { get { return data[1]; } }

        public int Blue { get { return data[2]; } }

        public int Ambient { get { return data[3]; } }

        public override string ToString()
        {
            return $"{data[0]}-{data[1]}-{data[2]}-{data[3]}";
        }
    }

}
