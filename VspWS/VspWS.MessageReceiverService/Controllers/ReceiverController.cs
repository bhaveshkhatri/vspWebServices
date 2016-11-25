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
            Thread.Sleep(new DelayGenerator(Constants.MaximumReceivingDelay).Milliseconds);
            return Ok(new List<string>() { "Message", "Receiver", "Service" });
        }
    }
}
