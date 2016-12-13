using Newtonsoft.Json;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using System.Collections.Generic;

namespace VspWS.Plugins.BusinessLogic
{
    public class PayloadParser
    {
        public List<Payload> Parse(string message)
        {
            var result = JsonConvert.DeserializeObject<List<Payload>>(message);
            foreach(var x in result)
            {
                var content = x.MessageBody.ToString();
                x.Hl7Message = new Hl7Message(content);
            }
            return result;
        }
    }

    public class Payload
    {
        public string MessageType { get; set; }
        public string MessageBody { get; set; }
        public string MessageSource { get; set; }
        public Hl7Message Hl7Message { get; set; }
    }

    public class Hl7Message
    {
        public Hl7Message(string messageContent)
        {
            PipeParser parser = new PipeParser();
            Message = parser.Parse(messageContent.Trim());
            Version = Message.Version;
        }
        
        public dynamic Message { get; private set; }
        public string Version { get; private set; }
    }
}
