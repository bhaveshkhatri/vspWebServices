using System;
using System.Collections.Generic;
using System.Web.Http;
using VspWS.Common;

namespace VspWS.MessageReceiverService.Controllers
{
    public class ReceiverController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                new Worker(Constants.MaximumReceivingDelay, "There was an error calling the MessageReceiverService.").DoWork();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(new List<string>() { "Message", "Receiver", "Service" });
        }
    }
}
