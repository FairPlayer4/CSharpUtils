#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    [PublicAPI]
    public static class StringExtensions
    {

        //https://stackoverflow.com/a/17253735
        /// <summary>
        /// takes a substring between two anchor strings (or the end of the string if that anchor is null)
        /// </summary>
        /// <param name="fullString">a string</param>
        /// <param name="startString">an optional string to search after</param>
        /// <param name="endString">an optional string to search before</param>
        /// <param name="comparison">an optional comparison for the search</param>
        /// <returns>a substring based on the search</returns>
        [NotNull]
        public static string FindInBetweenTwoStrings([NotNull]this string fullString, [NotNull]string startString, [NotNull]string endString, StringComparison comparison = StringComparison.InvariantCulture)
        {
            return FindInBetweenTwoStrings(fullString, startString, endString, out bool _, comparison);
        }

        [NotNull]
        public static string FindInBetweenTwoStrings([NotNull]this string fullString, [NotNull]string startString, [NotNull]string endString, out bool success, StringComparison comparison = StringComparison.InvariantCulture)
        {
            int fromLength = startString.Length;
            int startIndex = fromLength == 0 ? 0 : fullString.IndexOf(startString, comparison) + fromLength;

            if (startIndex < fromLength)
            {
                success = false;
                return "";
            }

            int endIndex = endString.Length == 0 ? fullString.Length : fullString.IndexOf(endString, startIndex, comparison);

            if (endIndex < startIndex)
            {
                success = false;
                return "";
            }
            success = true;
            string subString = fullString.Substring(startIndex, endIndex - startIndex);
            return subString;
        }

        public static bool EqualsInvariantIgnoreCase([CanBeNull] this string value, [CanBeNull]string other)
        {
            return string.Equals(value, other, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqualsIgnoreCase([CanBeNull] this string value, [CanBeNull]string other)
        {
            return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsIgnoreCase([NotNull] this string value, [NotNull]string other)
        {
            return value.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool MatchesFilter([NotNull] this string value, [NotNull]string filter)
        {
            return value.ContainsIgnoreCase(filter);
        }

        [NotNull]
        public static string FindInBetweenTwoStrings([NotNull] this string fullString, (string, string) startAndEndString, StringComparison comparison = StringComparison.InvariantCulture, bool returnEmptyIfOneOfTheStringsIsNotFound = true)
        {
            return FindInBetweenTwoStrings(fullString, startAndEndString.Item1, startAndEndString.Item2, comparison);
        }
        [CanBeNull]
        public static (string, string)? StartsWithAndEndWithAny([NotNull]this string value, [NotNull]params (string, string)?[] startsWithAndEndWithTuples)
        {
            return startsWithAndEndWithTuples.FirstOrDefault(tuple => tuple.HasValue && value.StartsAndEndsWith(tuple.Value));
        }

        public static bool StartsAndEndsWith([NotNull]this string value, (string, string) startAndEnd)
        {
            return value.StartsWithOrdinal(startAndEnd.Item1) && value.EndsWithOrdinal(startAndEnd.Item2);
        }

        public static bool StartsAndEndsWith([NotNull]this string value, [NotNull]string startAndEnd)
        {
            return value.StartsWithOrdinal(startAndEnd) && value.EndsWithOrdinal(startAndEnd);
        }

        public static bool StartsWithOrdinal([NotNull]this string value, [NotNull]string start)
        {
            return value.StartsWith(start, StringComparison.Ordinal);
        }

        public static bool EndsWithOrdinal([NotNull]this string value, [NotNull]string end)
        {
            return value.EndsWith(end, StringComparison.Ordinal);
        }
        [NotNull]
        public static string RemoveMultipleSpaces([NotNull]this string value)
        {
            return string.Join(" ", value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
        [NotNull]
        public static string ReplaceAllSpacesNewLineTabsEtcWithSingleSpace([NotNull]this string value)
        {
            return Regex.Replace(value, @"\s+", " ");
        }
        [NotNull]
        public static string TrimStartOnce([NotNull]this string target, [NotNull]string trimString)
        {
            return target.StartsWith(trimString, StringComparison.Ordinal) ? target.Substring(trimString.Length) : target;
        }

        //https://stackoverflow.com/a/1857525
        [NotNull]
        public static string GetUntilOrEmpty([NotNull]this string text, [NotNull]string stopAt)
        {
            int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);
            return charLocation > 0 ? text.Substring(0, charLocation) : "";
        }

        [NotNull]
        public static string GetPrintString([CanBeNull]this string value, [NotNull]string onNull, [NotNull]string onEmptyOrWhitespace)
        {
            if (value == null) return onNull;
            if (string.IsNullOrWhiteSpace(value)) return onEmptyOrWhitespace;
            return value;
        }

        [SourceTemplate]
        public static bool IsNullOrEmpty([CanBeNull]this string value) => string.IsNullOrEmpty(value);

        [SourceTemplate]
        public static bool IsNullOrWhiteSpace([CanBeNull]this string value) => string.IsNullOrWhiteSpace(value);

        [NotNull]
        public static string ToStringFromCharArray([NotNull]this char[] charArray) => new string(charArray);

        [NotNull]
        public static string ToStringFromChars([NotNull]this IEnumerable<char> chars) => chars.ToArray().ToStringFromCharArray();

        [NotNull]
        public static string RemoveWhiteSpaceCharacters([NotNull] this string input)
        {
            return input.Where(c => !char.IsWhiteSpace(c)).ToStringFromChars();
        }

        /// <summary>
        /// This method adds a space in front of every captial letter except the first one and also groups of captial letters are kept together.
        /// If the last character is upper case then no space is added in front of it.
        /// Also no space is added if the previous character is not a letter e.g. a number, a symbol or other character like space.
        /// Adapted from https://stackoverflow.com/a/272929
        /// </summary>
        [NotNull]
        public static string AddSpaceBeforeCapitalLettersExcludeAcronyms([CanBeNull] this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                char previousChar = text[i - 1];
                char currentChar = text[i];
                if (char.IsUpper(currentChar) && char.IsLetter(previousChar) && char.IsLower(previousChar) && i < text.Length - 1)
                {
                    newText.Append(' ');
                }
                newText.Append(currentChar);
            }
            return newText.ToString();
        }
    }
}
