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
            var maxDelayGenerator = new ConstrainedRandom(Constants.MaximumProcessingDelay);
            var delayGenerator = new ConstrainedRandom(maxDelayGenerator.Next);
            var errorGenerator = new ConstrainedRandom(Constants.OneInNChanceOfError);
            Thread.Sleep(delayGenerator.Next);
            if (errorGenerator.Next == errorGenerator.Next)
            {
                throw new System.Exception("There was an error calling the MessageProcessorService.");
            }
            return Ok(new List<string>() { "Message", "Processor", "Service" });
        }
    }
} 
