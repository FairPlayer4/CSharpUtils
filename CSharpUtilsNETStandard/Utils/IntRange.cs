#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils
{
    /// <summary>
    /// A integer range that can be used to simplify logic that checks whether an integer is between two integers.
    /// </summary>
    [PublicAPI]
    public readonly struct IntRange : IEnumerable<int>
    {
        public readonly int MinimumNumber;
        public readonly int MaximumNumber;

        public const long MaxCount = uint.MaxValue + 1L;

        public static readonly IntRange MaxRange = new IntRange(int.MinValue, int.MaxValue);
        public static readonly IntRange NonNegativeRange = new IntRange(0, int.MaxValue);
        public static readonly IntRange PositiveRange = new IntRange(1, int.MaxValue);
        public static readonly IntRange NegativeRange = new IntRange(int.MinValue, -1);

        public bool OnlyNegative => MaximumNumber < 0;

        public bool OnlyNonNegative => MinimumNumber >= 0;

        public bool OnlyPositive => MinimumNumber > 0;

        [ValueRange(0, MaxCount - 1)]
        public long TotalDistance => MaximumNumber.AsLong() - MinimumNumber;

        [ValueRange(1, MaxCount)]
        public long LongCount => TotalDistance + 1;

        /// <summary>
        /// Requirements: <c>minimumNumber &lt;= maximumNumber</c>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static IntRange Get(int minimumNumber, int maximumNumber) => new IntRange(minimumNumber, maximumNumber);

        /// <summary>
        /// Requirements: <c>count &gt;= 0 &#38;&#38; count &lt;= int.MaxValue &#38;&#38; minimumNumber + count - 1 &lt;= int.MaxValue</c>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static IntRange GetWithCount(int minimumNumber, [ValueRange(0, MaxCount)] long count)
        {
            long maximumNumber = minimumNumber + count - 1;
            if (count < 0 || count > int.MaxValue || maximumNumber > int.MaxValue)
                throw new ArgumentException(string.Format("The {0} ({1}) must be greater than 0 and smaller than or equal to {2} ({3}). Also the sum ({4}) of {5} ({6}) and {0} ({1}) must be smaller than or equal to {2} ({3}).",
                                                          nameof(count),
                                                          count,
                                                          nameof(int.MaxValue),
                                                          int.MaxValue,
                                                          minimumNumber + count,
                                                          nameof(minimumNumber),
                                                          minimumNumber), nameof(count));
            return new IntRange(minimumNumber, (int)maximumNumber);
        }

        /// <summary>
        /// Requirements: <c>minimumNumber &lt;= maximumNumber</c>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public IntRange(int minimumNumber, int maximumNumber)
        {
            if (minimumNumber > maximumNumber)
                throw new ArgumentException($"The {nameof(minimumNumber)} ({minimumNumber}) must be smaller or equal to the {nameof(maximumNumber)} ({maximumNumber}).", nameof(minimumNumber));
            MinimumNumber = minimumNumber;
            MaximumNumber = maximumNumber;
        }

        [Pure]
        public bool IsInRange(int number) => number >= MinimumNumber && number <= MaximumNumber;

        [Pure]
        public bool IsInRange(IntRange intRange) => intRange.MinimumNumber >= MinimumNumber && intRange.MaximumNumber <= MaximumNumber;

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = MinimumNumber; i <= MaximumNumber; i++) yield return i;
        }

        /// <returns>[MinimumNumber, MaximumNumber]</returns>
        [NotNull]
        public override string ToString() => $"[{MinimumNumber}, {MaximumNumber}]";

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Requirements: <c>index &gt;= 0 &#38;&#38; index &lt; LongCount</c>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        public int this[[ValueRange(0, MaxCount)] long index]
        {
            get
            {
                if (index < 0 || index >= LongCount)
                    throw new IndexOutOfRangeException($"The {nameof(index)} ({index}) must be greater than 0 and smaller than {nameof(LongCount)} ({LongCount}).");
                return (int)(MinimumNumber + index);
            }
        }
    }
}
