#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CSharpUtilsNETStandard.Utils.Collections;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.Collections
{
    public static class CollectionUtilExtensions
    {
        [NotNull]
        public static List<T> ToSingleElementList<T>([NotNull]this T t)
        {
            return new List<T> { t };
        }

        [NotNull]
        public static SetList<T> ToSingleElementSetList<T>([NotNull]this T t)
        {
            return new SetList<T> { t };
        }

        [NotNull]
        public static T[] ToSingleElementArray<T>([NotNull]this T t)
        {
            return new[] { t };
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> ToEnumerable<T>([NotNull] this T t)
        {
            yield return t;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> ToEnumerable<T>([NotNull] this T t, [NotNull]T other)
        {
            yield return t;
            yield return other;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> ToEnumerable<T>([NotNull] this T t, [NotNull, ItemNotNull]params T[] others)
        {
            yield return t;
            foreach (T other in others) yield return other;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> Append<T>([NotNull, ItemNotNull] this IEnumerable<T> enumerable, [NotNull] T t)
        {
            return enumerable.Concat(t.ToEnumerable());
        }

        [NotNull]
        public static HashSet<T> ToHashSet<T>([NotNull]this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        [NotNull]
        public static HashSet<T> ToHashSet<T>([NotNull]this IEnumerable<T> enumerable, [NotNull]IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(enumerable, comparer);
        }

        [NotNull]
        public static SetList<T> ToSetList<T>([NotNull]this IEnumerable<T> enumerable)
        {
            return new SetList<T>(enumerable);
        }

        [NotNull]
        public static SetList<T> ToSetList<T>([NotNull]this IEnumerable<T> enumerable, [NotNull]IEqualityComparer<T> comparer)
        {
            return new SetList<T>(enumerable, comparer);
        }

        [NotNull]
        public static IEnumerable<TResult> SelectWhereNotNull<TSource, TResult>([NotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, TResult> selector) where TResult : class
        {
            return source.Select(selector).Where(result => result != null);
        }

        public static int FirstIndexOf<T>([NotNull, ItemCanBeNull] this IEnumerable<T> source, [CanBeNull]T item)
        {
            int index = 0;
            foreach (T t in source)
            {
                if (Equals(t, item)) return index;
                index++;
            }
            return -1;
        }

        public static bool ContainsBy<T, TComparedValue>([NotNull] this IEnumerable<T> source, [NotNull]T value, [NotNull] Func<T, TComparedValue> selector) where TComparedValue : IEquatable<TComparedValue>
        {
            return source.Select(selector).Contains(selector(value));
        }

        public static void RemoveNullValues<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary) where TValue : class
        {
            List<TKey> keysToRemove = dictionary.Where(x => x.Value == null).Select(x => x.Key).ToList();
            foreach (TKey key in keysToRemove)
            {
                dictionary.Remove(key);
            }
        }

        [NotNull]
        public static DictionaryList<TKey, TSource> ToDictionaryList<TSource, TKey>([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector, [CanBeNull]IEqualityComparer<TKey> comparer = null)
        {
            return ToDictionaryList(source, keySelector, x => x, comparer);
        }

        [NotNull]
        public static DictionaryList<TKey, TValue> ToDictionaryList<TSource, TKey, TValue>([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector, [NotNull]Func<TSource, TValue> elementSelector, [CanBeNull]IEqualityComparer<TKey> comparer = null)
        {
            DictionaryList<TKey, TValue> dictionary = new DictionaryList<TKey, TValue>(comparer);
            foreach (TSource element in source)
            {
                dictionary[keySelector(element)] = elementSelector(element);
            }
            return dictionary;
        }

        [NotNull]
        public static Dictionary<TKey, TSource> ToDictionaryIgnoreDuplicateKey<TSource, TKey>([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector, [CanBeNull]IEqualityComparer<TKey> comparer = null)
        {
            return ToDictionaryIgnoreDuplicateKey(source, keySelector, x => x, comparer);
        }

        [NotNull]
        public static Dictionary<TKey, TElement> ToDictionaryIgnoreDuplicateKey<TSource, TKey, TElement>([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector, [NotNull]Func<TSource, TElement> elementSelector, [CanBeNull]IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource element in source.Where(element => !dictionary.ContainsKey(keySelector(element))))
            {
                dictionary[keySelector(element)] = elementSelector(element);
            }
            return dictionary;
        }

        [NotNull]
        public static Dictionary<TKey, TSource> ToDictionaryOverrideOnDuplicateKey<TSource, TKey>([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector, [CanBeNull]IEqualityComparer<TKey> comparer = null)
        {
            return ToDictionaryOverrideOnDuplicateKey(source, keySelector, x => x, comparer);
        }

        [NotNull]
        public static Dictionary<TKey, TElement> ToDictionaryOverrideOnDuplicateKey<TSource, TKey, TElement>([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector, [NotNull]Func<TSource, TElement> elementSelector, [CanBeNull]IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource element in source) dictionary[keySelector(element)] = elementSelector(element);
            return dictionary;
        }

        public static IEnumerable<T> PrintWarningPredicate<T>([NotNull]this IEnumerable<T> enumerable, [NotNull]Predicate<T> predicate, [NotNull]Func<T, string> warningMessage)
        {
            foreach (T t in enumerable)
            {
                if (predicate(t)) t.PrintWarning(warningMessage(t));
                yield return t;
            }
        }

        //https://stackoverflow.com/a/13410938
        [NotNull]
        public static Dictionary<TValue, List<TKey>> ToValueKeyListDictionary<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary) where TKey : IEquatable<TKey> where TValue : IEquatable<TValue>
        {
            return dictionary.ToDictionaryWithListValues(x => x.Value, x => x.Key);
        }

        [NotNull]
        public static Dictionary<TKey, List<TListValue>> ToDictionaryWithListValues<T, TKey, TListValue>([NotNull]this IEnumerable<T> enumerable, [NotNull]Func<T, TKey> keyConverter, [NotNull]Func<T, TListValue> valueConverter)
        {
            Dictionary<TKey, List<TListValue>> dictionary = new Dictionary<TKey, List<TListValue>>();
            foreach (T item in enumerable)
            {
                TKey key = keyConverter(item);
                TListValue listValue = valueConverter(item);
                if (dictionary.TryGetValue(key, out List<TListValue> list)) list.Add(listValue);
                else dictionary[key] = listValue.ToSingleElementList();
            }
            return dictionary;
        }

        public static void AddToValueListOrCreateSingleValueList<TKey, TListValue>([NotNull]this IDictionary<TKey, List<TListValue>> dictionary, [NotNull]TKey key, [NotNull]TListValue value)
        {
            if (dictionary.TryGetValue(key, out List<TListValue> list)) list.Add(value);
            else dictionary[key] = new List<TListValue>{value};
        }

        [NotNull, ItemNotNull]
        public static List<List<T>> GroupBySimple<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<T, T, bool> belongsToSameGroupPredicate)
        {
            List<List<T>> result = new List<List<T>>();
            foreach (T item in source)
            {
                List<T> firstList = result.Find(list => belongsToSameGroupPredicate(list[0], item));
                if (firstList == null) result.Add(item.ToSingleElementList());
                else firstList.Add(item);
            }
            return result;
        }

        [NotNull]
        public static TRes[] ConvertAll<T, TRes>([NotNull] this T[] array, [NotNull]Converter<T, TRes> converter)
        {
            return Array.ConvertAll(array, converter);
        }

        public static void AddRangeOverwrite<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] IReadOnlyDictionary<TKey, TValue> otherDictionary)
        {
            foreach (var keyValuePair in otherDictionary) dictionary[keyValuePair.Key] = keyValuePair.Value;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<TValue> GetExistingValuesFromKeys<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull, ItemNotNull] IEnumerable<TKey> keys)
        {
            foreach (TKey key in keys)
            {
                if (dictionary.TryGetValue(key, out TValue value)) yield return value;
            }
        }

        public static bool HasNullOrWhiteSpaceKeysOrValues([NotNull]this IEnumerable enumerable)
        {
            if (enumerable is IDictionary dictionary) return dictionary.HasNullOrWhiteSpaceKeysOrValues();
            foreach (object o in enumerable)
            {
                if (o == null) return true;
                if (o is IEnumerable innerEnumerable && innerEnumerable.HasNullOrWhiteSpaceKeysOrValues()) return true;
            }
            return false;
        }

        public static bool HasNullOrWhiteSpaceKeysOrValues([NotNull]this IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.IsNullOrWhiteSpace()) return true;
                if (entry.Value is IEnumerable enumerable && enumerable.HasNullOrWhiteSpaceKeysOrValues()) return true;
            }
            return false;
        }

        private static bool IsNullOrWhiteSpace(this DictionaryEntry entry)
        {
            return entry.Key is string && string.IsNullOrWhiteSpace(entry.Key.ToString()) || entry.Value == null || entry.Value is string && string.IsNullOrWhiteSpace(entry.Value.ToString());
        }

        /// <summary>
        /// DOES NOT WORK FOR NESTED GENERIC ICollection THAT DOES NOT IMPLEMENT UNGENERIC IList or IDictionary.
        /// => DOES NOT WORK FOR NESTED HashSet or other implementations of ISet
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static bool RemoveNullOrWhiteSpaceKeysAndValues([NotNull]this ICollection collection)
        {
            if (collection.Count == 0) return false;
            bool anyNullValues = false;
            if (collection is IDictionary dictionary)
            {
                List<object> keysToRemoveList = new List<object>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.IsNullOrWhiteSpace()) keysToRemoveList.Add(entry.Key);
                    else if (entry.Value is ICollection value) anyNullValues |= value.RemoveNullOrWhiteSpaceKeysAndValues();
                }
                if (keysToRemoveList.Count == 0) return anyNullValues;
                // ReSharper disable once AssignNullToNotNullAttribute
                keysToRemoveList.ForEach(key => dictionary.Remove(key));
                return true;
            }
            IList list = collection as IList;
            int numberOfNullEntries = 0;
            if (list != null)
            {
                foreach (object o in list)
                {
                    if (o == null || o is string && string.IsNullOrWhiteSpace(o.ToString())) numberOfNullEntries++;
                    else if (o is ICollection castCollection) anyNullValues |= castCollection.RemoveNullOrWhiteSpaceKeysAndValues();
                }

                if (numberOfNullEntries == 0) return anyNullValues;
                for (int i = 0; i < numberOfNullEntries; i++) list.Remove(null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// DOES NOT WORK FOR NESTED GENERIC ICollection THAT DOES NOT IMPLEMENT UNGENERIC IList or IDictionary.
        /// => DOES NOT WORK FOR NESTED HashSet or other implementations of ISet
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool RemoveNullOrWhiteSpaceKeysAndValues<T>([NotNull]this ICollection<T> collection) where T : class
        {
            if (collection.Count == 0) return false;
            bool anyNullValues = false;
            bool valueIsCollection = typeof(ICollection).IsAssignableFrom(typeof(T));
            int numberOfNullEntries = 0;
            foreach (object o in collection)
            {
                if (o == null || o is string && string.IsNullOrWhiteSpace(o.ToString())) numberOfNullEntries++;
                else if (valueIsCollection) anyNullValues |= ((ICollection)o).RemoveNullOrWhiteSpaceKeysAndValues();
            }
            if (numberOfNullEntries == 0) return anyNullValues;
            for (int i = 0; i < numberOfNullEntries; i++) collection.Remove(null);
            return true;
        }

        /// <summary>
        /// DOES NOT WORK FOR NESTED GENERIC ICollection THAT DOES NOT IMPLEMENT UNGENERIC IList or IDictionary.
        /// => DOES NOT WORK FOR NESTED HashSet or other implementations of ISet
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static bool RemoveNullOrWhiteSpaceKeysAndValues<TKey, TValue>([NotNull]this IDictionary<TKey, TValue> dictionary) where TValue : class
        {
            if (dictionary.Count == 0) return false;
            bool anyNullValues = false;
            bool valueIsCollection = typeof(ICollection).IsAssignableFrom(typeof(TValue));
            List<TKey> keysToRemoveList = new List<TKey>();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
            {
                if (keyValuePair.Key is string && string.IsNullOrWhiteSpace(keyValuePair.Key.ToString())) keysToRemoveList.Add(keyValuePair.Key);
                else if (keyValuePair.Value == null || keyValuePair.Value is string && string.IsNullOrWhiteSpace(keyValuePair.Value.ToString())) keysToRemoveList.Add(keyValuePair.Key);
                else if (valueIsCollection) anyNullValues |= ((ICollection)keyValuePair.Value).RemoveNullOrWhiteSpaceKeysAndValues();
            }
            if (keysToRemoveList.Count == 0) return anyNullValues;
            // ReSharper disable once AssignNullToNotNullAttribute
            keysToRemoveList.ForEach(key => dictionary.Remove(key));
            return true;
        }

        [NotNull, SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static IEnumerable<TKey> GetMissingKeysAndRemoveInvalidKeys<TKey, TValue>([NotNull]this IDictionary<TKey, TValue> dictionary, [NotNull]IDictionary<TKey, TValue> validDictionary)
        {
            List<TKey> keysRemove = new List<TKey>(dictionary.Keys.Where(key => !validDictionary.ContainsKey(key)));
            keysRemove.ForEach(key => dictionary.Remove(key));
            return validDictionary.Keys.Where(key => !dictionary.ContainsKey(key));
        }

        public static void AddRange<T>([NotNull, ItemNotNull]this ISet<T> set, [NotNull, ItemNotNull] IEnumerable<T> values)
        {
            set.UnionWith(values);
        }

        public static bool ContainsAll<T>([NotNull, ItemNotNull]this ISet<T> set, [NotNull, ItemNotNull] IEnumerable<T> values)
        {
            return set.IsSupersetOf(values);
        }

        public static bool ContainsAll<T>([NotNull, ItemNotNull]this ISet<T> set, [NotNull, ItemNotNull] params T[] values)
        {
            return set.IsSupersetOf(values);
        }

        private static bool ContainsKeyAndRemoveKeyIfValueNull<TKey, TValue>([NotNull]this IDictionary<TKey, TValue> dictionary, [NotNull]TKey key)
        {
            if (!dictionary.TryGetValue(key, out TValue value)) return false;
            if (value != null) return true;
            dictionary.Remove(key);
            return false;
        }

        public static bool AddIfNotContains<T>([NotNull]this IList<T> list, [NotNull]T value)
        {
            if (list.Contains(value)) return false;
            list.Add(value);
            return true;
        }

        public static void AddIfNotContains<T>([NotNull]this IList<T> list, [NotNull]IEnumerable<T> values)
        {
            foreach (T value in values) list.AddIfNotContains(value);
        }

        public static bool AddIfNotContainsAndReplaceNull<TKey, TValue>([NotNull]this IDictionary<TKey, TValue> dictionary, [NotNull]TKey key, [NotNull]TValue value)
        {
            if (dictionary.ContainsKeyAndRemoveKeyIfValueNull(key)) return false;
            dictionary[key] = value;
            return true;
        }

        public static bool IsEmpty<T>([NotNull]this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T> collection) return collection.Count == 0;
            return !enumerable.Any();
        }

        public static bool IsNullOrEmpty<T>([CanBeNull]this IEnumerable<T> enumerable)
        {
            return enumerable == null || enumerable.IsEmpty();
        }
        [NotNull, ItemCanBeNull]
        public static IEnumerable<T> ToEnumerable<T>([NotNull]this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> ItemNotNull<T>([NotNull, ItemCanBeNull]this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => x != null);
        }

        /// <summary>
        /// Like the indexer but returns null instead of throwing an exception.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="failureLogLevel"></param>
        /// <returns></returns>
        [ContractAnnotation("defaultValue:null => canbenull; defaultValue:notnull => notnull")] //This will only hold if the dictionary does not contain null values
        public static TValue GetValueOrDefault<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> dictionary, [NotNull] TKey key, [CanBeNull]TValue defaultValue = default, [CanBeNull] LogLevel? failureLogLevel = null)
        {
            if (dictionary.TryGetValue(key, out TValue value)) return value;
            PrintLogOnFailure<TKey, TValue>(key, failureLogLevel);
            return defaultValue;
        }

        [NotNull]
        public static string GetValueOrEmpty<TKey>([NotNull] this IReadOnlyDictionary<TKey, string> dictionary, [NotNull] TKey key, [NotNull] string defaultValue = "", [CanBeNull] LogLevel? failureLogLevel = null)
        {
            return GetValueOrDefault(dictionary, key, defaultValue, failureLogLevel);
        }

        [CanBeNull]
        public static TValue GetValueOrNull<TKey, TValue>([NotNull] this IReadOnlyDictionary<TKey, TValue> dictionary, [NotNull]TKey key, [CanBeNull] LogLevel? failureLogLevel = null) where TValue : class
        {
            return GetValueOrDefault(dictionary, key, null, failureLogLevel);
        }

        private static void PrintLogOnFailure<TKey, TValue>([NotNull] TKey key, [CanBeNull] LogLevel? failureLogLevel)
        {
            if (failureLogLevel.HasValue) Logger.PrintLogLevel(failureLogLevel.Value, string.Format("The key {0} was not found in a Dictionary of <{1}, {2}>! This warning message is for debugging purposes and could be normal behaviour.", key.ToString(), typeof(TKey).Name, typeof(TValue).Name), typeof(CollectionUtilExtensions).Name);
        }

        public static void ForEachValue<TKey, TValue>([NotNull]this IReadOnlyDictionary<TKey, TValue> dictionary, [NotNull]Action<TValue> action)
        {
            foreach (TValue value in dictionary.Values)
            {
                action(value);
            }
        }

        public static void ForEach<T>([NotNull]this IEnumerable<T> enumerable, [NotNull]Action<T> action)
        {
            foreach (T e in enumerable)
            {
                action(e);
            }
        }
        [NotNull]
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            ([NotNull]this IEnumerable<TSource> source, [NotNull]Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        [NotNull]
        public static string ToSeparatedString<T>([NotNull, ItemNotNull] this IEnumerable<T> enumerable, [NotNull] string separator)
        {
            return string.Join(separator, enumerable);
        }

        [NotNull]
        public static string ToCommaSeparatedString<T>([NotNull, ItemNotNull] this IEnumerable<T> enumerable)
        {
            return string.Join(",", enumerable);
        }
    }
}
