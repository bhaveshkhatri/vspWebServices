using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using VspWS.Common;

namespace VspWS.Plugins.WebTest
{
    public class WebTestPostProcessor : WebTestPlugin
    {        
        private LoadTestExecutionLedger LoadTestLedger;
        private WebTestExecutionLedger WebTestLedger;
        private int ProcessingResultsPollingIntervalInMilliseconds = 1000;

        [DefaultValue(0)]
        [Description("Maximum single request duration in milliseconds.")]
        public int MaximumRequestDurationInMilliseconds { get; set; }

        [DefaultValue(0)]
        [Description("Maximum average request duration in milliseconds.")]
        public int MaximumAverageDurationInMilliseconds { get; set; }

        [DefaultValue(60)]
        [Description("Maximum time to wait for processing to complete in seconds before a timeout.")]
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
                    MaximumRequestDurationInMilliseconds = MaximumRequestDurationInMilliseconds,
                    MaximumAverageDurationInMilliseconds = MaximumAverageDurationInMilliseconds,
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

            var requestLedger = new WebRequestExecutionLedger()
            {
                RequestStarted = Utils.Now(),
                MaximumProcessingWaitTimeInMilliseconds = MaximumProcessingWaitTimeInSeconds * 1000,
                ProcessingResultsPollingIntervalInMilliseconds = ProcessingResultsPollingIntervalInMilliseconds,
                AlSysConnectionString = AlSysConnectionString,
                FalconConnectionString = FalconConnectionString
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
            requestLedger.MessageId = body.Id;
        }
    }
}
