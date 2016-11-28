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
            var maxDelayGenerator = new DelayGenerator(Constants.MaximumProcessingDelay);
            var delayGenerator = new DelayGenerator(maxDelayGenerator.Milliseconds);
            Thread.Sleep(delayGenerator.Milliseconds);
            return Ok(new List<string>() { "Message", "Processor", "Service" });
        }
    }
} 
