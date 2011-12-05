using System;

namespace EasyNetQ.SystemMessages
{
    [Serializable]
    public class ScheduleMeRepetitive : ScheduledMessage, IAssociable
    {
        public DateTimeOffset WakeTime { get; set; }
        
        private int _numberOfRepetitions;
        private TimeSpan _repetitionInterval;

        public string Name { get; set; }
        public string Group { get; set; }

        public int NumberOfRepetitions
        {
            get { return _numberOfRepetitions; }
            set
            {
                if (_numberOfRepetitions < -1)
                    throw new ArgumentOutOfRangeException("value", "Invalid NumberOfRepetitions!");
                _numberOfRepetitions = value;
            }
        }

        public TimeSpan RepetitionInterval
        {
            get { return _repetitionInterval; }
            set
            {
                if (value.TotalMilliseconds < 1)
                    throw new ArgumentOutOfRangeException("value", "Invalid RepetitionInterval!");

                _repetitionInterval = value;
            }
        }
    }
}