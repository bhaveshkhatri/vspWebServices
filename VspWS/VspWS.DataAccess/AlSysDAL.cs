using System;
using System.Linq;
using VspWS.Data;

namespace VspWS.DataAccess
{
    public class AlSysDAL : IDisposable
    {
        private AlSys _context;

        public AlSysDAL()
        {
            this._context = new AlSys();
        }

        public AlSysDAL(string connectionString)
        {
            this._context = new AlSys(connectionString);
        }

        public void AddEhrMessageTrackingInfo(EhrMessageTrackingInfo trackingInfo)
        {
            this._context.EhrMessageTrackingInfos.Add(trackingInfo);
            this._context.SaveChanges();
        }

        public void SetProcessStarted(int messageId, DateTime processStartedOn)
        {
            var trackingInfo = this.GetEhrMessageTrackingInfo(messageId);
            trackingInfo.ProcessStartedOn = processStartedOn;
            this._context.SaveChanges();
        }

        public EhrMessageTrackingInfo GetEhrMessageTrackingInfo(int? messageId)
        {
            return this._context.EhrMessageTrackingInfos.SingleOrDefault(info => info.MessageId == messageId);
        }

        public void Ping()
        {
            var x = this._context.EhrMessageTrackingInfos.FirstOrDefault();
        }

        public void SetProcessCompleted(int messageId, DateTime processCompletedOn, DateTime? requestReceivedOn, DateTime? requestCompletedOn)
        {
            var trackingInfo = this.GetEhrMessageTrackingInfo(messageId);
            trackingInfo.ProcessCompletedOn = processCompletedOn;
            trackingInfo.RequestReceivedOn = requestReceivedOn;
            trackingInfo.RequestCompletedOn = requestCompletedOn;
            this._context.SaveChanges();
        }

        ~AlSysDAL()
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
