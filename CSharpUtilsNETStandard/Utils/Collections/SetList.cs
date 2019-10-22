#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CSharpUtilsNETStandard.Utils.Extensions.Collections;
using CSharpUtilsNETStandard.Utils.Extensions.Collections.ToString;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Collections
{
    /// <summary>
    /// A list that is also a set.
    /// Uses twice the space compared to a normal set.
    /// Provides the functionality of the standard List and HashSet implementation.
    /// Keeps the order and can be indexed.
    /// The standard enumerator will enumerate according to the order of the list.
    /// Will not allow adding duplicates similar to HashSet.
    /// The non pure ISet methods will perform much slower than the those from the standard HashSet because the List needs to be modified.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SetList<T> : IList<T>, IList, ISet<T>, IAddOnlySet<T>, IStackSet<T>, IReadOnlyList<T>, IListUtil<T>, IEquatable<SetList<T>>
    {
        [NotNull]
        private readonly HashSet<T> set;

        [NotNull]
        private readonly List<T> list;

        public SetList()
        {
            set = new HashSet<T>();
            list = new List<T>();
        }

        public SetList([CanBeNull] IEqualityComparer<T> comparer)
        {
            set = new HashSet<T>(comparer);
            list = new List<T>();
        }

        public SetList(int capacity)
        {
            set = new HashSet<T>();
            list = new List<T>(capacity);
        }

        public SetList(int capacity, [CanBeNull] IEqualityComparer<T> comparer)
        {
            set = new HashSet<T>(comparer);
            list = new List<T>(capacity);
        }

        public SetList([NotNull] IEnumerable<T> collection)
        {
            list = new List<T>(collection);
            set = new HashSet<T>();
            FillSetFromListAndRemoveDuplicates();
        }

        public SetList([NotNull] IEnumerable<T> collection, [CanBeNull] IEqualityComparer<T> comparer)
        {
            list = new List<T>(collection);
            set = new HashSet<T>(comparer);
            FillSetFromListAndRemoveDuplicates();
        }

        private void FillSetFromListAndRemoveDuplicates()
        {
            for (int i = 0; i < list.Count; i++)
                if (!set.Add(list[i]))
                    list.RemoveAt(i--);
        }

        public int Capacity
        {
            get => list.Capacity;
            set => list.Capacity = value;
        }

        public IEqualityComparer<T> Comparer => set.Comparer;

        public int Count => list.Count;

        public T this[int index]
        {
            get => list[index];
            set
            {
                if (index >= 0 && index < Count)
                {
                    if (!set.Add(value)) return;
                    set.Remove(list[index]);
                    list[index] = value;
                }
                else // Keep default behavior for indexes that are out of bounds
                {
                    list[index] = value;
                }
            }
        }

        private void MakeListEqualToSet()
        {
            list.RemoveAll(item => !set.Contains(item));

            foreach (T item in set)
                if (!list.Contains(item))
                    list.Add(item);
        }

        private void MakeSetEqualToList()
        {
            set.Clear();
            set.UnionWith(list);
        }

        public bool Add(T item)
        {
            if (!set.Add(item)) return false;
            list.Add(item);
            return true;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            list.AddRange(collection.Where(item => set.Add(item)));
        }

        [NotNull]
        public ReadOnlyCollection<T> AsReadOnly => list.AsReadOnly();

        public void Clear()
        {
            set.Clear();
            list.Clear();
        }

        public bool Contains(T item) => set.Contains(item);

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count) list.Insert(index, item);
            else if (set.Add(item)) list.Insert(index, item);
        }

        public void InsertOrMove(int index, T item)
        {
            if (index < 0 || index > Count) list.Insert(index, item);
            else
            {
                if (set.Add(item)) list.Insert(index, item);
                else
                {
                    Remove(item);
                    Insert(index, item);
                }
            }
        }

        [NotNull]
        public IEnumerable<T> InsertRange(int index, [NotNull] IEnumerable<T> collection)
        {
            List<T> notAdded = collection.ToList();
            List<T> toAdd = new List<T>();
            if (index < 0 || index > Count)
            {
                list.InsertRange(index, notAdded);
                return toAdd;
            }
            for (int i = 0; i < notAdded.Count; i++)
            {
                if (!set.Add(notAdded[i])) continue;
                toAdd.Add(notAdded[i]);
                notAdded.RemoveAt(i--);
            }
            list.InsertRange(index, toAdd);
            return notAdded;
        }

        public bool Remove(T t)
        {
            if (!set.Remove(t)) return false;
            list.Remove(t);
            return true;
        }

        public int RemoveAll([NotNull] Predicate<T> match)
        {
            int removeAllFromList = list.RemoveAll(match);
            int removeAllFromSet = set.RemoveWhere(match);
            if (removeAllFromList == removeAllFromSet) return removeAllFromList;
            MakeListEqualToSet();
            return removeAllFromSet;
        }

        public int RemoveWhere([NotNull] Predicate<T> match) => RemoveAll(match);

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < Count) set.Remove(list[index]);
            list.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            //Removal cannot cause duplicates!
            list.RemoveRange(index, count);
            MakeSetEqualToList();
        }

        #region Implicit Implementations

        bool ICollection<T>.IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        void ICollection<T>.Add(T item)
        {
            if (item != null) Add(item);
        }

        bool ICollection.IsSynchronized => ((IList)list).IsSynchronized;

        object ICollection.SyncRoot => ((IList)list).SyncRoot;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }

        bool IList.IsFixedSize => ((IList)list).IsFixedSize;

        bool IList.IsReadOnly => ((IList)list).IsReadOnly;

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        int IList.Add(object value)
        {
            int index = ((IList)list).Add(value);
            if (set.Add((T)value)) return index;
            list.RemoveAt(index);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            ((IList)list).Insert(index, value);
            if (set.Add((T)value)) return;
            list.RemoveAt(index);
        }

        int IList.IndexOf(object value) => ((IList)list).IndexOf(value);

        bool IList.Contains(object value) => ((IList)list).Contains(value);

        void IList.Remove(object value)
        {
            ((IList)list).Remove(value);
            if (value is T tValue) set.Remove(tValue);
        }

        #endregion

        #region IListUtil Implementation

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) => list.BinarySearch(index, count, item, comparer);

        public int BinarySearch(T item) => list.BinarySearch(item);

        public int BinarySearch(T item, IComparer<T> comparer) => list.BinarySearch(item, comparer);

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => list.ConvertAll(converter);

        public void CopyTo(T[] array)
        {
            list.CopyTo(array);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            list.CopyTo(index, array, arrayIndex, count);
        }

        public bool Exists(Predicate<T> match) => list.Exists(match);

        public T Find(Predicate<T> match) => list.Find(match);

        public List<T> FindAll(Predicate<T> match) => list.FindAll(match);

        public int FindIndex(Predicate<T> match) => list.FindIndex(match);

        public int FindIndex(int startIndex, Predicate<T> match) => list.FindIndex(startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match) => list.FindIndex(startIndex, count, match);

        public T FindLast(Predicate<T> match) => list.FindLast(match);

        public int FindLastIndex(Predicate<T> match) => list.FindLastIndex(match);

        public int FindLastIndex(int startIndex, Predicate<T> match) => list.FindLastIndex(startIndex, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match) => list.FindLastIndex(startIndex, count, match);

        public void ForEach(Action<T> action)
        {
            list.ForEach(action);
        }

        public List<T>.Enumerator GetEnumerator() => list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [NotNull]
        public List<T> GetRange(int index, int count) => list.GetRange(index, count);

        public int IndexOf(T item) => list.IndexOf(item);

        public int IndexOf(T item, int index) => list.IndexOf(item, index);

        public int IndexOf(T item, int index, int count) => list.IndexOf(item, index, count);

        public int LastIndexOf(T item) => list.LastIndexOf(item);

        public int LastIndexOf(T item, int index) => list.LastIndexOf(item, index);

        public int LastIndexOf(T item, int index, int count) => list.LastIndexOf(item, index, count);

        public void Reverse()
        {
            list.Reverse();
        }

        public void Reverse(int index, int count)
        {
            list.Reverse(index, count);
        }

        public void Sort()
        {
            list.Sort();
        }

        public void Sort(IComparer<T> comparer)
        {
            list.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            list.Sort(index, count, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            list.Sort(comparison);
        }

        public T[] ToArray() => list.ToArray();

        public void TrimExcess()
        {
            list.TrimExcess();
            set.TrimExcess();
        }

        public bool TrueForAll(Predicate<T> match) => list.TrueForAll(match);

        #endregion

        #region ISet Implementation

        public HashSet<T> UnionWithPure(IEnumerable<T> other)
        {
            var newSet = set.ToHashSet();
            newSet.UnionWith(other);
            return newSet;
        }

        public HashSet<T> IntersectWithPure(IEnumerable<T> other)
        {
            var newSet = set.ToHashSet();
            newSet.IntersectWith(other);
            return newSet;
        }

        public HashSet<T> ExceptWithPure(IEnumerable<T> other)
        {
            var newSet = set.ToHashSet();
            newSet.ExceptWith(other);
            return newSet;
        }

        public HashSet<T> SymmetricExceptWithPure(IEnumerable<T> other)
        {
            var newSet = set.ToHashSet();
            newSet.SymmetricExceptWith(other);
            return newSet;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            set.UnionWith(other);
            MakeListEqualToSet();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            set.IntersectWith(other);
            MakeListEqualToSet();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            set.ExceptWith(other);
            MakeListEqualToSet();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            set.SymmetricExceptWith(other);
            MakeListEqualToSet();
        }

        public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);

        public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);

        public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);

        #endregion

        public bool Equals(SetList<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Count != other.Count) return false;
            for (int i = 0; i < list.Count; i++) if (!Comparer.Equals(list[i], other.list[i])) return false;
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as SetList<T>);

        public override int GetHashCode()
        {
            unchecked
            {
                if (Count == 0) return 0;
                int hashCode = Comparer.GetHashCode(list[0]);
                for (int i = 1; i < list.Count; i++) hashCode = (hashCode * 397) ^ Comparer.GetHashCode(list[i]);
                return hashCode;
            }
        }

        public override string ToString() => list.ToReadableString();
        public T Peek()
        {
            if (Count == 0) return new Stack<T>().Peek(); //This will throw an InvalidOperationException
            return this[0];
        }

        public T Pop()
        {
            if (Count == 0) return new Stack<T>().Pop(); //This will throw an InvalidOperationException
            T item = this[0];
            RemoveAt(0);
            return item;
        }

        public void PushOrMoveToTop(T item)
        {
            InsertOrMove(0, item);
        }
    }
}
