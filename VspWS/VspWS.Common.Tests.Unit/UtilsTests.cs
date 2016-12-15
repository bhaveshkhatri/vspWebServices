using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace VspWS.Common.Tests.Unit
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void ParseEnumForMessageSourceReturnsUnknownAsDefault()
        {
            var result = Utils.ParseEnum<MessageSource>("");

            result.Should().Be(MessageSource.unknown);
        }

        [TestMethod]
        public void ParseEnumForMessageSourceReturnsParsedMessageSource()
        {
            var result = Utils.ParseEnum<MessageSource>("processor");

            result.Should().Be(MessageSource.processor);
        }
    }
}
