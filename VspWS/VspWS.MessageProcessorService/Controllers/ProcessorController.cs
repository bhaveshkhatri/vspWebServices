using System;
using System.Collections.Generic;
using System.Web.Http;
using VspWS.Common;
using VspWS.Common.Models;

namespace VspWS.MessageProcessorService.Controllers
{
    public class ProcessorController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                new Worker(Constants.MaximumProcessingDelay, "There was an error calling the MessageProcessorService.").DoWork();
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(new List<string>() { "Message", "Processor", "Service" });
        }

        [HttpPost]
        public IHttpActionResult Post(Message message)
        {
            try
            {
                new Worker(Constants.MaximumProcessingDelay, "There was an error calling the MessageProcessorService.").DoWork(message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(new List<string>() { "Processed", message.MessageBody, "On " + DateTime.Now });
        }
    }
} 
