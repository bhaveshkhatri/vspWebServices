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
                return new Random(Guid.NewGuid().GetHashCode()).Next(0, _maximumMillisecondDelay);
            }
        }
    }
}
