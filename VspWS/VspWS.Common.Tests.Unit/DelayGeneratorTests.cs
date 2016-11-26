using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

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

            for(var i = 0; i < 1000; i++)
            {
                sut.Milliseconds.Should().BeLessOrEqualTo(maximumDelay);
            }
        }
    }
}
