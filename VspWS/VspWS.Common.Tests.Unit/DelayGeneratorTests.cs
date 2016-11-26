using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VspWS.Common.Tests.Unit
{
    [TestClass]
    public class DelayGeneratorTests
    {
        [TestMethod]
        public void MillisecondsShouldBeLessOrEqualToMaximum()
        {
            const int maximumDelay = 5000;
            var sut = new DelayGenerator(maximumDelay);

            var results = new List<int>();

            for(var i = 0; i < 1000; i++)
            {
                results.Add(sut.Milliseconds);
            }

            results.All(x => x <= maximumDelay);
        }

        [TestMethod]
        public void MillisecondsShouldBeRandom()
        {
            const int maximumDelay = 5000;
            const int iterations = 1000;
            const double repeatThresholdPercent = 1;
            var repeatThreshold = Math.Round(iterations * (repeatThresholdPercent/100));

            var results = new List<int>();

            for (var i = 0; i < iterations; i++)
            {
                var sut = new DelayGenerator(maximumDelay);
                results.Add(sut.Milliseconds);
            }

            results.GroupBy(delay => delay).All(repeats => repeats.Count() < repeatThreshold).Should().BeTrue();
        }
    }
}
