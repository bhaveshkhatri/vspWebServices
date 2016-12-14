using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using VspWS.Common;
using VspWS.DataAccess;
using VspWS.Plugins.BusinessLogic;

namespace VspWS.Plugins.WebTest
{
    public class WebTestPostProcessor : WebTestPlugin
    {        
        private LoadTestExecutionLedger LoadTestLedger;
        private WebTestExecutionLedger WebTestLedger;
        private int ProcessingResultsPollingIntervalInMilliseconds = 1000;

        [DefaultValue(0)]
        [Description("Maximum single duration in milliseconds.")]
        public int MaximumSingleDurationInMilliseconds { get; set; }

        [DefaultValue(0)]
        [Description("Maximum average request duration in milliseconds.")]
        public int MaximumAverageDurationInMilliseconds { get; set; }

        [DefaultValue(60)]
        [Description("Maximum time to wait for processing times to be obtained.")]
        public int MaximumProcessingWaitTimeInSeconds { get; set; }

        [DefaultValue("RequestDuration")]
        [Description("This will be used to measure the elapsed time (e.g. RequestDuration, ProcessingDuration, TotalDuration). TotalDuration is the default.")]
        public string MeasureBy { get; set; }

        [DefaultValue("")]
        [Description("Connection string to the AlSys database.")]
        public string AlSysConnectionString { get; set; }

        [DefaultValue("")]
        [Description("Connection string to the Falcon database.")]
        public string FalconConnectionString { get; set; }

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            base.PreWebTest(sender, e);
            LoadTestLedger = e.WebTest.Context[Constants.LedgerKey] as LoadTestExecutionLedger ?? new LoadTestExecutionLedger();
            if (LoadTestLedger.WebTestExecutionLedgers.Any(x => x.Key == e.WebTest.Name))
            {
                WebTestLedger = LoadTestLedger.WebTestExecutionLedgers.Single(x => x.Key == e.WebTest.Name).Value;
            }
            else
            {
                WebTestLedger = new WebTestExecutionLedger {
                    WebTestName = e.WebTest.Name,
                    MeasurementType = Utils.ParseEnum<MeasurementType>(MeasureBy),
                    MaximumSingleDurationInMilliseconds = MaximumSingleDurationInMilliseconds == 0 ? int.MaxValue : MaximumSingleDurationInMilliseconds,
                    MaximumAverageDurationInMilliseconds = MaximumAverageDurationInMilliseconds == 0 ? int.MaxValue : MaximumAverageDurationInMilliseconds,
                    MaximumProcessingWaitTimeInMilliseconds = MaximumProcessingWaitTimeInSeconds == 0 ? int.MaxValue : MaximumProcessingWaitTimeInSeconds * 1000
                };
                LoadTestLedger.WebTestExecutionLedgers.TryAdd(e.WebTest.Name, WebTestLedger);
            }
        }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            Guid requestGuid;
            do
            {
                e.Request.Guid = Guid.NewGuid();
                requestGuid = e.Request.Guid;
            } while (WebTestLedger.WebRequestExecutionLedgers.ContainsKey(requestGuid));

            var payloads = new List<Payload>();

            if (e.Request.Body != null)
            {
                var bodyString = ((StringHttpBody)e.Request.Body).BodyString;
                if (!string.IsNullOrWhiteSpace(bodyString))
                {
                    payloads = new PayloadParser().Parse(bodyString);
                }
            }

            var requestLedger = new WebRequestExecutionLedger()
            {
                RequestStarted = Utils.Now(),
                MaximumProcessingWaitTimeInMilliseconds = MaximumProcessingWaitTimeInSeconds * 1000,
                ProcessingResultsPollingIntervalInMilliseconds = ProcessingResultsPollingIntervalInMilliseconds,
                AlSysConnectionString = AlSysConnectionString,
                FalconConnectionString = FalconConnectionString,
                Payloads = payloads
            };

            WebTestLedger.WebRequestExecutionLedgers.TryAdd(requestGuid, requestLedger);
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            var requestLedger = WebTestLedger.WebRequestExecutionLedgers[e.Request.Guid];
            var bodyString = e.Response.BodyString;
            dynamic body = JObject.Parse(bodyString);

            requestLedger.RequestCompleted = Utils.Now();
            requestLedger.ResponseCode = e.Response.StatusCode;
            requestLedger.IsSuccess = requestLedger.ResponseCode == HttpStatusCode.OK;
            requestLedger.MessageId = body.Id;

            ValidateDuration(e, requestLedger);
        }

        private void ValidateDuration(PostRequestEventArgs e, WebRequestExecutionLedger requestLedger)
        {
            if (WebTestLedger.MeasurementType == MeasurementType.ProcessingDuration || WebTestLedger.MeasurementType == MeasurementType.TotalDuration)
            {
                var isComplete = false;
                var startTime = Utils.Now();
                var endTime = startTime.AddMilliseconds(WebTestLedger.MaximumProcessingWaitTimeInMilliseconds).AddMilliseconds(-1 * requestLedger.ProcessingResultsPollingIntervalInMilliseconds);
                while (!isComplete && Utils.Now() <= endTime)
                {
                    Thread.Sleep(requestLedger.ProcessingResultsPollingIntervalInMilliseconds);
                    using (var dal = new AlSysDAL(requestLedger.AlSysConnectionString))
                    {
                        var item = dal.GetEhrMessageTrackingInfo(requestLedger.MessageId);

                        if (item != null && item.ProcessCompletedOn != null)
                        {
                            requestLedger.ProcessStarted = item.ProcessStartedOn;
                            requestLedger.ProcessCompleted = item.ProcessCompletedOn;
                            isComplete = true;
                        }
                    }
                }

                if (!(requestLedger.ProcessStarted.HasValue && requestLedger.ProcessCompleted.HasValue))
                {
                    requestLedger.IsSuccess = false;
                    requestLedger.ResponseCode = HttpStatusCode.Ambiguous;
                    requestLedger.AdditionalInformation += "Processing duration could not be determined in time.";
                }
            }

            if (WebTestLedger.MaximumSingleDurationInMilliseconds > 0
            && requestLedger.Duration(WebTestLedger.MeasurementType) > WebTestLedger.MaximumSingleDurationInMilliseconds)
            {
                requestLedger.IsSuccess = false;
                requestLedger.AdditionalInformation += "Request duration exceeded threshold.";
            }

            e.Request.Outcome = e.WebTest.Outcome = requestLedger.IsSuccess ? Outcome.Pass : Outcome.Fail;
            e.WebTest.AddCommentToResult(requestLedger.AdditionalInformation);
        }
    }
}
