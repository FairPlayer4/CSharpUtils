#region Imports

using System.Collections.Generic;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Collections
{
    [PublicAPI]
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>
    {
        [Pure]
        bool Contains([NotNull] T item);
        [NotNull, Pure]
        HashSet<T> UnionWithPure([NotNull] IEnumerable<T> other);
        [NotNull, Pure]
        HashSet<T> IntersectWithPure([NotNull] IEnumerable<T> other);
        [NotNull, Pure]
        HashSet<T> ExceptWithPure([NotNull] IEnumerable<T> other);
        [NotNull, Pure]
        HashSet<T> SymmetricExceptWithPure([NotNull] IEnumerable<T> other);
        [Pure]
        bool IsSubsetOf([NotNull] IEnumerable<T> other);
        [Pure]
        bool IsSupersetOf([NotNull] IEnumerable<T> other);
        [Pure]
        bool IsProperSupersetOf([NotNull] IEnumerable<T> other);
        [Pure]
        bool IsProperSubsetOf([NotNull] IEnumerable<T> other);
        [Pure]
        bool Overlaps([NotNull] IEnumerable<T> other);
        [Pure]
        bool SetEquals([NotNull] IEnumerable<T> other);
    }

    [PublicAPI]
    public interface IAddOnlySet<T> : IReadOnlySet<T>
    {
        bool Add([NotNull] T item);
        void AddRange([NotNull] IEnumerable<T> items);
    }

    [PublicAPI]
    public interface IStackSet<T> : IReadOnlyCollection<T>
    {
        T Peek();

        T Pop();

        void PushOrMoveToTop([NotNull] T item);
    }
}
