using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VspWS.Plugins
{
    [XmlRoot("testResults")]
    public class TestResults
    {
        public TestResults()
        {
            this.Version = "1.2";
        }

        [XmlElement(ElementName = "httpSample")]
        public List<HttpSample> HttpSamples { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }

    public class HttpSample
    {
        [XmlAttribute(AttributeName = "t")]
        public int ElapsedTimeInMilliseconds { get; set; }

        [XmlAttribute(AttributeName = "ec")]
        public int ErrorCount { get; set; }

        [XmlAttribute(AttributeName = "lb")]
        public string Label { get; set; }

        [XmlAttribute(AttributeName = "rc")]
        public int ResponseCode { get; set; }

        [XmlAttribute(AttributeName = "rm")]
        public string ResponseMessage { get; set; }
    }
}
