#region Imports

using System.Collections.Generic;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Collections
{
    [PublicAPI]
    public sealed class BiDictionary<TFirst, TSecond>
    {
        private readonly Dictionary<TFirst, TSecond> _firstToSecond = new Dictionary<TFirst, TSecond>();
        private readonly Dictionary<TSecond, TFirst> _secondToFirst = new Dictionary<TSecond, TFirst>();

        private void Add([NotNull] TFirst first, [NotNull] TSecond second)
        {
            // Delete values, in case they are already mapped
            _firstToSecond.Remove(first);
            _secondToFirst.Remove(second);

            // Add values
            _firstToSecond[first] = second;
            _secondToFirst[second] = first;
        }

        public void AddRange([NotNull] Dictionary<TFirst, TSecond> firstToSecondMap)
        {
            foreach (TFirst first in firstToSecondMap.Keys) Add(first, firstToSecondMap[first]);
        }

        public void Clear()
        {
            _firstToSecond.Clear();
            _secondToFirst.Clear();
        }

        # region Get/Set Properties

        [CanBeNull]
        public TSecond this[[NotNull] TFirst first] => _firstToSecond.TryGetValue(first, out TSecond result) ? result : default;

        [CanBeNull]
        public TFirst this[[NotNull] TSecond second] => _secondToFirst.TryGetValue(second, out TFirst result) ? result : default;

        # endregion

        # region Keys/Values Access

        [NotNull]
        public ICollection<TFirst> Keys => _firstToSecond.Keys;

        [NotNull]
        public ICollection<TSecond> Values => _firstToSecond.Values;

        #endregion

    }
}