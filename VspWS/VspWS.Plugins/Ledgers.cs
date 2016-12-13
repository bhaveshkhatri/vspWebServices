using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VspWS.Common;
using VspWS.Plugins.BusinessLogic;

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
        public int MaximumProcessingWaitTimeInMilliseconds { get; set; }

        public int MaximumAverageDurationInMilliseconds { get; set; }

        public int MaximumSingleDurationInMilliseconds { get; set; }

        public ConcurrentDictionary<Guid, WebRequestExecutionLedger> WebRequestExecutionLedgers { get; set; }

        public ConcurrentDictionary<Guid, WebRequestExecutionLedger> AppendedRequestExecutionLedgers { get; set; }

        public string WebTestName { get; set; }

        public MeasurementType MeasurementType { get; set; }

        public WebTestExecutionLedger()
        {
            WebRequestExecutionLedgers = new ConcurrentDictionary<Guid, WebRequestExecutionLedger>();
            AppendedRequestExecutionLedgers = new ConcurrentDictionary<Guid, WebRequestExecutionLedger>();
        }
    }

    public class WebRequestExecutionLedger
    {
        private string _additionalInformation;

        private Dictionary<MeasurementType, Func<double>> _measurements { get; set; }

        public WebRequestExecutionLedger()
        {
            this.IsSuccess = true;
            this.Payloads = new List<Payload>();
            this._measurements = new Dictionary<MeasurementType, Func<double>>
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

        public double Duration(MeasurementType measurementType)
        {
            return _measurements[measurementType]();
        }

        public int MaximumProcessingWaitTimeInMilliseconds { get; set; }

        public int ProcessingResultsPollingIntervalInMilliseconds { get; set; }

        public string AlSysConnectionString { get; set; }

        public string FalconConnectionString { get; set; }

        public string LabelSuffix { get; set; }

        public string AdditionalInformation
        {
            get
            {
                return _additionalInformation ?? "";
            }
            set
            {
                _additionalInformation = value;
            }
        }

        public List<Payload> Payloads { get; set; }

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
