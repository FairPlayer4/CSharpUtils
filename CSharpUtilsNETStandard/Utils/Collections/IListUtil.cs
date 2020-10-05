#region Imports

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Collections
{
    [PublicAPI]
    public interface IReadOnlyListUtil<T>
    {
        [Pure]
        int BinarySearch(int index, int count, [CanBeNull] T item, [NotNull] IComparer<T> comparer);

        [Pure]
        int BinarySearch([CanBeNull] T item);

        [Pure]
        int BinarySearch([CanBeNull] T item, [NotNull] IComparer<T> comparer);

        [NotNull, ItemCanBeNull, Pure]
        List<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter);

        void CopyTo([NotNull] T[] array);

        void CopyTo([NotNull] T[] array, int arrayIndex);

        void CopyTo(int index, [NotNull] T[] array, int arrayIndex, int count);

        [Pure]
        bool Exists([NotNull] Predicate<T> match);

        [CanBeNull, Pure]
        T Find([NotNull] Predicate<T> match);

        [NotNull, ItemCanBeNull, Pure]
        List<T> FindAll([NotNull] Predicate<T> match);

        [Pure]
        int FindIndex([NotNull] Predicate<T> match);

        [Pure]
        int FindIndex(int startIndex, [NotNull] Predicate<T> match);

        [Pure]
        int FindIndex(int startIndex, int count, [NotNull] Predicate<T> match);

        [CanBeNull, Pure]
        T FindLast([NotNull] Predicate<T> match);

        [Pure]
        int FindLastIndex([NotNull] Predicate<T> match);

        [Pure]
        int FindLastIndex(int startIndex, [NotNull] Predicate<T> match);

        [Pure]
        int FindLastIndex(int startIndex, int count, [NotNull] Predicate<T> match);

        void ForEach([NotNull] Action<T> action);

        [NotNull, ItemCanBeNull, Pure]
        List<T> GetRange(int index, int count);

        [Pure]
        int IndexOf([CanBeNull] T item);

        [Pure]
        int IndexOf([CanBeNull] T item, int index);

        [Pure]
        int IndexOf([CanBeNull] T item, int index, int count);

        [Pure]
        int LastIndexOf([CanBeNull] T item);

        [Pure]
        int LastIndexOf([CanBeNull] T item, int index);

        [Pure]
        int LastIndexOf([CanBeNull] T item, int index, int count);

        [NotNull, Pure]
        T[] ToArray();

        [Pure]
        bool TrueForAll([NotNull] Predicate<T> match);
    }

    /// <summary>
    /// All method that the standard List provides that are not defined in any Interfaces.
    /// Used by SetList and DictionaryList so they have all the functionality of the standard List.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public interface IListUtil<T> : IReadOnlyListUtil<T>
    {
        void Reverse();

        void Reverse(int index, int count);

        void Sort();

        void Sort([NotNull] IComparer<T> comparer);

        void Sort(int index, int count, [NotNull] IComparer<T> comparer);

        void Sort([NotNull] Comparison<T> comparison);

        void TrimExcess();
    }
}
