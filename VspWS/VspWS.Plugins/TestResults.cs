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
        public HttpSample()
        {
            this.DataType = "text";
            this.Bytes = 1;
            this.SentBytes = 1;
            this.NumberOfActiveThreadsInThisGroup = 1;
            this.NumberOfActiveThreadsInAllGroups = 1;
        }

        [XmlAttribute(AttributeName = "t")]
        public int ElapsedTimeInMilliseconds { get; set; }

        [XmlAttribute(AttributeName = "it")]
        public int IdleTimeInMilliseconds { get; set; }

        [XmlAttribute(AttributeName = "lt")]
        public int LatencyInMilliseconds { get; set; }

        [XmlAttribute(AttributeName = "ct")]
        public int ConnectTimeInMilliseconds { get; set; }

        [XmlAttribute(AttributeName = "ts")]
        public int MillisecondsSince19700101 { get; set; }

        [XmlAttribute(AttributeName = "s")]
        public bool IsSuccess { get; set; }

        [XmlAttribute(AttributeName = "lb")]
        public string Label { get; set; }

        [XmlAttribute(AttributeName = "rc")]
        public int ResponseCode { get; set; }

        [XmlAttribute(AttributeName = "rm")]
        public string ResponseMessage { get; set; }

        [XmlAttribute(AttributeName = "tn")]
        public string ThreadName { get; set; }

        [XmlAttribute(AttributeName = "dt")]
        public string DataType { get; set; }

        [XmlAttribute(AttributeName = "by")]
        public int Bytes { get; set; }

        [XmlAttribute(AttributeName = "sby")]
        public int SentBytes { get; set; }

        [XmlAttribute(AttributeName = "ng")]
        public int NumberOfActiveThreadsInThisGroup { get; set; }

        [XmlAttribute(AttributeName = "na")]
        public int NumberOfActiveThreadsInAllGroups { get; set; }

        [XmlAttribute(AttributeName = "vsptmid")]
        public string MessageId { get; set; }
    }
}