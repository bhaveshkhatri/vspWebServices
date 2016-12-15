using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VspWS.Common.Tests.Unit
{
    [TestClass]
    public class ConstrainedRandomTests
    {
        [TestMethod]
        public void NextShouldBeLessOrEqualToMaximum()
        {
            const int maximumDelay = 5000;
            var sut = new ConstrainedRandom(maximumDelay);

            var results = new List<int>();

            for(var i = 0; i < 1000; i++)
            {
                results.Add(sut.Next);
            }

            results.All(x => x <= maximumDelay);
        }

        [TestMethod]
        public void NextShouldBeRandom()
        {
            const int maximumDelay = 5000;
            const int iterations = 1000;
            const double repeatThresholdPercent = 1;
            var repeatThreshold = Math.Round(iterations * (repeatThresholdPercent/100));

            var results = new List<int>();

            for (var i = 0; i < iterations; i++)
            {
                var sut = new ConstrainedRandom(maximumDelay);
                results.Add(sut.Next);
            }

            results.GroupBy(delay => delay).All(repeats => repeats.Count() < repeatThreshold).Should().BeTrue();
        }

        [TestMethod]
        public void NextCanBeSameAsPrevious()
        {
            const int oneInNChance = 10;
            var sut = new ConstrainedRandom(oneInNChance);

            var previous = sut.Next;
            for (var i = 0; i < oneInNChance * oneInNChance; i++)
            {
                var current = sut.Next;
                if (previous == current)
                {
                    Console.WriteLine("{0} / {1}", previous, current);
                    return;
                }

            }

            Assert.Fail();
        }
    }
}
