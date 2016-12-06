using Microsoft.VisualStudio.TestTools.WebTesting;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace VspWS.Plugins.WebTest
{
    public class WebTestPostProcessor : WebTestPlugin
    {        
        private LoadTestExecutionLedger LoadTestLedger;
        private WebTestExecutionLedger WebTestLedger;

        [DefaultValue(0)]
        [Description("Maximum request duration in milliseconds.")]
        public int MaximumDurationInMilliseconds { get; set; }

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
                WebTestLedger = new WebTestExecutionLedger();
                LoadTestLedger.WebTestExecutionLedgers.TryAdd(e.WebTest.Name, WebTestLedger);
            }
        }

        public override void PreRequest(object sender, PreRequestEventArgs e)
        {
            e.Request.Guid = Guid.NewGuid();
            var requestGuid = e.Request.Guid;
            var requestLedger = new WebRequestExecutionLedger()
            {
                RequestStarted = DateTime.UtcNow,
                MaximumDurationInMilliseconds = MaximumDurationInMilliseconds
            };

            WebTestLedger.WebRequestExecutionLedgers.TryAdd(requestGuid, requestLedger);
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            var requestGuid = e.Request.Guid;
            var requestLedger = WebTestLedger.WebRequestExecutionLedgers[requestGuid];

            requestLedger.RequestCompleted = DateTime.UtcNow;
            
            if(e.Response.StatusCode != HttpStatusCode.OK)
            {
                e.Request.Outcome = Outcome.Fail;
            }
            if(requestLedger.MaximumDurationInMilliseconds > 0
                && requestLedger.RequestDurationInMilliseconds > requestLedger.MaximumDurationInMilliseconds)
            {
                e.Request.Outcome = Outcome.Fail;
                e.WebTest.AddCommentToResult(string.Format("Request [{0}] took longer than the expected [{1}] millisecond(s).", requestGuid, requestLedger.MaximumDurationInMilliseconds));
            }
        }
    }
}
