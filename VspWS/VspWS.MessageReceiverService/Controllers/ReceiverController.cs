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
            var maxDelayGenerator = new ConstrainedRandom(Constants.MaximumReceivingDelay);
            var delayGenerator = new ConstrainedRandom(maxDelayGenerator.Next);
            var errorGenerator = new ConstrainedRandom(Constants.OneInNChanceOfError);
            Thread.Sleep(delayGenerator.Next);
            if(errorGenerator.Next == errorGenerator.Next)
            {
                throw new System.Exception("There was an error calling the MessageReceiverService.");
            }
            return Ok(new List<string>() { "Message", "Receiver", "Service" });
        }
    }
}
