using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHapi.Model.V251.Message;

namespace VspWS.Plugins.BusinessLogic.Tests.Unit
{
    [TestClass]
    public class PayloadParserTests
    {
        [TestMethod]
        public void ParseShouldPopulateTheHl7MessageProperty()
        {
            var payload = @"[{""MessageType"":""0"",""MessageBody"":""MSH|^~\\&|EMA^ModernizingMedicine|epmstaging^EPM STA|OM|9000030572^EPM STA|20161117221350||DFT^P03|20161116221350882|P|2.5.1
EVN|P03|20161117221350
PID|||934636||Patient^Falcon^R^^Mr||19740115|M|||||||||||555445544
PV1|1|O|^^^184^^^^^Eyefinity Practice Management||||1864^Smith^Michael^^^||||||||||||1358794|||||||||||||||||||||||||||||||1358794
ZPO||||||
ZRX|902793|20161116141043-0800|1864^Smith^Michael^^^|S|-0.25|-0.25|100.00||||||||-0.25|-0.25|100.00||||||||^|20171117|DSV|^|^^^^^activeInd:true||F|^
"",""PracticeId"": ""9000030572""}]";

            var parsedPayload = new PayloadParser().Parse(payload);

            parsedPayload[0].Should().NotBeNull();
            parsedPayload[0].Hl7Message.Should().NotBeNull();
            parsedPayload[0].Hl7Message.Version.Should().Be("2.5.1");
            ((int)parsedPayload[0].Hl7Message.Message.PID.DateTimeOfBirth.Time.Year).Should().Be(1974);
        }
    }
}
