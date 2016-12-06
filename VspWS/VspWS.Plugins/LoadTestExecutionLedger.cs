using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int? MessageId { get; set; }
        public DateTime? RequestStarted { get; set; }
        public DateTime? RequestCompleted { get; set; }
        public DateTime? ProcessStarted { get; set; }
        public DateTime? ProcessCompleted { get; set; }
        public int ResponseCode { get; set; }

        public int RequestDurationInMilliseconds { get { return (RequestCompleted - RequestStarted).HasValue ? (RequestCompleted - RequestStarted).Value.Milliseconds : 0; } }
        public int ProcessingDurationInMilliseconds { get { return (ProcessCompleted - ProcessStarted).HasValue ? (ProcessCompleted - ProcessStarted).Value.Milliseconds : 0; } }
        public int TotalDurationInMilliseconds
        {
            get
            {
                var orderedValues = new List<DateTime?>() { RequestStarted, RequestCompleted, ProcessStarted, ProcessCompleted }
                .Where(x => x.HasValue)
                .OrderBy(x => x.Value.Ticks);
                return (orderedValues.Max() - orderedValues.Min()).Value.Milliseconds;
            }
        }

        public int MaximumDurationInMilliseconds { get; internal set; }
    }
}
