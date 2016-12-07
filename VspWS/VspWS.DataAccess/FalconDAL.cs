using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VspWS.Data;

namespace VspWS.DataAccess
{
    public class FalconDAL : IDisposable
    {
        private Falcon _context;

        public FalconDAL()
        {
            this._context = new Falcon();
        }

        public FalconDAL(string connectionString)
        {
            this._context = new Falcon(connectionString);
        }

        public IntegrationMessage GetIntegrationMessage(int messageId)
        {
            return this._context.IntegrationMessages.FirstOrDefault(msg => msg.MessageId == messageId);
        }

        public void AddIntegrationMessage(IntegrationMessage message)
        {
            this._context.IntegrationMessages.Add(message);
            this._context.SaveChanges();
        }

        public void SetRequestCompleted(int messageId, DateTime requestCompletedOn)
        {
            var trackingInfo = this._context.IntegrationMessages.Single(info => info.MessageId == messageId);
            trackingInfo.RequestCompletedOn = requestCompletedOn;
            this._context.SaveChanges();
        }

        public void Ping()
        {
            var x = this._context.IntegrationMessages.FirstOrDefault();
        }

        ~FalconDAL()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._context != null)
                {
                    this._context.Dispose();
                }
            }
        }
    }
}
