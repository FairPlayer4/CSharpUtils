#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.Collections.ToString
{
    /// <summary>
    /// Useful class to print nested enumerables in a custom format.
    /// Supports the builder pattern so everything can be done in one line if necessary.
    /// Provides some convenience methods for adding Formats and custom ToString methods.
    /// Overrides the ToString Method which prints the enumerable using <see cref="ToReadableStringExtension"/>.
    /// </summary>
    [PublicAPI]
    public class CollectionPrinter
    {
        [NotNull]
        private readonly IEnumerable _enumerable;
        [NotNull]
        private readonly Dictionary<Type, ToStringDelegate<object>> _typeToStringMethod = new Dictionary<Type, ToStringDelegate<object>> ();
        [NotNull]
        private readonly Queue<EnumerablePrintFormat> _nestedEnumerablePrintFormatsQueue = new Queue<EnumerablePrintFormat>();
        [NotNull]
        private static ToStringDelegate<object> CreateSafeToStringMethod<T>([NotNull]ToStringDelegate<T> toStringMethod)
        {
            return obj =>
            {
                if (obj is T value) return toStringMethod(value);
                if (obj == null) return ToReadableStringExtension.NullString;
                string returnMessage = "Invalid Type provided for the ToString() Method!\nGiven Type: " + obj.GetType().Name + " | Required Type: " + typeof(T).Name;
                Logger.PrintWarning(returnMessage, nameof(CollectionPrinter));
                return returnMessage;
            };
        }
        [NotNull]
        public static CollectionPrinter Builder([NotNull]IEnumerable enumerable)
        {
            return new CollectionPrinter(enumerable);
        }

        public CollectionPrinter([NotNull]IEnumerable enumerable)
        {
            _enumerable = enumerable;
        }
        [NotNull]
        public CollectionPrinter AddTypeToStringMethod<T>([NotNull]Type type, [NotNull]ToStringDelegate<T> toStringMethod)
        {
            if (type != typeof(T) && !type.IsSubclassOf(typeof(T)))
            {
                Logger.PrintWarning(string.Format("The CollectionPrinter does not allow mapping incompatible types!\nThe key type must be equal or a subtype of the value type!\nKey Type: {0} | Value Type: {1}", type, typeof(T)));
            }
            _typeToStringMethod.AddIfNotContainsAndReplaceNull(type, CreateSafeToStringMethod(toStringMethod));
            return this;
        }
        [NotNull]
        public CollectionPrinter AddPrintFormat(EnumerablePrintFormat printFormat)
        {
            _nestedEnumerablePrintFormatsQueue.Enqueue(printFormat);
            return this;
        }
        [NotNull]
        public CollectionPrinter AddStandardPrintFormat()
        {
            _nestedEnumerablePrintFormatsQueue.Enqueue(new EnumerablePrintFormat());
            return this;
        }
        [NotNull]
        public CollectionPrinter AddMultiLinePrintFormat()
        {
            _nestedEnumerablePrintFormatsQueue.Enqueue(new EnumerablePrintFormat(newLinesEachElement:true));
            return this;
        }

        [NotNull]
        public override string ToString()
        {
            return _enumerable.ToReadableStringNested(_nestedEnumerablePrintFormatsQueue, _typeToStringMethod);
        }
    }
}
