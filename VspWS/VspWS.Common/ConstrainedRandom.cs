using System;

namespace VspWS.Common
{
    public class ConstrainedRandom
    {
        private int _maximumValue;

        public ConstrainedRandom(int maximumValue)
        {
            _maximumValue = maximumValue;
        }

        public int Next
        {
            get
            {
                return new Random(Guid.NewGuid().GetHashCode()).Next(0, _maximumValue);
            }
        }
    }
}
