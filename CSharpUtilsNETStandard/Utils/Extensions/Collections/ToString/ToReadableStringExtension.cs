#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.Collections.ToString
{
    [CanBeNull]
    public delegate string ToStringDelegate<in T>([CanBeNull] T value);

    [PublicAPI]
    public static class ToReadableStringExtension
    {
        public const string NullString = "NULL";

        public const string DefaultFormat = "G";

        /// <summary>
        /// Converts enumerables (Dictionaries, Lists, Sets,...) to a readable string.
        /// Works with nested enumerables like the nested Dictionaries or Lists.
        /// Any mixture or nesting of enumerables will be converted to a somewhat readable string.
        /// Without arguments dictionaries are returned as [x = a, y = b, ...]
        /// Without arguments other collections are returned as [x, y, ...]
        /// Is not really efficient but very simple.
        /// Useful for logging and debugging e.g. checking the content of nested collections.
        /// Objects that do not implement IDictionary or IEnumerable will be converted using the regular ToString() method.
        /// If the enumerable is null or contains values that are null they are printed as "NULL".
        /// If this method is used with further arguments it makes sense to use the class CollectionPrinter.
        /// </summary>
        /// <param name="enumerable">An IEnumerable.</param>
        /// <param name="nestedEnumerablePrintFormatsQueue">A queue that provides formatting <see cref="EnumerablePrintFormat"/> for nested enumerables. For each enumerable a dequeue is performed and if the queue is null or empty the default format will be used.</param>
        /// <param name="typeToStringMethod">A dictionary that maps types to functions. These functions will be used on mapped types instead of ToString(). This allows to define a custom ToString() methods for certain types that do not have a suitable (too little or too much information) ToString() method.</param>
        /// <returns>A readable version of the enumerable displaying the content</returns>
        [NotNull]
        public static string ToReadableStringNested(
            [CanBeNull, ItemCanBeNull] this IEnumerable enumerable,
            [CanBeNull, ItemCanBeNull]Queue<EnumerablePrintFormat> nestedEnumerablePrintFormatsQueue = null,
            [CanBeNull] Dictionary<Type, ToStringDelegate<object>> typeToStringMethod = null
        )
        {
            if (enumerable == null) return NullString;
            //If ToString() is overridden then use that (e.g. the class String implements IEnumerable but also overrides the ToString() method from Object)
            string enumerableToString = enumerable.ToString();
            if (enumerable.GetType().ToString() != enumerableToString && enumerableToString != null) return enumerableToString;
            nestedEnumerablePrintFormatsQueue = nestedEnumerablePrintFormatsQueue ?? new Queue<EnumerablePrintFormat>();
            if (nestedEnumerablePrintFormatsQueue.Count == 0) nestedEnumerablePrintFormatsQueue.Enqueue(new EnumerablePrintFormat());
            EnumerablePrintFormat printFormat = nestedEnumerablePrintFormatsQueue.Dequeue() ?? new EnumerablePrintFormat();
            string multiLineString = "";
            if (printFormat.NewLinesEachElement)
                for (int i = 0; i < printFormat.NumLinesEachElement; i++)
                    multiLineString += "\n";

            StringBuilder result = new StringBuilder(printFormat.StartString + multiLineString);
            if (enumerable is IDictionary dictionary)
                foreach (DictionaryEntry keyValue in dictionary)
                {
                    result.AppendFormat("{0}{1}{2}{3}{4}",
                                        keyValue.Key is IEnumerable keyEnumerable ? keyEnumerable.ToReadableStringNested() : typeToStringMethod.ToStringCustom(keyValue.Key),
                                        printFormat.KeyValueDelimiter,
                                        keyValue.Value is IEnumerable valueEnumerable ? valueEnumerable.ToReadableStringNested(new Queue<EnumerablePrintFormat>(nestedEnumerablePrintFormatsQueue), typeToStringMethod) : typeToStringMethod.ToStringCustom(keyValue.Value),
                                        printFormat.ElementDelimiter,
                                        multiLineString);
                }
            else
                foreach (object element in enumerable)
                {
                    result.AppendFormat("{0}{1}{2}",
                                        element is IEnumerable elementEnumerable ? elementEnumerable.ToReadableStringNested(new Queue<EnumerablePrintFormat>(nestedEnumerablePrintFormatsQueue), typeToStringMethod) : typeToStringMethod.ToStringCustom(element),
                                        printFormat.ElementDelimiter,
                                        multiLineString);
                }

            result.Append(printFormat.EndString);
            return result.ToString().Replace(printFormat.ElementDelimiter + multiLineString + printFormat.EndString, multiLineString + printFormat.EndString);
        }

        [NotNull]
        public static string ToStringConsiderEnumerable<T>([NotNull] this T value)
        {
            if (value is IEnumerable enumerable) return enumerable.ToReadableStringNested();
            return value.ToString() ?? NullString;
        }

        [NotNull]
        private static string ToStringCustom<T>(
            [CanBeNull] this IDictionary<Type, ToStringDelegate<object>> typeToStringMethod,
            [CanBeNull] T obj
        )
        {
            if (obj == null) return NullString;
            if (typeToStringMethod == null || !typeToStringMethod.TryGetValue(obj.GetType(), out ToStringDelegate<object> toStringMethod) || toStringMethod == null) toStringMethod = NonNullToString<object>(null);
            return toStringMethod(obj) ?? NullString;
        }

        [NotNull]
        private static ToStringDelegate<T> NonNullToString<T>([CanBeNull] ToStringDelegate<T> toStringMethod)
        {
            if (toStringMethod == null) return obj => obj == null ? NullString : obj.ToString() ?? NullString;
            return obj => obj == null ? NullString : toStringMethod(obj);
        }

        /// <summary>
        /// <see cref="ToReadableStringNested"/>
        /// </summary>
        [NotNull]
        public static string ToReadableString<T>(
            [CanBeNull, ItemCanBeNull] this IEnumerable<T> enumerable,
            [CanBeNull] EnumerablePrintFormat enumerablePrintFormat = null,
            [CanBeNull]ToStringDelegate<T> toStringMethod = null
        )
        {
            if (enumerable == null) return NullString;
            //If ToString() is overridden then use that (e.g. the class String implements IEnumerable but also overrides the ToString() method from Object)
            string enumerableToString = enumerable.ToString();
            if (enumerableToString != enumerable.GetType().ToString() && enumerableToString != null) return enumerableToString;
            enumerablePrintFormat = enumerablePrintFormat ?? new EnumerablePrintFormat();
            ToStringDelegate<T> safeToStringMethod = NonNullToString(toStringMethod);
            return ToStringInternal(enumerable, enumerablePrintFormat, safeToStringMethod);
        }

        //Don't provide an invalid Format! This may cause an exception!
        //Default FormatProvider is InvariantCulture
        [NotNull]
        public static string ToReadableStringFormat<T>(
            [CanBeNull, ItemCanBeNull] this IEnumerable<T> enumerable,
            [CanBeNull] EnumerablePrintFormat enumerablePrintFormat = null,
            [NotNull] string format = DefaultFormat,
            [CanBeNull] IFormatProvider formatProvider = null
        )
            where T : IFormattable
        {
            if (enumerable == null) return NullString;
            //If ToString() is overridden then use that (e.g. the class String implements IEnumerable but also overrides the ToString() method from Object)
            string enumerableToString = enumerable.ToString();
            if (enumerableToString != enumerable.GetType().ToString() && enumerableToString != null) return enumerableToString;
            enumerablePrintFormat = enumerablePrintFormat ?? new EnumerablePrintFormat();
            if (string.IsNullOrWhiteSpace(format)) format = DefaultFormat;
            formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
            string ToStringMethod(T arg) => arg == null ? NullString : arg.ToString(format, formatProvider);
            return ToStringInternal(enumerable, enumerablePrintFormat, ToStringMethod);
        }

        [NotNull]
        private static string ToStringInternal<T>(
            [NotNull, ItemCanBeNull] this IEnumerable<T> enumerable,
            [NotNull] EnumerablePrintFormat enumerablePrintFormat,
            [NotNull] ToStringDelegate<T> toStringMethod
        )
        {
            string multiLineString = "";
            if (enumerablePrintFormat.NewLinesEachElement)
                for (int i = 0; i < enumerablePrintFormat.NumLinesEachElement; i++)
                    multiLineString += "\n";

            StringBuilder result = new StringBuilder(enumerablePrintFormat.StartString + multiLineString);
            foreach (T element in enumerable)
            {
                result.AppendFormat("{0}{1}{2}",
                                    toStringMethod(element) ?? NullString,
                                    enumerablePrintFormat.ElementDelimiter,
                                    multiLineString);
            }
            result.Append(enumerablePrintFormat.EndString);
            return result.ToString().Replace(enumerablePrintFormat.ElementDelimiter + multiLineString + enumerablePrintFormat.EndString, multiLineString + enumerablePrintFormat.EndString);
        }

        [NotNull]
        public static string ToReadableString<TKey, TValue>(
            [CanBeNull] this IDictionary<TKey, TValue> dictionary,
            [CanBeNull] EnumerablePrintFormat enumerablePrintFormat = null,
            [CanBeNull] ToStringDelegate<TKey> keyToStringMethod = null,
            [CanBeNull] ToStringDelegate<TValue> valueToStringMethod = null
        )
        {
            if (dictionary == null) return "NULL";
            //If ToString() is overridden then use that (e.g. the class String implements IEnumerable but also overrides the ToString() method from Object)
            string dictionaryToString = dictionary.ToString();
            if (dictionaryToString != dictionary.GetType().ToString() && dictionaryToString != null) return dictionaryToString;
            enumerablePrintFormat = enumerablePrintFormat ?? new EnumerablePrintFormat();
            ToStringDelegate<TKey> safeKeyToStringMethod = NonNullToString(keyToStringMethod);
            ToStringDelegate<TValue> safeValueToStringMethod = NonNullToString(valueToStringMethod);
            string multiLineString = "";
            if (enumerablePrintFormat.NewLinesEachElement)
                for (int i = 0; i < enumerablePrintFormat.NumLinesEachElement; i++)
                    multiLineString += "\n";

            StringBuilder result = new StringBuilder(enumerablePrintFormat.StartString + multiLineString);
            foreach (KeyValuePair<TKey, TValue> keyValue in dictionary)
            {
                result.AppendFormat("{0}{1}{2}{3}{4}",
                                    safeKeyToStringMethod(keyValue.Key),
                                    enumerablePrintFormat.KeyValueDelimiter,
                                    safeValueToStringMethod(keyValue.Value),
                                    enumerablePrintFormat.ElementDelimiter,
                                    multiLineString);
            }
            result.Append(enumerablePrintFormat.EndString);
            return result.ToString().Replace(enumerablePrintFormat.ElementDelimiter + multiLineString + enumerablePrintFormat.EndString, multiLineString + enumerablePrintFormat.EndString);
        }
    }
}
