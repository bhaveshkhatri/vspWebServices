using System.Collections.Generic;
using System.Web.Http;

namespace VspWS.MessageProcessorService.Controllers
{
    public class ProcessorController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(new List<string>() { "Message", "Processor", "Service" });
        }
    }
} 
