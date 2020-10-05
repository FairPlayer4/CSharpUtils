using System;
using System.Linq;
using CSharpUtilsNETStandard.Utils;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using Xunit;

namespace CSharpUtilsNETStandardTest.Utils
{
    public class IntRangeTests
    {
        [Theory]
        [InlineData(3, -3)]
        [InlineData(-2, -3)]
        [InlineData(3, 2)]
        [InlineData(int.MaxValue, int.MinValue)]
        public void NewIntRange_Exception(int min, int max)
        {
            Assert.Throws<ArgumentException>(() => new IntRange(min, max));
            Assert.Throws<ArgumentException>(() => IntRange.Get(min, max));
        }

        [Theory]
        [InlineData(0, -1)]
        [InlineData(2, int.MaxValue)]
        [InlineData(int.MinValue, long.MaxValue)]
        public void GetWithCount_Exception(int min, long count)
        {
            Assert.Throws<ArgumentException>(() => IntRange.GetWithCount(min, count));
        }

        [Theory]
        [InlineData(-3, 3, 6)]
        [InlineData(-2, 3, 5)]
        [InlineData(-3, -2, 1)]
        [InlineData(-10, -10, 0)]
        [InlineData(0, 0, 0)]
        [InlineData(int.MinValue, int.MaxValue, uint.MaxValue)]
        public void TotalDistanceAndCount_ShouldBeCorrect(int min, int max, long expected)
        {
            Assert.Equal(expected, IntRange.Get(min, max).TotalDistance);
            Assert.Equal(expected + 1, IntRange.Get(min, max).LongCount);
        }

        [Theory]
        [InlineData(-2, 2, new[] { -2, -1, 0, 1, 2 })]
        [InlineData(0, 0, new[] { 0 })]
        public void Enumeration_ShouldBeCorrect(int min, int max, int[] expected)
        {
            Assert.Equal(expected, IntRange.Get(min, max).ToArray());
        }

        [Theory]
        [InlineData(-3, 3, 7)]
        [InlineData(-3, 3, -3)]
        [InlineData(0, int.MaxValue, long.MaxValue)]
        public void Indexing_Exception(int min, int max, long index)
        {
            Assert.Throws<IndexOutOfRangeException>(() => IntRange.Get(min, max)[index]);
        }

        [Theory]
        [InlineData(-3, 3, 6, 3)]
        [InlineData(-3, 3, 0, -3)]
        [InlineData(-3, 3, 3, 0)]
        [InlineData(int.MinValue, int.MaxValue, int.MaxValue, -1)]
        public void SimpleIndexing_ShouldBeCorrect(
            int min,
            int max,
            long index,
            int expected
        )
        {
            Assert.Equal(expected, IntRange.Get(min, max)[index]);
        }

        [Fact]
        public void TotalDistance_StaticShouldBeCorrect()
        {
            Assert.Equal(int.MaxValue.AsLong() - int.MinValue, IntRange.MaxRange.TotalDistance);
            Assert.Equal(int.MaxValue.AsLong(), IntRange.NonNegativeRange.TotalDistance);
            Assert.Equal(int.MaxValue.AsLong() - 1, IntRange.PositiveRange.TotalDistance);
            Assert.Equal(-int.MinValue.AsLong() - 1, IntRange.NegativeRange.TotalDistance);
            Assert.Equal(int.MaxValue.AsLong() - int.MinValue + 1, IntRange.MaxRange.LongCount);
            Assert.Equal(int.MaxValue.AsLong() + 1, IntRange.NonNegativeRange.LongCount);
            Assert.Equal(int.MaxValue.AsLong(), IntRange.PositiveRange.LongCount);
            Assert.Equal(-int.MinValue.AsLong(), IntRange.NegativeRange.LongCount);
        }
    }
}
