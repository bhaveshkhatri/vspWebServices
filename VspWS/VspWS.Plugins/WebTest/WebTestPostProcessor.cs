using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using VspWS.Common;

namespace VspWS.Plugins.WebTest
{
    public class WebTestPostProcessor : WebTestPlugin
    {        
        private LoadTestExecutionLedger LoadTestLedger;
        private WebTestExecutionLedger WebTestLedger;
        private MeasurementType MeasurementType;
        private int ProcessingResultsPollingIntervalInMilliseconds = 1000;
        private string AlSysConnectionString = "Data Source=104.214.38.78;Initial Catalog=AlSys;User ID=vspws;Password=V5pw3bservice!@";
        private string FalconConnectionString = "Data Source=104.214.38.78;Initial Catalog=Falcon;User ID=vspws;Password=V5pw3bservice!@";

        [DefaultValue(0)]
        [Description("Maximum request duration in milliseconds.")]
        public int MaximumDurationInMilliseconds { get; set; }

        [DefaultValue(60)]
        [Description("Maximum time to wait for processing to complete in seconds before a timeout.")]
        public int MaximumProcessingWaitTimeInSeconds { get; set; }

        [DefaultValue("RequestDuration")]
        [Description("This will be used to measure the elapsed time (e.g. RequestDuration, ProcessingDuration, TotalDuration). TotalDuration is the default.")]
        public string MeasureBy { get; set; }

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            base.PreWebTest(sender, e);
            MeasurementType = Utils.ParseEnum<MeasurementType>(MeasureBy);
            LoadTestLedger = e.WebTest.Context[Constants.LedgerKey] as LoadTestExecutionLedger ?? new LoadTestExecutionLedger();
            if (LoadTestLedger.WebTestExecutionLedgers.Any(x => x.Key == e.WebTest.Name))
            {
                WebTestLedger = LoadTestLedger.WebTestExecutionLedgers.Single(x => x.Key == e.WebTest.Name).Value;
            }
            else
            {
                WebTestLedger = new WebTestExecutionLedger();
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
                MeasurementType = MeasurementType,
                RequestStarted = Utils.Now(),
                MaximumDurationInMilliseconds = MaximumDurationInMilliseconds,
                MaximumProcessingWaitTimeInMilliseconds = MaximumProcessingWaitTimeInSeconds * 1000,
                ProcessingResultsPollingIntervalInMilliseconds = ProcessingResultsPollingIntervalInMilliseconds,
                AlSysConnectionString = AlSysConnectionString,
                FalconConnectionString = FalconConnectionString
            };

            WebTestLedger.WebRequestExecutionLedgers.TryAdd(requestGuid, requestLedger);
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            var requestGuid = e.Request.Guid;
            var requestLedger = WebTestLedger.WebRequestExecutionLedgers[requestGuid];

            requestLedger.RequestCompleted = Utils.Now();
            requestLedger.ResponseCode = e.Response.StatusCode;
            var bodyString = e.Response.BodyString;
            dynamic body = JObject.Parse(bodyString);
            requestLedger.MessageId = body.Id;

            if (requestLedger.ResponseCode != HttpStatusCode.OK)
            {
                e.Request.Outcome = Outcome.Fail;
                requestLedger.IsSuccess = false;
            }
            if(requestLedger.MaximumDurationInMilliseconds > 0
                && requestLedger.Duration > requestLedger.MaximumDurationInMilliseconds)
            {
                e.Request.Outcome = Outcome.Fail;
                e.WebTest.AddCommentToResult(string.Format("Request [{0}] took longer than the expected [{1}] millisecond(s).", requestGuid, requestLedger.MaximumDurationInMilliseconds));
                requestLedger.IsSuccess = false;
            }
        }
    }
}
