#region Imports

using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.Collections.ToString
{
    /// <summary>
    /// This class is used to format strings of enumerables and dictionaries.
    /// <see cref="CollectionPrinter"/> and <see cref="ToReadableStringExtension"/>
    /// </summary>
    [PublicAPI]
    public sealed class EnumerablePrintFormat
    {

        [NotNull] public string StartString { get; }

        [NotNull] public string EndString { get; }

        /// <summary>
        /// The string that is inserted between elements of an enumerable.
        /// </summary>
        [NotNull] public string ElementDelimiter { get; }

        /// <summary>
        /// If the enumerable is a dictionary then this string is inserted between key and value.
        /// Otherwise this property is ignored.
        /// </summary>
        [NotNull] public string KeyValueDelimiter { get; }

        /// <summary>
        /// Defines if new lines are inserted after each element.
        /// They are inserted after the <see cref="ElementDelimiter"/>.
        /// The number of new lines that are inserted can be changed with <see cref="NumLinesEachElement"/>.
        /// Useful for dictionaries to display each key and its value on one line.
        /// </summary>
        public bool NewLinesEachElement { get; }

        /// <summary>
        /// If <see cref="NewLinesEachElement"/> is active then this property defines how many new lines are created after each element.
        /// Otherwise this property is ignored.
        /// </summary>
        public int NumLinesEachElement { get; }

        public EnumerablePrintFormat([NotNull]string startString = "[", [NotNull]string endString = "]", [NotNull]string elementDelimiter = ", ", [NotNull]string keyValueDelimiter = " = ", bool newLinesEachElement = false, int numLinesEachElement = 1)
        {
            StartString = startString;
            EndString = endString;
            ElementDelimiter = elementDelimiter;
            KeyValueDelimiter = keyValueDelimiter;
            NewLinesEachElement = newLinesEachElement;
            NumLinesEachElement = numLinesEachElement;
        }

        [NotNull]
        public static readonly EnumerablePrintFormat OnlyDelimiter = new EnumerablePrintFormat("", "");

        [NotNull]
        public static readonly EnumerablePrintFormat ItemEveryLineNoSymbols = new EnumerablePrintFormat("", "", "", " = ", true);

        [NotNull]
        public static EnumerablePrintFormat GetOnlyDelimiter([NotNull] string delimiter)
        {
            return new EnumerablePrintFormat("", "", delimiter, "", false, 0);
        }
    }
}
