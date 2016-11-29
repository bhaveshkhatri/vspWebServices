using System;
using System.Threading;
using VspWS.Common.Models;

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
            DoWork(MessageType.secondaryNormal);
        }

        public void DoWork(Message message)
        {
            DoWork(message.MessageType);
        }

        private void DoWork(MessageType messageType)
        {
            double factor = GetDelayFactor(messageType);

            var maxDelayGenerator = new ConstrainedRandom((int)Math.Round(_maximumDelay * factor, 0));
            var delayGenerator = new ConstrainedRandom(maxDelayGenerator.Next);
            var errorGenerator = new ConstrainedRandom(Constants.OneInNChanceOfError);
            Thread.Sleep(delayGenerator.Next);
            if (errorGenerator.Next == errorGenerator.Next)
            {
                throw new Exception(_errorMessage);
            }
        }

        private static double GetDelayFactor(MessageType messageType)
        {
            double factor;
            switch (messageType)
            {
                case MessageType.primarySlow:
                    factor = 2.0;
                    break;
                case MessageType.secondaryNormal:
                    factor = 1.0;
                    break;
                case MessageType.tertiaryFast:
                    factor = 0.5;
                    break;
                default:
                    throw new ArgumentException(string.Format("{0} is not a valid message type.", messageType));
            }

            return factor;
        }
    }
}
