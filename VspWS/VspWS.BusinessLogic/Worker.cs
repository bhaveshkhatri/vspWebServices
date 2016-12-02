using System;
using System.Threading;
using VspWS.Common;
using VspWS.Common.Models;
using VspWS.DataAccess;

namespace VspWS.BusinessLogic
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

        public MessageResponse DoWork(Message message)
        {
            if(message.Source == MessageSource.receiver)
            {
                // TODO
            }
            else if(message.Source == MessageSource.processor)
            {
                // TODO
            }

            var messageType = message.MessageType;
            var messageId = new ConstrainedRandom(int.MaxValue).Next;
            var timestamp = DateTime.UtcNow;
            double factor = GetDelayFactor(messageType);

            var maxDelayGenerator = new ConstrainedRandom((int)Math.Round(_maximumDelay * factor, 0));
            var delayGenerator = new ConstrainedRandom(maxDelayGenerator.Next);
            var errorGenerator = new ConstrainedRandom(Constants.OneInNChanceOfError);
            Thread.Sleep(delayGenerator.Next);
            if (errorGenerator.Next == errorGenerator.Next)
            {
                throw new Exception(_errorMessage);
            }

            // TODO: populate
            return new MessageResponse
            {
                Id = messageId,
                Timestamp = timestamp.ToString()
            };
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
