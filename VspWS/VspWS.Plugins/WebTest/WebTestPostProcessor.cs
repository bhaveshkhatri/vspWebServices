using Microsoft.VisualStudio.TestTools.WebTesting;
using System.Linq;
using System.Net;
using VspWS.Plugins.WebTestRequest;

namespace VspWS.Plugins.WebTest
{
    public class WebTestPostProcessor : WebTestPlugin
    {        
        private LoadTestExecutionLedger LoadTestLedger;
        private WebTestExecutionLedger WebTestLedger;

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

        public override void PostWebTest(object sender, PostWebTestEventArgs e)
        {
            base.PostWebTest(sender, e);

            var responseCodes = LoadTestLedger.WebTestExecutionLedgers.SelectMany(x => x.Value.WebRequestExecutionLedgers.Select(y => y.ResponseCode)).ToList();
            if (responseCodes.Any(x => x != (int)HttpStatusCode.OK))
            {
                e.WebTest.Outcome = Outcome.Fail;
                e.WebTest.AddCommentToResult("Not all responses had status code 200.");
            }
        }

        public override void PreRequestDataBinding(object sender, PreRequestDataBindingEventArgs e)
        {
            var reference = new WebTestRequestPluginReference
            {
                Description = "A",
                DisplayName = "B",
                ExecutionOrder = RuleExecutionOrder.AfterDependents,
                Type = typeof(WebTestRequestPostProcessor)
            };
            // TODO: Don't call event explicitly
            reference.CreateInstance().PreRequestDataBinding(sender, e);
            e.Request.WebTestRequestPluginReferences.Add(reference);
            base.PreRequestDataBinding(sender, e);
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            var reference = new WebTestRequestPluginReference
            {
                Description = "A",
                DisplayName = "B",
                ExecutionOrder = RuleExecutionOrder.AfterDependents,
                Type = typeof(WebTestRequestPostProcessor)
            };
            // TODO: Don't call event explicitly
            reference.CreateInstance().PostRequest(sender, e);
            e.Request.WebTestRequestPluginReferences.Add(reference);
            base.PostRequest(sender, e);
        }
    }
}
