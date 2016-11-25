using System.Collections.Generic;
using System.Web.Http;

namespace VspWS.MessageReceiverService.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(new List<string>() { "Message", "Receiver", "Service" });
        }
    }
}
