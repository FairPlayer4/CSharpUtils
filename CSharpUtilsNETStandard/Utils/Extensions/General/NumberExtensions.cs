#region Imports

using System;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    public static class NumberExtensions
    {

        public static int ToIntSafeBounded(this long number) => ((double)number).ToIntSafeBounded();

        public static int ToIntSafeBounded(this float number, bool round = false) => ((double)number).ToIntSafeBounded(round);

        public static int ToIntSafeBounded(this double number, bool round = false)
        {
            if (round) number = Math.Round(number, MidpointRounding.AwayFromZero);
            if (number > int.MaxValue) return int.MaxValue;
            if (number < int.MinValue) return int.MinValue;
            return (int)number;
        }

        public static bool IsShort(this int number) => number >= short.MinValue && number <= short.MaxValue;

        public static bool IsInt(this long number) => number >= int.MinValue && number <= int.MaxValue;

        public static bool IsGreaterZero(this int number) => number > 0;

        public static bool IsZeroOrSmaller(this int number) => number <= 0;
    }
}
