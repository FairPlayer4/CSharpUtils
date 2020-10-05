#region Imports

using System;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    [PublicAPI]
    public static class NumberExtensions
    {
        /// <summary>
        /// <see cref="ToIntSafeBounded(double,int,bool)"/>
        /// </summary>
        // This is valid because if the long is so large (small) that it cannot be represented by double with accuracy then it will become the maximal (minimal) integer anyway.
        public static int ToIntSafeBounded(this long number) => ((double)number).ToIntSafeBounded();

        /// <summary>
        /// <see cref="ToIntSafeBounded(double,int,bool)"/>
        /// </summary>
        public static int ToIntSafeBounded(this float number, int valueInCaseOfNaN = 0, bool round = false) => ((double)number).ToIntSafeBounded(valueInCaseOfNaN, round);

        /// <summary>
        /// Transforms a double to an integer without an exception.
        /// Any value greater than int.MaxValue will be transformed to int.MaxValue.
        /// Any value lesser than int.MinValue will be transformed to int.MinValue.
        /// </summary>
        public static int ToIntSafeBounded(this double number, int valueInCaseOfNaN = 0, bool round = false)
        {
            if (double.IsNaN(number)) return valueInCaseOfNaN; //Safe behaviour
            if (round) number = Math.Round(number, MidpointRounding.AwayFromZero);
            if (number > int.MaxValue) return int.MaxValue;
            if (number < int.MinValue) return int.MinValue;
            return (int)number;
        }

        public static bool IsShort(this int number) => number >= short.MinValue && number <= short.MaxValue;

        public static bool IsInt(this long number) => number >= int.MinValue && number <= int.MaxValue;

        public static bool IsInt(this float number, double epsilon = 0D) => number.AsDouble().IsInt(epsilon);

        public static bool IsInt(this double number, double epsilon = 0D) => number - Math.Floor(number) <= epsilon;

        public static bool IsFinite(this float number) => number.AsDouble().IsFinite();

        public static bool IsFinite(this double number) => !double.IsNaN(number) && !double.IsInfinity(number);

        public static bool IsGreaterZero(this int number) => number > 0;

        public static bool IsZeroOrSmaller(this int number) => number <= 0;

        public static long AsLong(this int number) => number;

        public static double AsDouble(this int number) => number;

        public static double AsDouble(this float number) => number;
    }
}
