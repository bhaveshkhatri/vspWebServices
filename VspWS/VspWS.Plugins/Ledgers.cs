using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using VspWS.Common;

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
        private Dictionary<MeasurementType, Func<double>> Measurements { get; set; }

        public WebRequestExecutionLedger()
        {
            this.IsSuccess = true;
            this.Measurements = new Dictionary<MeasurementType, Func<double>>
            {
                { MeasurementType.TotalDuration, TotalDurationInMilliseconds },
                { MeasurementType.RequestDuration, RequestDurationInMilliseconds },
                { MeasurementType.ProcessingDuration, ProcessingDurationInMilliseconds }
            };
        }

        public int? MessageId { get; set; }

        public DateTime? RequestStarted { get; set; }

        public DateTime? RequestCompleted { get; set; }

        public DateTime? ProcessStarted { get; set; }

        public DateTime? ProcessCompleted { get; set; }

        public HttpStatusCode ResponseCode { get; set; }

        public bool IsSuccess { get; set; }

        public int MaximumDurationInMilliseconds { get; set; }

        public MeasurementType MeasurementType { get; set; }

        public double Duration
        {
            get { return Measurements[MeasurementType](); }
        }

        public int MaximumProcessingWaitTimeInMilliseconds { get; set; }

        public int ProcessingResultsPollingIntervalInMilliseconds { get; set; }

        public string AlSysConnectionString { get; set; }

        public string FalconConnectionString { get; set; }

        private double RequestDurationInMilliseconds()
        {
            var result = (RequestCompleted - RequestStarted).HasValue ? (RequestCompleted - RequestStarted).Value.TotalMilliseconds : 0;

            if (result < 0)
            {
                // TODO
                // throw new Exception("RequestDurationInMilliseconds cannot be less than zero.");
            }

            return result;
        }

        private double ProcessingDurationInMilliseconds()
        {
            return (ProcessCompleted - ProcessStarted).HasValue ? (ProcessCompleted - ProcessStarted).Value.TotalMilliseconds : 0;
        }

        private double TotalDurationInMilliseconds()
        {
            var orderedValues = new List<DateTime?>() { RequestStarted, RequestCompleted, ProcessStarted, ProcessCompleted }
            .Where(x => x.HasValue)
            .OrderBy(x => x.Value.Ticks);
            if ((orderedValues.Max() - orderedValues.Min()).Value.TotalMilliseconds < 0)
            {
                // TODO
                // throw new Exception("TotalDurationInMilliseconds cannot be less than zero.");
            }
            return (orderedValues.Max() - orderedValues.Min()).Value.TotalMilliseconds;
        }
    }
}
