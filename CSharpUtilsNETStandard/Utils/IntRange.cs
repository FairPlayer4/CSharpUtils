#region Imports

using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils
{
    [PublicAPI]
    public readonly struct IntRange
    {
        public readonly int MinimumNumber;
        public readonly int MaximumNumber;

        public static readonly IntRange MaxRange = new IntRange(int.MinValue, int.MaxValue);
        public static readonly IntRange PositiveRange = new IntRange(0, int.MaxValue);
        public static readonly IntRange NegativeRange = new IntRange(int.MinValue, -1);

        public IntRange(int minimumNumber, int maximumNumber)
        {
            MinimumNumber = minimumNumber;
            MaximumNumber = maximumNumber;
        }

        public bool IsValid => MinimumNumber <= MaximumNumber;

        public bool OnlyNegative => MaximumNumber < 0;

        public bool OnlyPositive => MinimumNumber >= 0;

        [Pure]
        public bool IsInRange(int number)
        {
            return number >= MinimumNumber && number <= MaximumNumber;
        }

        public int TotalDistance => MaximumNumber - MinimumNumber;

        [NotNull]
        public override string ToString()
        {
            return string.Format("Range [{0}, {1}]", MinimumNumber, MaximumNumber);
        }
    }
}
