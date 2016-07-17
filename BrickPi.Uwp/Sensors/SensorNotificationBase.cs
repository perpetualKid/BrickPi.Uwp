using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace BrickPi.Sensors
{
    public sealed class SensorNotificationBase
    {
        Timer timer;

        public SensorNotificationBase(TimerCallback callback) : this(callback, 1000)
        {
        }

        public SensorNotificationBase(TimerCallback callback, int miliseconds)
        {
            InitializeTimer(miliseconds, callback);
        }

        public void StopSendingNotifications()
        {
            StopTimerInternal();
        }

        public void InitializeTimer(int miliseconds, TimerCallback callback)
        {
            StopTimerInternal();
            timer = new Timer(callback, this, TimeSpan.FromMilliseconds(miliseconds), TimeSpan.FromMilliseconds(miliseconds));

        }

        private void StopTimerInternal()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }


        private int value;
        private string valueAsString;

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        { get { return value; }
            set {
                if (value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged(nameof(Value));
                } } }

        /// <summary>
        /// Return the raw value  as a string of the sensor
        /// </summary>
        public string ValueAsString
        { get { return valueAsString; }
            set {
                if (valueAsString != value)
                {
                    valueAsString = value;
                    OnPropertyChanged(nameof(ValueAsString));
                } } }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
