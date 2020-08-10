#region Imports

using System;
using System.Globalization;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    [PublicAPI]
    public class DoubleNumberFormat
    {
        [NotNull]
        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
        public int PrecisionDigits { get; set; } = 15;
        public bool TrimZeros { get; set; } = true;
        public int MinimumNumberOfDigits { get; set; }
        public bool AlwaysShowSeparator { get; set; } = true;

        public bool AllowENotation { get; set; }
        public double UseENotationIfSmallerThan { get; set; } = 0.00001;
        public double UseENotationIfLargerThan { get; set; } = 99999;
        public bool TrimZerosAfterE { get; set; } = true;
        public int MinimumNumberOfZerosAfterE { get; set; } = 1;

        [NotNull]
        public static DoubleNumberFormat GetDefaultWithoutENotation() => new DoubleNumberFormat();

        [NotNull]
        public static DoubleNumberFormat GetDefaultWithENotation() => new DoubleNumberFormat { AllowENotation = true };

        [NotNull]
        public string ConvertToString(double number)
        {
            return number.ToStringFormat(CultureInfo, AllowENotation, UseENotationIfSmallerThan, UseENotationIfLargerThan, PrecisionDigits, TrimZeros, MinimumNumberOfDigits, TrimZerosAfterE, MinimumNumberOfZerosAfterE, AlwaysShowSeparator);
        }
    }

    public static class CultureExtensions
    {
        [NotNull]
        public static string ToStringInvariant<T>([NotNull] this T obj) where T : IConvertible
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        [NotNull]
        public static string ToStringFormat(this double number, [CanBeNull] CultureInfo cultureInfo = null, bool allowENotation = false, double useENotationIfSmallerThan = 0.01, double useENotationIfLargerThan = 9999, int precisionDigits = 15, bool trimZeros = true, int minimumNumberOfDigits = 0, bool trimZerosAfterE = true, int minimumNumberOfZerosAfterE = 1, bool alwaysShowSeparator = true)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.InvariantCulture;
            bool isInvariantCulture = cultureInfo.Equals(CultureInfo.InvariantCulture);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (isInvariantCulture && number == default) return alwaysShowSeparator ? "0." + new string('0', minimumNumberOfDigits) : "0";
            if (useENotationIfSmallerThan < 0) useENotationIfSmallerThan = -useENotationIfSmallerThan;
            if (useENotationIfLargerThan < 0) useENotationIfLargerThan = -useENotationIfLargerThan;
            bool useENotation = allowENotation && (number < useENotationIfSmallerThan || number > useENotationIfLargerThan || -number > -useENotationIfSmallerThan || -number < -useENotationIfLargerThan);
            if (useENotation) return number.ToStringWithENotation(cultureInfo, precisionDigits, true, trimZeros, minimumNumberOfDigits, trimZerosAfterE, minimumNumberOfZerosAfterE);
            string returnValue = number.ToString("F" + precisionDigits, cultureInfo);
            if (trimZeros) returnValue = returnValue.TrimEnd('0');
            string numberSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator;
            if (returnValue.Length == 0) return alwaysShowSeparator ? string.Format("0{0}{1}", numberSeparator, new string('0', minimumNumberOfDigits)) : "0";
            minimumNumberOfDigits = GetMinimumNumberOfDigitsAfterSeparator(returnValue, minimumNumberOfDigits, numberSeparator);
            if (returnValue.Length < minimumNumberOfDigits) returnValue = returnValue + new string('0', minimumNumberOfDigits - returnValue.Length);
            if (char.IsDigit(returnValue[returnValue.Length - 1])) return returnValue;
            if (isInvariantCulture) return alwaysShowSeparator ? returnValue + "0" : returnValue.Remove(returnValue.Length - 1);
            if (alwaysShowSeparator) return string.Format("{0}{1}{2}", returnValue, numberSeparator, 0.ToString(cultureInfo));
            return returnValue.Remove(returnValue.Length - numberSeparator.Length - 1, numberSeparator.Length);
        }

        private static int GetMinimumNumberOfDigitsAfterSeparator([NotNull] string value, int minimumNumberOfDigits, [NotNull] string numberSeparator)
        {
            int separatorLength = numberSeparator.Length;
            int indexOfSeparator = value.IndexOf(numberSeparator, StringComparison.Ordinal);
            int indexAfterSeparator = indexOfSeparator + separatorLength;
            return minimumNumberOfDigits + indexAfterSeparator;
        }

        [NotNull]
        public static string ToStringInvariantAlwaysShowSeparator(this double value)
        {
            string stringInvariant = value.ToStringInvariant();
            if (!stringInvariant.Contains(".")) stringInvariant = stringInvariant + ".0";
            return stringInvariant;
        }
        [NotNull]
        public static string ToStringInvariantAlwaysShowSeparator(this float value)
        {
            string stringInvariant = value.ToStringInvariant();
            if (!stringInvariant.Contains(".")) stringInvariant = stringInvariant + ".0";
            return stringInvariant;
        }
        [NotNull]
        public static string ToStringWithENotation(this double value, [CanBeNull] CultureInfo cultureInfo = null, int precisionDigits = 15, bool largeE = true, bool trimZerosBeforeE = true, int minimumNumberOfDigits = 6, bool trimZerosAfterE = true, int minimumNumberOfZerosAfterE = 1)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.InvariantCulture;
            if (precisionDigits < 1) precisionDigits = 1;
            if (minimumNumberOfDigits < 0) minimumNumberOfDigits = 0;
            if (minimumNumberOfZerosAfterE < 0) minimumNumberOfZerosAfterE = 0;
            char e = largeE ? 'E' : 'e';
            string returnValue = value.ToString(string.Format("{0}{1}", e, precisionDigits), cultureInfo);
            minimumNumberOfDigits = GetMinimumNumberOfDigitsAfterSeparator(returnValue, minimumNumberOfDigits, cultureInfo.NumberFormat.NumberDecimalSeparator);
            int indexOfE = returnValue.IndexOf(e);
            if (indexOfE <= 0) return returnValue;
            if (indexOfE < minimumNumberOfDigits)
            {
                //fill
                returnValue = returnValue.Insert(indexOfE, new string('0', minimumNumberOfDigits - indexOfE));
            }
            else if (trimZerosBeforeE && indexOfE > minimumNumberOfDigits)
            {
                int index = indexOfE;
                int count = 0;
                while (index > minimumNumberOfDigits && returnValue[index - 1].Equals('0'))
                {
                    index--;
                    count++;
                }
                if (count > 0) returnValue = returnValue.Remove(index, count);
            }
            if (trimZerosAfterE)
            {
                int index = returnValue.IndexOf(e);
                if (index <= 0) return returnValue;
                while (index < returnValue.Length && !char.IsDigit(returnValue[index])) index++; //Move after all non digits e.g. - or +
                int startingIndex = index; //First digit or out of bounds
                int count = 0;
                while (index < returnValue.Length && returnValue[index].Equals('0'))
                {
                    index++;
                    count++;
                }
                if (index == returnValue.Length) return returnValue.Remove(startingIndex) + new string('0', minimumNumberOfZerosAfterE);
                if (count > minimumNumberOfZerosAfterE) return returnValue.Remove(startingIndex, count - minimumNumberOfZerosAfterE);
                if (count < minimumNumberOfZerosAfterE) return returnValue.Insert(startingIndex, new string('0', minimumNumberOfZerosAfterE - count));
            }

            return returnValue;
        }

    }
}
