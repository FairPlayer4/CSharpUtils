using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;
using Xunit;

namespace CSharpUtilsNETStandardTest.Utils.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("?dasdas!", "?", "!", "dasdas", true)]
        [InlineData("fdg?dasdas!gsdfg", "?", "!", "dasdas", true)]
        [InlineData("fdgdasdasgsdfg", "?", "!", "", false)]
        public void FindInBetweenTwoStrings_ShouldWorkCorrectly([NotNull] string fullString, [NotNull] string startString, [NotNull] string endString, string expectedString, bool expectedSuccess)
        {
            string actual = fullString.FindInBetweenTwoStrings(startString, endString, out bool success);
            Assert.Equal(expectedString, actual);
            Assert.Equal(expectedSuccess, success);
        }
    }
}
