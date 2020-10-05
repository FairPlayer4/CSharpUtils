#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpUtilsNETStandard.Utils.Collections;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.Collections
{
    [PublicAPI]
    public static class ContentEqualsExtension
    {
        #region Extra Stuff

        public static bool ContentEqualsIgnoreOrder<T>([CanBeNull, ItemCanBeNull] this IEnumerable<T> enumerable, [CanBeNull, ItemCanBeNull] IEnumerable<T> enumerableOther)
            where T : IEquatable<T>
        {
            return ContentEquals(enumerable, enumerableOther, ignoreOrder: true);
        }

        public static bool ContentEqualsIgnoreOrder<T>([CanBeNull, ItemCanBeNull] this ICollection<T> collection, [CanBeNull, ItemCanBeNull] ICollection<T> collectionOther)
            where T : IEquatable<T>
        {
            return ContentEquals(collection, collectionOther, ignoreOrder: true);
        }

        public static bool SetEquals<T>([CanBeNull, ItemCanBeNull] this IEnumerable<T> enumerable, [CanBeNull, ItemCanBeNull] IEnumerable<T> enumerableOther)
            where T : IEquatable<T>
        {
            return ContentEquals(enumerable, enumerableOther, true, true);
        }

        public static bool SetEquals<T>([CanBeNull, ItemCanBeNull] this ICollection<T> collection, [CanBeNull, ItemCanBeNull] ICollection<T> collectionOther)
            where T : IEquatable<T>
        {
            return ContentEquals(collection, collectionOther, true, true);
        }

        public static bool EqualsAny<T>([NotNull] this T value, [NotNull, ItemCanBeNull] params T[] others)
        {
            return others.Length > 0 && others.Any(other => value.Equals(other));
        }

        #endregion

        /// <summary>
        /// Works for IEnumerables that contain elements that implement IEquatable.
        /// Use this method if possible because it is faster.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="enumerableOther"></param>
        /// <param name="ignoreDuplicates"></param>
        /// <param name="ignoreOrder"></param>
        /// <returns></returns>
        public static bool ContentEquals<T>([CanBeNull, ItemCanBeNull] this IEnumerable<T> enumerable, [CanBeNull, ItemCanBeNull] IEnumerable<T> enumerableOther, bool ignoreDuplicates = false, bool ignoreOrder = false)
            where T : IEquatable<T>
        {
            // First use the normal Equals method => returns true if both object are null, have the same reference or if the Equals method is overridden and both objects are considered equal.
            if (Equals(enumerable, enumerableOther)) return true;
            // If any enumerable is null return false (if they are both null the method will have already returned true)
            if (enumerable == null || enumerableOther == null) return false;
            return enumerable.ContentEqualsFullCheck(enumerableOther, ignoreDuplicates, ignoreOrder);
        }

        public static bool ContentEquals<T>([CanBeNull, ItemCanBeNull] this ICollection<T> collection, [CanBeNull, ItemCanBeNull] ICollection<T> collectionOther, bool ignoreDuplicates = false, bool ignoreOrder = false)
            where T : IEquatable<T>
        {
            // First use the normal Equals method => returns true if both object are null, have the same reference or if the Equals method is overridden and both objects are considered equal.
            if (Equals(collection, collectionOther)) return true;
            // If any enumerable is null return false (if they are both null the method will have already returned true)
            if (collection == null || collectionOther == null) return false;
            if (collection is ISet<T> || collectionOther is ISet<T>) ignoreOrder = true;
            int count1 = collection.Count;
            int count2 = collectionOther.Count;
            if (!ignoreDuplicates && count1 != count2) return false;
            if (count1 == 0 && count2 == 0) return true;
            if (count1 != 1 || count2 != 1) return collection.ContentEqualsFullCheck(collectionOther, ignoreDuplicates, ignoreOrder);
            return collection.FirstOrDefault().CheckNullThenEquals(collectionOther.FirstOrDefault());
        }

        private static bool ContentEqualsFullCheck<T>([NotNull, ItemCanBeNull] this IEnumerable<T> enumerable, [NotNull, ItemCanBeNull] IEnumerable<T> enumerableOther, bool ignoreDuplicates, bool ignoreOrder)
            where T : IEquatable<T>
        {
            if (ignoreDuplicates)
            {
                if (ignoreOrder)
                {
                    ISet<T> set = enumerable as ISet<T> ?? new HashSet<T>(enumerable);
                    return set.SetEquals(enumerableOther);
                }
                else
                {
                    IList<T> set = enumerable as SetList<T> ?? new SetList<T>(enumerable);
                    IList<T> setOther = enumerableOther as SetList<T> ?? new SetList<T>(enumerableOther);
                    int setSize = set.Count;
                    if (setSize != setOther.Count) return false;
                    for (int i = 0; i < setSize; i++)
                        if (!set[i].CheckNullThenEquals(setOther[i]))
                            return false;
                }
            }
            else
            {
                IList<T> list;
                IList<T> listOther;
                if (ignoreOrder)
                {
                    list = enumerable.OrderBy(x => x).ToList();
                    listOther = enumerableOther.OrderBy(x => x).ToList();
                }
                else
                {
                    list = enumerable as IList<T> ?? new List<T>(enumerable);
                    listOther = enumerableOther as IList<T> ?? new List<T>(enumerableOther);
                }
                int listSize = list.Count;
                if (listSize != listOther.Count) return false;
                for (int i = 0; i < listSize; i++)
                    if (!list[i].CheckNullThenEquals(listOther[i]))
                        return false;
            }
            return true;
        }

        public static bool ContentEquals<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> dictionary, [CanBeNull] IDictionary<TKey, TValue> dictionaryOther)
            where TKey : IEquatable<TKey>
            where TValue : IEquatable<TValue>
        {
            // First use the normal Equals method => returns true if both object are null, have the same reference or if the Equals method is overridden and both objects are considered equal.
            if (Equals(dictionary, dictionaryOther)) return true;
            // If any enumerable is null return false (if they are both null the method will have already returned true)
            if (dictionary == null || dictionaryOther == null) return false;
            if (dictionary.Count != dictionaryOther.Count) return false;
            foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
            {
                if (!dictionaryOther.TryGetValue(keyValuePair.Key, out TValue value)) return false;
                if (!keyValuePair.Value.CheckNullThenEquals(value)) return false;
            }
            return true;
        }

        /// <summary>
        /// Makes sure that the IEquatable Equals is used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="valueOther"></param>
        /// <returns></returns>
        public static bool CheckNullThenEquals<T>([CanBeNull] this T value, [CanBeNull] T valueOther)
            where T : IEquatable<T>
        {
            if (ReferenceEquals(value, valueOther)) return true;
            if (value == null || valueOther == null) return false;
            return value.Equals(valueOther);
        }

        public static bool EqualsConsiderEnumerable(
            [CanBeNull] this object obj, [CanBeNull] object objOther, bool ignoreDuplicates = false, bool ignoreOrder = false, bool ignoreDuplicatesOnInnerEnumerables = false,
            bool ignoreOrderOnInnerEnumerables = false
        )
        {
            if (Equals(obj, objOther)) return true;
            if (obj == null || objOther == null) return false;
            if (!(obj is IEnumerable enumerable) || !(objOther is IEnumerable enumerableOther)) return false;
            return NestedContentEquals(enumerable,
                                       enumerableOther,
                                       ignoreDuplicates,
                                       ignoreOrder,
                                       ignoreDuplicatesOnInnerEnumerables,
                                       ignoreOrderOnInnerEnumerables);
        }

        /// <summary>
        /// Checks if the content of two enumerables is equal.
        /// Not efficient but works with complex nested enumerables.
        /// Technically it should work with anything that implements IEnumerable.
        /// The enumerables are cast to IDictionary if they implement it otherwise they are compared as IEnumerable.
        /// The keys of dictionaries are compared with the regular Equals method.
        /// Duplicates and element order can be ignored if the parameters are set accordingly.
        /// </summary>
        /// <param name="enumerable">An IEnumerable.</param>
        /// <param name="enumerableOther">Another IEnumerable.</param>
        /// <param name="ignoreDuplicates">If true then the enumerables are compared ignoring element order and removing duplicate elements before comparison.</param>
        /// <param name="ignoreOrder">If true then the enumerables are compared ignoring element order.</param>
        /// <param name="ignoreDuplicatesOnInnerEnumerables">If true then inner (nested) enumerables are compared ignoring element order and removing duplicate elements before comparison.</param>
        /// <param name="ignoreOrderOnInnerEnumerables">If true then inner (nested) enumerables are compared ignoring element order.</param>
        /// <returns></returns>
        public static bool NestedContentEquals(
            [CanBeNull, ItemCanBeNull] this IEnumerable enumerable, [CanBeNull, ItemCanBeNull] IEnumerable enumerableOther, bool ignoreDuplicates = false, bool ignoreOrder = false, bool ignoreDuplicatesOnInnerEnumerables = false,
            bool ignoreOrderOnInnerEnumerables = false
        )
        {
            // First use the normal Equals method => returns true if both object are null, have the same reference or if the Equals method is overridden and both objects are considered equal.
            if (Equals(enumerable, enumerableOther)) return true;
            // If any enumerable is null return false (if they are both null the method will have already returned true)
            if (enumerable == null || enumerableOther == null) return false;
            // Special case for string for better performance
            if (!ignoreDuplicates && !ignoreOrder)
            {
                bool? result = PerformBasicEqualsChecks(enumerable, enumerableOther);
                if (result.HasValue) return result.Value;
            }
            // If both enumerables are directories then threat them as such
            if (enumerable is IDictionary dictionary && enumerableOther is IDictionary dictionaryOther)
            {
                return dictionary.NestedContentEquals(dictionaryOther,
                                                      ignoreDuplicatesOnInnerEnumerables,
                                                      ignoreOrderOnInnerEnumerables);
            }
            // If duplicates are ignored then the order must also be ignored because otherwise it would matter in what way duplicates are removed.
            if (ignoreDuplicates) ignoreOrder = true;
            if (ignoreDuplicatesOnInnerEnumerables) ignoreOrderOnInnerEnumerables = true;
            List<object> list = enumerable.Cast<object>().ToList();
            List<object> listOther = enumerableOther.Cast<object>().ToList();
            if (ignoreDuplicates)
            {
                IEnumerable<object> duplicateList = list.GetAllDuplicates(ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables);
                foreach (object duplicate in duplicateList)
                {
                    list.Remove(duplicate);
                }
                IEnumerable<object> duplicateListOther = listOther.GetAllDuplicates(ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables);
                foreach (object duplicate in duplicateListOther)
                {
                    listOther.Remove(duplicate);
                }
            }
            if (list.Count != listOther.Count) return false;
            List<int> indexList = new List<int>();
            if (ignoreOrder)
            {
                foreach (object element in list)
                {
                    for (int i = 0; i < listOther.Count; i++)
                    {
                        if (indexList.Contains(i)) continue;
                        if (element is IEnumerable elementEnumerable && listOther[i] is IEnumerable elementEnumerableOther)
                        {
                            if (!elementEnumerableOther.NestedContentEquals(elementEnumerable, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables)) continue;
                        }
                        else
                        {
                            if (!Equals(listOther[i], element)) continue;
                        }
                        indexList.Add(i);
                        break;
                    }
                }
                return indexList.Count == listOther.Count;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is IEnumerable e1 && listOther[i] is IEnumerable e2)
                {
                    if (!e1.NestedContentEquals(e2, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables)) return false;
                }
                else
                {
                    if (!Equals(list[i], listOther[i])) return false;
                }
            }
            return true;
        }

        private static bool? PerformBasicEqualsChecks([NotNull, NoEnumeration] this IEnumerable enumerable, [NotNull, NoEnumeration] IEnumerable enumerableOther)
        {
            if (enumerable is ICollection collection && enumerableOther is ICollection collectionOther && collection.Count != collectionOther.Count) return false;
            if (enumerable is string firstString && enumerableOther is string secondString) return firstString == secondString;
            if (enumerable is IEnumerable<string> firstStringEnumerable && enumerableOther is IEnumerable<string> secondStringEnumerable) return firstStringEnumerable.ContentEquals(secondStringEnumerable);
            if (enumerable is IEnumerable<int> firstIntEnumerable && enumerableOther is IEnumerable<int> secondIntEnumerable) return firstIntEnumerable.ContentEquals(secondIntEnumerable);
            return null;
        }

        public static bool NestedContentEquals(
            [CanBeNull] this IDictionary dictionary, [CanBeNull] IDictionary dictionaryOther,
            bool ignoreDuplicatesOnInnerEnumerables = false,
            bool ignoreOrderOnInnerEnumerables = false
        )
        {
            // First use the normal Equals method => returns true if both object are null, have the same reference or if the Equals method is overridden and both objects are considered equal.
            if (Equals(dictionary, dictionaryOther)) return true;
            // If any dictionary is null return false (if they are both null the method will have already returned true)
            if (dictionary == null || dictionaryOther == null) return false;
            // If duplicates are ignored then the order must also be ignored because otherwise it would matter in what way duplicates are removed.
            if (ignoreDuplicatesOnInnerEnumerables) ignoreOrderOnInnerEnumerables = true;
            if (!dictionary.Keys.NestedContentEquals(dictionaryOther.Keys, false, true)) return false;
            foreach (DictionaryEntry keyValue in dictionary)
            {
                //TODO null check is unnecessary because dictionary.Keys.ContentEquals(dictionaryOther.Keys, false, true)
                object keyOther = dictionaryOther.Keys.Cast<object>().FirstOrDefault(k => Equals(k, keyValue.Key));
                if (keyOther == null) return false;
                if (keyValue.Value is IEnumerable valueEnumerable)
                {
                    if (!valueEnumerable.NestedContentEquals(dictionaryOther[keyOther] as IEnumerable, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables)) return false;
                }
                else
                {
                    if (!Equals(keyValue.Value, dictionaryOther[keyOther])) return false;
                }
            }
            return true;
        }

        public static bool HasDuplicates<T>([CanBeNull, ItemCanBeNull] this IEnumerable<T> enumerable)
            where T : IEquatable<T>
        {
            if (enumerable == null) return false;
            HashSet<T> set = new HashSet<T>();
            return enumerable.Any(value => !set.Add(value));
        }

        /// <summary>
        /// Checks if an enumerable contains duplicates.
        /// Not efficient but works with complex nested enumerables.
        /// </summary>
        /// <param name="enumerable">An IEnumerable.</param>
        /// <param name="ignoreDuplicatesOnInnerEnumerables">If true then inner (nested) enumerables are compared ignoring element order and removing duplicate elements before comparison.</param>
        /// <param name="ignoreOrderOnInnerEnumerables">If true then inner (nested) enumerables are compared ignoring order.</param>
        /// <returns></returns>
        public static bool HasDuplicatesNested([CanBeNull, ItemCanBeNull] this IEnumerable enumerable, bool ignoreDuplicatesOnInnerEnumerables = false, bool ignoreOrderOnInnerEnumerables = false)
        {
            if (enumerable == null) return false;
            if (ignoreDuplicatesOnInnerEnumerables) ignoreOrderOnInnerEnumerables = true;
            List<object> list = enumerable.Cast<object>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i] is IEnumerable e1 && list[j] is IEnumerable e2)
                    {
                        if (e1.NestedContentEquals(e2, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables)) return true;
                    }
                    else
                    {
                        if (Equals(list[i], list[j])) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Helper Method for ContentEquals.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="ignoreDuplicatesOnInnerEnumerables"></param>
        /// <param name="ignoreOrderOnInnerEnumerables"></param>
        /// <returns></returns>
        [NotNull, ItemCanBeNull]
        private static IEnumerable<object> GetAllDuplicates([CanBeNull, ItemCanBeNull] this IEnumerable enumerable, bool ignoreDuplicatesOnInnerEnumerables = false, bool ignoreOrderOnInnerEnumerables = false)
        {
            if (ignoreDuplicatesOnInnerEnumerables) ignoreOrderOnInnerEnumerables = true;
            List<object> duplicateList = new List<object>();
            if (enumerable == null) return duplicateList;
            List<int> markedAsDuplicate = new List<int>();
            List<object> list = enumerable.Cast<object>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (markedAsDuplicate.Contains(i) || markedAsDuplicate.Contains(j)) continue;
                    if (list[i] is IEnumerable e1 && list[j] is IEnumerable e2)
                    {
                        if (!e1.NestedContentEquals(e2, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables, ignoreDuplicatesOnInnerEnumerables, ignoreOrderOnInnerEnumerables)) continue;
                        duplicateList.Add(e2);
                        markedAsDuplicate.Add(j);
                    }
                    else
                    {
                        if (!Equals(list[i], list[j])) continue;
                        duplicateList.Add(list[j]);
                        markedAsDuplicate.Add(j);
                    }
                }
            }
            return duplicateList;
        }
    }
}
