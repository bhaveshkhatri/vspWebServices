using System;
using System.Threading;

namespace VspWS.Common
{
    public class Worker
    {
        private int _maximumDelay;
        private string _errorMessage;

        public Worker(int maximumDelay, string errorMessage)
        {
            _maximumDelay = maximumDelay;
            _errorMessage = errorMessage;
        }

        public void DoWork()
        {
            var maxDelayGenerator = new ConstrainedRandom(_maximumDelay);
            var delayGenerator = new ConstrainedRandom(maxDelayGenerator.Next);
            var errorGenerator = new ConstrainedRandom(Constants.OneInNChanceOfError);
            Thread.Sleep(delayGenerator.Next);
            if (errorGenerator.Next != errorGenerator.Next)
            {
                throw new Exception(_errorMessage);
            }
        }
    }
}
