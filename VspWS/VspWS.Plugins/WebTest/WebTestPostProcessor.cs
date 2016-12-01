using Microsoft.VisualStudio.TestTools.WebTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace VspWS.Plugins.WebTest
{
    public class WebTestPostProcessor : WebTestPlugin
    {
        private List<int> _responseCodes = new List<int>();

        public override void PostWebTest(object sender, PostWebTestEventArgs e)
        {
            base.PostWebTest(sender, e);

            if (_responseCodes.Any(x => x != (int)HttpStatusCode.OK))
            {
                e.WebTest.Outcome = Outcome.Fail;
                e.WebTest.AddCommentToResult("Not all responses had status code 200.");
            }

            //Debugger.Launch();
        }

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {
            base.PostRequest(sender, e);

            this._responseCodes.Add((int)e.Response.StatusCode);

            //Debugger.Launch();
        }
    }
}
