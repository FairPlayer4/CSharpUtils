#region Imports

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.Collections
{

    public interface IDeepCopy<out T>
    {
        [NotNull, Pure]
        T DeepCopy();
    }

    /// <summary>
    /// Provides the DeepCopy method for nested Collections.
    /// Works only with immutable types.
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public static class DeepCopyExtension
    {
        // TODO General Refactoring

        //DO NOT USE NULL VALUES! IN GENERAL NEVER USE NULL VALUES IN COLLECTIONS!

        #region DeepCopy Interface

        [NotNull]
        public static List<T> DeepCopySpecial<T>([NotNull]this List<T> list) where T : IDeepCopy<T>
        {
            return list.ConvertAll(element => element.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, TValue> DeepCopySpecial<TValue>([NotNull]this Dictionary<string, TValue> dictionary) where TValue : IDeepCopy<TValue>
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey, TValue> DeepCopySpecial<TKey, TValue>([NotNull]this Dictionary<TKey, TValue> dictionary) where TKey : struct where TValue : IDeepCopy<TValue>
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }

        #endregion

        #region DeepCopy
        [NotNull, Pure]
        public static List<T> DeepCopy<T>([NotNull]this List<T> list) where T : struct
        {
            return new List<T>(list);
        }
        [NotNull, ItemNotNull]
        public static List<string> DeepCopy([NotNull, ItemNotNull]this List<string> list)
        {
            return new List<string>(list);
        }
        [NotNull, ItemNotNull]
        public static List<List<T>> DeepCopy<T>([NotNull, ItemNotNull]this List<List<T>> list) where T : struct
        {
            return list.Select(innerList => innerList.DeepCopy()).ToList();
        }
        [NotNull, ItemNotNull]
        public static List<List<string>> DeepCopy([NotNull, ItemNotNull]this List<List<string>> list)
        {
            return list.Select(innerList => innerList.DeepCopy()).ToList();
        }
        [NotNull]
        public static Dictionary<TKey, List<T>> DeepCopy<TKey, T>([NotNull]this Dictionary<TKey, List<T>> dictionary) where TKey : struct where T : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey, List<string>> DeepCopy<TKey>([NotNull]this Dictionary<TKey, List<string>> dictionary) where TKey : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, List<T>> DeepCopy<T>([NotNull]this Dictionary<string, List<T>> dictionary) where T : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, List<string>> DeepCopy([NotNull]this Dictionary<string, List<string>> dictionary)
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>([NotNull]this Dictionary<TKey, TValue> dictionary) where TKey : struct where TValue : struct
        {
            return new Dictionary<TKey, TValue>(dictionary);
        }
        [NotNull]
        public static Dictionary<TKey, string> DeepCopy<TKey>([NotNull]this Dictionary<TKey, string> dictionary) where TKey : struct
        {
            return new Dictionary<TKey, string>(dictionary);
        }
        [NotNull]
        public static Dictionary<string, TValue> DeepCopy<TValue>([NotNull]this Dictionary<string, TValue> dictionary) where TValue : struct
        {
            return new Dictionary<string, TValue>(dictionary);
        }
        [NotNull]
        public static Dictionary<string, string> DeepCopy([NotNull]this Dictionary<string, string> dictionary)
        {
            return new Dictionary<string, string>(dictionary);
        }

        #region Nested Dictionaries

        // 2 Dictionaries
        [NotNull]
        public static Dictionary<TKey1, Dictionary<TKey2, TValue>> DeepCopy<TKey1, TKey2, TValue>([NotNull]this Dictionary<TKey1, Dictionary<TKey2, TValue>> dictionary) where TKey1 : struct where TKey2 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<TKey2, string>> DeepCopy<TKey1, TKey2>([NotNull]this Dictionary<TKey1, Dictionary<TKey2, string>> dictionary) where TKey1 : struct where TKey2 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<string, TValue>> DeepCopy<TKey1, TValue>([NotNull]this Dictionary<TKey1, Dictionary<string, TValue>> dictionary) where TKey1 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<TKey2, TValue>> DeepCopy<TKey2, TValue>([NotNull]this Dictionary<string, Dictionary<TKey2, TValue>> dictionary) where TKey2 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<string, string>> DeepCopy<TKey1>([NotNull]this Dictionary<TKey1, Dictionary<string, string>> dictionary) where TKey1 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<TKey2, string>> DeepCopy<TKey2>([NotNull]this Dictionary<string, Dictionary<TKey2, string>> dictionary) where TKey2 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<string, TValue>> DeepCopy<TValue>([NotNull]this Dictionary<string, Dictionary<string, TValue>> dictionary) where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<string, string>> DeepCopy([NotNull]this Dictionary<string, Dictionary<string, string>> dictionary)
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }

        // 3 Dictionaries
        [NotNull]
        public static Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, TValue>>> DeepCopy<TKey1, TKey2, TKey3, TValue>([NotNull]this Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, TValue>>> dictionary) where TKey1 : struct where TKey2 : struct where TKey3 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, string>>> DeepCopy<TKey1, TKey2, TKey3>([NotNull]this Dictionary<TKey1, Dictionary<TKey2, Dictionary<TKey3, string>>> dictionary) where TKey1 : struct where TKey2 : struct where TKey3 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<TKey2, Dictionary<string, TValue>>> DeepCopy<TKey1, TKey2, TValue>([NotNull]this Dictionary<TKey1, Dictionary<TKey2, Dictionary<string, TValue>>> dictionary) where TKey1 : struct where TKey2 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<string, Dictionary<TKey3, TValue>>> DeepCopy<TKey1, TKey3, TValue>([NotNull]this Dictionary<TKey1, Dictionary<string, Dictionary<TKey3, TValue>>> dictionary) where TKey1 : struct where TKey3 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<TKey2, Dictionary<TKey3, TValue>>> DeepCopy<TKey2, TKey3, TValue>([NotNull]this Dictionary<string, Dictionary<TKey2, Dictionary<TKey3, TValue>>> dictionary) where TKey2 : struct where TKey3 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<TKey2, Dictionary<string, string>>> DeepCopy<TKey1, TKey2>([NotNull]this Dictionary<TKey1, Dictionary<TKey2, Dictionary<string, string>>> dictionary) where TKey1 : struct where TKey2 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<string, Dictionary<TKey3, string>>> DeepCopy<TKey1, TKey3>([NotNull]this Dictionary<TKey1, Dictionary<string, Dictionary<TKey3, string>>> dictionary) where TKey1 : struct where TKey3 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<TKey2, Dictionary<TKey3, string>>> DeepCopy<TKey2, TKey3>([NotNull]this Dictionary<string, Dictionary<TKey2, Dictionary<TKey3, string>>> dictionary) where TKey2 : struct where TKey3 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<string, Dictionary<string, TValue>>> DeepCopy<TKey1, TValue>([NotNull]this Dictionary<TKey1, Dictionary<string, Dictionary<string, TValue>>> dictionary) where TKey1 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<TKey2, Dictionary<string, TValue>>> DeepCopy<TKey2, TValue>([NotNull]this Dictionary<string, Dictionary<TKey2, Dictionary<string, TValue>>> dictionary) where TKey2 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<string, Dictionary<TKey3, TValue>>> DeepCopy<TKey3, TValue>([NotNull]this Dictionary<string, Dictionary<string, Dictionary<TKey3, TValue>>> dictionary) where TKey3 : struct where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<TKey1, Dictionary<string, Dictionary<string, string>>> DeepCopy<TKey1>([NotNull]this Dictionary<TKey1, Dictionary<string, Dictionary<string, string>>> dictionary) where TKey1 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<TKey2, Dictionary<string, string>>> DeepCopy<TKey2>([NotNull]this Dictionary<string, Dictionary<TKey2, Dictionary<string, string>>> dictionary) where TKey2 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<string, Dictionary<TKey3, string>>> DeepCopy<TKey3>([NotNull]this Dictionary<string, Dictionary<string, Dictionary<TKey3, string>>> dictionary) where TKey3 : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<string, Dictionary<string, TValue>>> DeepCopy<TValue>([NotNull]this Dictionary<string, Dictionary<string, Dictionary<string, TValue>>> dictionary) where TValue : struct
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }
        [NotNull]
        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> DeepCopy([NotNull]this Dictionary<string, Dictionary<string, Dictionary<string, string>>> dictionary)
        {
            return dictionary.ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value.DeepCopy());
        }

        #endregion

        #endregion

    }
}
