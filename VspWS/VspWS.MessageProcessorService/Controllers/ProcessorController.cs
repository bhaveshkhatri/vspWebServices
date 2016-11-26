using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using VspWS.Common;

namespace VspWS.MessageProcessorService.Controllers
{
    public class ProcessorController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            Thread.Sleep(new DelayGenerator(Constants.MaximumProcessingDelay).Milliseconds);
            return Ok(new List<string>() { "Message", "Processor", "Service" });
        }
    }
} 
