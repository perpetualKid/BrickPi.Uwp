using System;
using System.Threading;

namespace BrickPi.Uwp.Base
{
    public static class Util
    {
        public static void SpinUntil(TimeSpan delay)
        {
            DateTime endTime = DateTime.UtcNow + delay;

            SpinWait.SpinUntil(() => DateTime.UtcNow.Ticks + delay.Ticks > endTime.Ticks);
        }
    }

    public static class EnumExtension<T>
    {
        public static T[] All()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }

    public static class BooleanExtension
    {
        public static int ToInt(this bool value)
        {
            return Convert.ToInt32(value);
        }
    }

    public static class TimeSpanExtension
    {
        public static TimeSpan? Max(this TimeSpan? current, TimeSpan? value)
        {
            if (current.HasValue)
            {
                if (value.HasValue)
                    return current.Value > value.Value ? current : value;
                return current;
            }
            return value;
        }
    }

}
