#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Collections
{
    /// <summary>
    /// Keeps the order of keys that are inserted into the DictionaryList.
    /// Initially the keys are ordered according to the insertion order.
    /// However this can be changed by functionality provided by the IListUtil.
    /// The standard enumerator will enumerate the KeyValuePairs according to the order of the underlying list.
    /// The property IsInsertionOrder will indicate if the order is still the insertion order.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [PublicAPI]
    public sealed class DictionaryList<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionaryList<TKey, TValue>, IListUtil<TKey>
    {
        [NotNull]
        private readonly Dictionary<TKey, TValue> dictionary;

        [NotNull, ItemNotNull]
        private readonly SetList<TKey> keySetList;

        public DictionaryList()
        {
            dictionary = new Dictionary<TKey, TValue>();
            keySetList = new SetList<TKey>();
        }

        public DictionaryList([CanBeNull] IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(comparer);
            keySetList = new SetList<TKey>(comparer);
        }

        public DictionaryList(int capacity)
        {
            dictionary = new Dictionary<TKey, TValue>(capacity);
            keySetList = new SetList<TKey>(capacity);
        }

        public DictionaryList(int capacity, [CanBeNull] IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
            keySetList = new SetList<TKey>(capacity, comparer);
        }

        public DictionaryList([NotNull] IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary);
            keySetList = new SetList<TKey>(dictionary.Keys);
        }

        public DictionaryList([NotNull] IDictionary<TKey, TValue> dictionary, [CanBeNull] IEqualityComparer<TKey> comparer)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            keySetList = new SetList<TKey>(dictionary.Keys, comparer);
        }

        public bool IsInsertionOrder { get; private set; } = true;

        /// <summary>
        /// Returns the list of keys by the order of their insertion to the DictionaryList.
        /// </summary>
        public IReadOnlyList<TKey> KeyList => keySetList.AsReadOnly;

        public int Count => keySetList.Count;

        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values;

        [CanBeNull]
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set
            {
                dictionary[key] = value;
                keySetList.Add(key);
            }
        }

        [NotNull]
        TKey IReadOnlyList<TKey>.this[int index] => GetKeyAt(index);

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => GetAt(index);

        [NotNull]
        public TKey GetKeyAt(int index) => keySetList[index];

        [CanBeNull]
        public TValue GetValueAt(int index) => this[GetKeyAt(index)];

        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            TKey key = GetKeyAt(index);
            return new KeyValuePair<TKey, TValue>(key, this[key]);
        }

        public TValue GetValueOrDefault(TKey key, TValue defaultValue = default)
        {
            return TryGetValue(key, out TValue result) ? result : defaultValue;
        }

        public void Add(TKey key, [CanBeNull] TValue value)
        {
            dictionary.Add(key, value);
            keySetList.Add(key);
        }

        public bool Remove(TKey key)
        {
            bool removeResult = dictionary.Remove(key) && keySetList.Remove(key);
            if (Count <= 1) IsInsertionOrder = true;
            return removeResult;
        }

        public void Clear()
        {
            dictionary.Clear();
            keySetList.Clear();
            IsInsertionOrder = true;
        }

        public bool ContainsKey(TKey key) => keySetList.Contains(key);

        public bool ContainsValue(TValue value) => dictionary.ContainsValue(value);

        public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

        public DictionaryListEnumerator GetEnumerator() => new DictionaryListEnumerator(dictionary, keySetList);

        #region Implicit Implementations

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>)dictionary).Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>)dictionary).Values;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Add(item);
            keySetList.Add(item.Key);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            bool removeResult = ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(item) && keySetList.Remove(item.Key);
            if (Count <= 1) IsInsertionOrder = true;
            return removeResult;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)dictionary).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized => ((ICollection)dictionary).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

        bool IDictionary.IsFixedSize => ((IDictionary)dictionary).IsFixedSize;

        bool IDictionary.IsReadOnly => ((IDictionary)dictionary).IsReadOnly;

        ICollection IDictionary.Keys => ((IDictionary)dictionary).Keys;

        ICollection IDictionary.Values => ((IDictionary)dictionary).Values;

        [CanBeNull]
        object IDictionary.this[object key]
        {
            get => ((IDictionary)dictionary)[key];
            set
            {
                ((IDictionary)dictionary)[key] = value;
                ((IList)keySetList).Add(key);
            }
        }

        void IDictionary.Add(object key, [CanBeNull] object value)
        {
            ((IDictionary)dictionary).Add(key, value);
            ((IList)keySetList).Add(key);
        }

        bool IDictionary.Contains(object key) => ((IDictionary)dictionary).Contains(key);

        void IDictionary.Remove(object key)
        {
            ((IDictionary)dictionary).Remove(key);
            ((IList)keySetList).Remove(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => GetEnumerator();

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => keySetList.GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Special Enumerator

        public struct DictionaryListEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
                                                 IDictionaryEnumerator
        {
            [NotNull]
            private readonly Dictionary<TKey, TValue> dictionary;

            private Dictionary<TKey, TValue>.Enumerator dictionaryEnumerator;

            [NotNull]
            private readonly IEnumerator<TKey> listEnumerator;

            internal DictionaryListEnumerator([NotNull] Dictionary<TKey, TValue> dictionary, [NotNull] SetList<TKey> list) : this()
            {
                this.dictionary = dictionary;
                dictionaryEnumerator = dictionary.GetEnumerator();
                listEnumerator = list.GetEnumerator();
                Current = new KeyValuePair<TKey, TValue>();
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (!dictionaryEnumerator.MoveNext() || !listEnumerator.MoveNext() || listEnumerator.Current == null) return false;
                Current = new KeyValuePair<TKey, TValue>(listEnumerator.Current, dictionary[listEnumerator.Current]);
                return true;
            }

            void IEnumerator.Reset()
            {
                ((IEnumerator)dictionaryEnumerator).Reset();
                listEnumerator.Reset();
                Current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    DictionaryEntry entry = ((IDictionaryEnumerator)dictionaryEnumerator).Entry;
                    return new DictionaryEntry(Current.Key, Current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    object key = ((IDictionaryEnumerator)dictionaryEnumerator).Key;
                    return Current.Key;
                }
            }

            [CanBeNull]
            object IDictionaryEnumerator.Value
            {
                get
                {
                    object value = ((IDictionaryEnumerator)dictionaryEnumerator).Value;
                    return Current.Value;
                }
            }

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            [NotNull]
            object IEnumerator.Current
            {
                get
                {
                    if (((IEnumerator)dictionaryEnumerator).Current is DictionaryEntry) return new DictionaryEntry(Current.Key, Current.Value);
                    return new KeyValuePair<TKey, TValue>(Current.Key, Current.Value);
                }
            }
        }

        #endregion

        #region IListUtil Implementation

        public int BinarySearch(int index, int count, TKey item, IComparer<TKey> comparer) => keySetList.BinarySearch(index, count, item, comparer);

        public int BinarySearch(TKey item) => keySetList.BinarySearch(item);

        public int BinarySearch(TKey item, IComparer<TKey> comparer) => keySetList.BinarySearch(item, comparer);

        public List<TOutput> ConvertAll<TOutput>(Converter<TKey, TOutput> converter) => keySetList.ConvertAll(converter);

        public void CopyTo(TKey[] array)
        {
            keySetList.CopyTo(array);
        }

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            keySetList.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, TKey[] array, int arrayIndex, int count)
        {
            keySetList.CopyTo(index, array, arrayIndex, count);
        }

        public bool Exists(Predicate<TKey> match) => keySetList.Exists(match);

        public TKey Find(Predicate<TKey> match) => keySetList.Find(match);

        public List<TKey> FindAll(Predicate<TKey> match) => keySetList.FindAll(match);

        public int FindIndex(Predicate<TKey> match) => keySetList.FindIndex(match);

        public int FindIndex(int startIndex, Predicate<TKey> match) => keySetList.FindIndex(startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<TKey> match) => keySetList.FindIndex(startIndex, count, match);

        public TKey FindLast(Predicate<TKey> match) => keySetList.FindLast(match);

        public int FindLastIndex(Predicate<TKey> match) => keySetList.FindLastIndex(match);

        public int FindLastIndex(int startIndex, Predicate<TKey> match) => keySetList.FindLastIndex(startIndex, match);

        public int FindLastIndex(int startIndex, int count, Predicate<TKey> match) => keySetList.FindLastIndex(startIndex, count, match);

        public void ForEach(Action<TKey> action)
        {
            keySetList.ForEach(action);
        }

        public List<TKey> GetRange(int index, int count) => keySetList.GetRange(index, count);

        public int IndexOf(TKey item) => keySetList.IndexOf(item);

        public int IndexOf(TKey item, int index) => keySetList.IndexOf(item, index);

        public int IndexOf(TKey item, int index, int count) => keySetList.IndexOf(item, index, count);

        public int LastIndexOf(TKey item) => keySetList.LastIndexOf(item);

        public int LastIndexOf(TKey item, int index) => keySetList.LastIndexOf(item, index);

        public int LastIndexOf(TKey item, int index, int count) => keySetList.LastIndexOf(item, index, count);

        public void Reverse()
        {
            keySetList.Reverse();
            IsInsertionOrder = false;
        }

        public void Reverse(int index, int count)
        {
            keySetList.Reverse(index, count);
            IsInsertionOrder = false;
        }

        public void Sort()
        {
            keySetList.Sort();
            IsInsertionOrder = false;
        }

        public void Sort(IComparer<TKey> comparer)
        {
            keySetList.Sort(comparer);
            IsInsertionOrder = false;
        }

        public void Sort(int index, int count, IComparer<TKey> comparer)
        {
            keySetList.Sort(index, count, comparer);
            IsInsertionOrder = false;
        }

        public void Sort(Comparison<TKey> comparison)
        {
            keySetList.Sort(comparison);
            IsInsertionOrder = false;
        }

        public TKey[] ToArray() => keySetList.ToArray();

        public void TrimExcess()
        {
            keySetList.TrimExcess();
        }

        public bool TrueForAll(Predicate<TKey> match) => keySetList.TrueForAll(match);

        #endregion
    }
}
