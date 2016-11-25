using System;

namespace VspWS.Common
{
    public class DelayGenerator
    {
        private int _maximumMillisecondDelay;

        public DelayGenerator(int maximumMillisecondDelay)
        {
            _maximumMillisecondDelay = maximumMillisecondDelay;
        }

        public int Milliseconds
        {
            get
            {
                var delay = new Random(DateTime.Now.Millisecond).Next();
                return Math.Min(delay, _maximumMillisecondDelay);
            }
        }
    }
}
