using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace VspWS.Plugins
{
    public class LoadTestExecutionLedger
    {
        public ConcurrentDictionary<string, WebTestExecutionLedger> WebTestExecutionLedgers { get; set; }

        public LoadTestExecutionLedger()
        {
            WebTestExecutionLedgers = new ConcurrentDictionary<string, WebTestExecutionLedger>();
        }
    }

    public class WebTestExecutionLedger
    {
        public ConcurrentDictionary<Guid, WebRequestExecutionLedger> WebRequestExecutionLedgers { get; set; }

        public WebTestExecutionLedger()
        {
            WebRequestExecutionLedgers = new ConcurrentDictionary<Guid, WebRequestExecutionLedger>();
        }
    }

    public class WebRequestExecutionLedger
    {
        public WebRequestExecutionLedger()
        {
            this.IsSuccess = true;
        }

        public int? MessageId { get; set; }

        public DateTime? RequestStarted { get; set; }

        public DateTime? RequestCompleted { get; set; }

        public DateTime? ProcessStarted { get; set; }

        public DateTime? ProcessCompleted { get; set; }

        public HttpStatusCode ResponseCode { get; set; }

        public bool IsSuccess { get; internal set; }

        public double RequestDurationInMilliseconds
        {
            get
            {
                var result = (RequestCompleted - RequestStarted).HasValue ? (RequestCompleted - RequestStarted).Value.TotalMilliseconds : 0;

                if(result < 0)
                {
                    throw new Exception("RequestDurationInMilliseconds cannot be less than zero.");
                }

                return result;
            }
        }

        public double ProcessingDurationInMilliseconds
        {
            get { return (ProcessCompleted - ProcessStarted).HasValue ? (ProcessCompleted - ProcessStarted).Value.TotalMilliseconds : 0; }
        }

        public double TotalDurationInMilliseconds
        {
            get
            {
                var orderedValues = new List<DateTime?>() { RequestStarted, RequestCompleted, ProcessStarted, ProcessCompleted }
                .Where(x => x.HasValue)
                .OrderBy(x => x.Value.Ticks);
                if((orderedValues.Max() - orderedValues.Min()).Value.TotalMilliseconds < 0)
                {
                    throw new Exception("TotalDurationInMilliseconds cannot be less than zero.");
                }
                return (orderedValues.Max() - orderedValues.Min()).Value.TotalMilliseconds;
            }
        }

        public int MaximumDurationInMilliseconds { get; internal set; }
    }
}
