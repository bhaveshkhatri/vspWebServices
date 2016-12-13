using System;
using System.Threading;
using System.Threading.Tasks;
using VspWS.Common;
using VspWS.Common.Models;
using VspWS.Data;
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
            var messageType = message.MessageType;
            var messageId = new ConstrainedRandom(int.MaxValue).Next;
            var timestamp = Utils.Now();

            if (message.Source == MessageSource.receiver)
            {
                // TODO
                // 1. Add IntegrationMessage
                using(var dal = new FalconDAL())
                {
                    dal.AddIntegrationMessage(new IntegrationMessage { MessageId = messageId, Body = message.MessageBody, RequestStartedOn = timestamp });
                }
                // 2. Start worker thread to process the message
                Task.Factory.StartNew(() =>
                {
                    ProcessMessage(messageType, messageId);
                    // 2.4 Mark IntegrationMessage as processed.
                    // TODO
                });
            }
            else if (message.Source == MessageSource.processor)
            {
                ProcessMessage(messageType, messageId);
            }
            else
            {
                throw new NotSupportedException(string.Format("The message source [{0}] is not supported.", message.Source));
            }

            SimulateWork(messageType);

            if (message.Source == MessageSource.receiver)
            {
                using (var dal = new FalconDAL())
                {
                    dal.SetRequestCompleted(messageId, Utils.Now());
                }
            }

            // TODO: populate
            return new MessageResponse
            {
                Id = messageId,
                Timestamp = timestamp.ToString()
            };
        }

        private void ProcessMessage(MessageType messageType, int messageId)
        {
            // Simulate processing delay
            SimulateWork(messageType);

            using (var dal = new AlSysDAL())
            {
                // 2.1 Add EhrMessageTrackingInfo
                dal.AddEhrMessageTrackingInfo(new EhrMessageTrackingInfo { MessageId = messageId });
                dal.SetProcessStarted(messageId, Utils.Now());
            }
            
            // 2.2 Simulate a delay
            SimulateWork(messageType);
            
            // 2.3 Get IntegrationMessageInfo
            DateTime? requestReceivedOn = null;
            DateTime? requestCompletedOn = null;
            using (var falconDal = new FalconDAL())
            {
                IntegrationMessage integrationMessage = falconDal.GetIntegrationMessage(messageId);
                if (integrationMessage != null)
                {
                    requestReceivedOn = integrationMessage.RequestStartedOn;
                    requestCompletedOn = integrationMessage.RequestCompletedOn;
                }
            }

            using (var dal = new AlSysDAL())
            {
                // 2.4 Update EhrMessageTrackingInfo completion
                dal.SetProcessCompleted(messageId, Utils.Now(), requestReceivedOn, requestCompletedOn);
            }
        }

        private void SimulateWork(MessageType messageType)
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
