using CSharpUtilsNETStandard.Utils.Extensions.General;
using Xunit;

namespace CSharpUtilsNETStandardTest.Utils.Extensions
{
    public class NumberExtensionsTests
    {
        [Theory]
        [InlineData(3L, 3)]
        [InlineData(-2L, -2)]
        [InlineData(0L, 0)]
        [InlineData(long.MaxValue, int.MaxValue)]
        [InlineData(long.MinValue, int.MinValue)]
        public void ToIntSafeBounded_LongShouldWorkCorrectly(long number, int expected)
        {
            Assert.Equal(expected, number.ToIntSafeBounded());
        }

        [Theory]
        [InlineData(3F, 3)]
        [InlineData(-2F, -2)]
        [InlineData(0.9F, 0)]
        [InlineData(0.0F, 0)]
        [InlineData(-2.02315422323F, -2)]
        [InlineData(float.MaxValue, int.MaxValue)]
        [InlineData(float.MinValue, int.MinValue)]
        [InlineData(float.NaN, 0)]
        [InlineData(float.PositiveInfinity, int.MaxValue)]
        [InlineData(float.NegativeInfinity, int.MinValue)]
        [InlineData(float.Epsilon, 0)]
        public void ToIntSafeBounded_FloatShouldWorkCorrectly(float number, int expected)
        {
            Assert.Equal(expected, number.ToIntSafeBounded());
        }

        [Theory]
        [InlineData(3D, 3)]
        [InlineData(-2D, -2)]
        [InlineData(0.9D, 0)]
        [InlineData(0.0D, 0)]
        [InlineData(-2.02315422323D, -2)]
        [InlineData(double.MaxValue, int.MaxValue)]
        [InlineData(double.MinValue, int.MinValue)]
        [InlineData(double.NaN, 0)]
        [InlineData(double.PositiveInfinity, int.MaxValue)]
        [InlineData(double.NegativeInfinity, int.MinValue)]
        [InlineData(double.Epsilon, 0)]
        public void ToIntSafeBounded_DoubleShouldWorkCorrectly(double number, int expected)
        {
            Assert.Equal(expected, number.ToIntSafeBounded());
        }

        [Theory]
        [InlineData(3.5, 4)]
        [InlineData(-2.1, -2)]
        [InlineData(0.9, 1)]
        [InlineData(-0.45, 0)]
        [InlineData(-2.02315422323D, -2)]
        [InlineData(double.MaxValue, int.MaxValue)]
        [InlineData(double.MinValue, int.MinValue)]
        [InlineData(double.NaN, 0)]
        [InlineData(double.PositiveInfinity, int.MaxValue)]
        [InlineData(double.NegativeInfinity, int.MinValue)]
        [InlineData(double.Epsilon, 0)]
        public void ToIntSafeBounded_RoundingShouldWorkCorrectly(double number, int expected)
        {
            Assert.Equal(expected, number.ToIntSafeBounded(round:true));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(-2)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ToIntSafeBounded_NaNShouldWorkCorrectly(int valueInCaseOfNaN)
        {
            Assert.Equal(valueInCaseOfNaN, double.NaN.ToIntSafeBounded(valueInCaseOfNaN));
            Assert.Equal(valueInCaseOfNaN, float.NaN.ToIntSafeBounded(valueInCaseOfNaN));
        }
    }
}
