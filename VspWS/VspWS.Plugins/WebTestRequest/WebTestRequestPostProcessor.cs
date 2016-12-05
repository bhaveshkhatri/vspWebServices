using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VspWS.Plugins.WebTestRequest
{
    public class WebTestRequestPostProcessor : WebTestRequestPlugin
    {
        private LoadTestExecutionLedger LoadTestLedger;
        private WebTestExecutionLedger WebTestLedger;
        private WebRequestExecutionLedger WebRequestLedger;
                
        public override void PreRequestDataBinding(object sender, PreRequestDataBindingEventArgs e)
        {
            base.PreRequestDataBinding(sender, e);
            
            LoadTestLedger = e.WebTest.Context[Constants.LedgerKey] as LoadTestExecutionLedger;
            WebTestLedger = LoadTestLedger.WebTestExecutionLedgers[e.WebTest.Name];

            WebRequestLedger = new WebRequestExecutionLedger();
            if (e.Request.Method == "POST")
            {
                var bodyString = ((StringHttpBody)e.Request.Body).BodyString;
                dynamic body = JObject.Parse(bodyString);
                WebRequestLedger.MessageId = body.messageId;
            }
            WebRequestLedger.RequestStarted = DateTime.UtcNow;
            
            WebTestLedger.WebRequestExecutionLedgers.Add(WebRequestLedger);
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            base.PostRequest(sender, e);

            WebRequestLedger.ResponseCode = (int)e.Response.StatusCode;
        }
    }
}
