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
        [UsedImplicitly, Pure]
        int BinarySearch(int index, int count, [CanBeNull] T item, [CanBeNull] IComparer<T> comparer);

        [UsedImplicitly, Pure]
        int BinarySearch([CanBeNull] T item);

        [UsedImplicitly, Pure]
        int BinarySearch([CanBeNull] T item, [CanBeNull] IComparer<T> comparer);

        [UsedImplicitly, NotNull, Pure]
        List<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter);

        [UsedImplicitly]
        void CopyTo([NotNull] T[] array);

        [UsedImplicitly]
        void CopyTo([NotNull] T[] array, int arrayIndex);

        [UsedImplicitly]
        void CopyTo(int index, [NotNull] T[] array, int arrayIndex, int count);

        [UsedImplicitly, Pure]
        bool Exists([NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        T Find([NotNull] Predicate<T> match);

        [UsedImplicitly, NotNull, Pure]
        List<T> FindAll([NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        int FindIndex([NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        int FindIndex(int startIndex, [NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        int FindIndex(int startIndex, int count, [NotNull] Predicate<T> match);

        [UsedImplicitly, CanBeNull, Pure]
        T FindLast([NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        int FindLastIndex([NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        int FindLastIndex(int startIndex, [NotNull] Predicate<T> match);

        [UsedImplicitly, Pure]
        int FindLastIndex(int startIndex, int count, [NotNull] Predicate<T> match);

        [UsedImplicitly]
        void ForEach([NotNull] Action<T> action);

        [UsedImplicitly, Pure]
        List<T> GetRange(int index, int count);

        [UsedImplicitly, Pure]
        int IndexOf([CanBeNull] T item);

        [UsedImplicitly, Pure]
        int IndexOf([CanBeNull] T item, int index);

        [UsedImplicitly, Pure]
        int IndexOf([CanBeNull] T item, int index, int count);

        [UsedImplicitly, Pure]
        int LastIndexOf([CanBeNull] T item);

        [UsedImplicitly, Pure]
        int LastIndexOf([CanBeNull] T item, int index);

        [UsedImplicitly, Pure]
        int LastIndexOf([CanBeNull] T item, int index, int count);

        [UsedImplicitly, NotNull, Pure]
        T[] ToArray();

        [UsedImplicitly, Pure]
        bool TrueForAll([NotNull] Predicate<T> match);
    }

    /// <summary>
    /// All method that the standard List provides that are not defined in any Interfaces.
    /// Used by SetList and DictionaryList so they have all the functionality of the standard List.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IListUtil<T> : IReadOnlyListUtil<T>
    {
        [UsedImplicitly]
        void Reverse();

        [UsedImplicitly]
        void Reverse(int index, int count);

        [UsedImplicitly]
        void Sort();

        [UsedImplicitly]
        void Sort([CanBeNull] IComparer<T> comparer);

        [UsedImplicitly]
        void Sort(int index, int count, [CanBeNull] IComparer<T> comparer);

        [UsedImplicitly]
        void Sort([NotNull] Comparison<T> comparison);

        [UsedImplicitly]
        void TrimExcess();
    }
}
