using System.Collections.Generic;
using JetBrains.Annotations;

namespace CSharpUtilsNETStandard.Utils.Collections
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface IReadOnlyDictionaryList<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IReadOnlyList<TKey>, IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        [NotNull]
        IReadOnlyList<TKey> KeyList { get; }

        bool ContainsValue([CanBeNull] TValue value);

        new int Count { get; }

        new DictionaryList<TKey, TValue>.DictionaryListEnumerator GetEnumerator();

        TKey GetKeyAt(int index);

        TValue GetValueAt(int index);

        KeyValuePair<TKey, TValue> GetAt(int index);

        [CanBeNull]
        TValue GetValueOrDefault([NotNull] TKey key, [CanBeNull] TValue defaultValue = default);

    }
}
