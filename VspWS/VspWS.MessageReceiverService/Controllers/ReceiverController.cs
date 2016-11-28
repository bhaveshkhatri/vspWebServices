using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using VspWS.Common;

namespace VspWS.MessageReceiverService.Controllers
{
    public class ReceiverController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            var maxDelayGenerator = new DelayGenerator(Constants.MaximumReceivingDelay);
            var delayGenerator = new DelayGenerator(maxDelayGenerator.Milliseconds);
            Thread.Sleep(delayGenerator.Milliseconds);
            return Ok(new List<string>() { "Message", "Receiver", "Service" });
        }
    }
}
