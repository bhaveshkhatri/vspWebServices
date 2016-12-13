using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using VspWS.BusinessLogic;
using VspWS.Common;
using VspWS.Common.Models;

namespace VspWS.MessageReceiverService.Controllers
{
    public class ReceiverController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                var message = new Message { MessageType = MessageType.secondaryNormal, Source = MessageSource.receiver };
                var response = new Worker(Constants.MaximumReceivingDelay, "There was an error calling the MessageReceiverService.").DoWork(message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public IHttpActionResult Post(List<Message> messages)
        {
            //TODO: This should work with multiple messages.
            return Post(messages.FirstOrDefault());
        }

        //[HttpPost]
        private IHttpActionResult Post(Message message)
        {
            try
            {
                if (message != null)
                {
                    message.Source = MessageSource.receiver;
                    var response = new Worker(Constants.MaximumReceivingDelay, "There was an error calling the MessageReceiverService.").DoWork(message);
                    return Ok(response);
                }
                else
                {
                    throw new Exception("Payload did not contain any messages");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
