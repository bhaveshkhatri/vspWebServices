using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace VspWS.Common.Tests.Unit
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void ParseEnumForMeasurementTypeReturnsTotalDurationAsDefault()
        {
            var result = Utils.ParseEnum<MeasurementType>("");

            result.Should().Be(MeasurementType.TotalDuration);
        }

        [TestMethod]
        public void ParseEnumForMeasurementTypeReturnsParsedMeasurementType()
        {
            var result = Utils.ParseEnum<MeasurementType>("ProcessingDuration");

            result.Should().Be(MeasurementType.ProcessingDuration);
        }
    }
}
