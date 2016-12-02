using System;
using System.Collections.Generic;
using System.Web.Http;
using VspWS.BusinessLogic;
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
                var message = new Message { MessageType = MessageType.secondaryNormal, Source = MessageSource.processor };
                var response = new Worker(Constants.MaximumProcessingDelay, "There was an error calling the MessageProcessorService.").DoWork(message);
                return Ok(response);
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public IHttpActionResult Post(Message message)
        {
            try
            {
                message.Source = MessageSource.processor;
                var response = new Worker(Constants.MaximumProcessingDelay, "There was an error calling the MessageProcessorService.").DoWork(message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
} 
