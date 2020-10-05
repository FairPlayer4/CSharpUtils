#region Imports

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    [PublicAPI]
    public static class MiscExtensions
    {
        [NotNull]
        public static string ToStringLowerCase(this bool value)
        {
            return value.ToString().ToLowerInvariant();
        }

        public static bool IsValidIndex(this int index, int count)
        {
            return index >= 0 || index < count;
        }

        [NotNull]
        public static string ToStringOrEmpty<T>([CanBeNull] this T obj)
        {
            return obj == null ? string.Empty : obj.ToString() ?? string.Empty;
        }

        public static bool IsValidIndex(this short index, int count)
        {
            return index >= 0 || index < count;
        }

        [NotNull]
        public static string GetTypeName<T>([NotNull] this T t)
        {
            return t.GetType().Name;
        }

        public static void AppendIfNotEmpty([NotNull] this StringBuilder stringBuilder, [NotNull] string value)
        {
            if (stringBuilder.Length > 0) stringBuilder.Append(value);
        }

        public static bool ImplementsInterface([NotNull] this Type type, [CanBeNull] Type interfaceType)
        {
            return type.GetInterfaces()
                       .FirstOrDefault(ifes =>
                                           ifes.IsGenericType
                                        && ifes.GetGenericTypeDefinition() == interfaceType)
                != null;
        }

        public static bool IsOverridden([NotNull] Type baseType, [NotNull] Type testedType, [NotNull] string methodName)
        {
            MethodInfo deletionMethod = testedType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (deletionMethod == null)
            {
                testedType.PrintWarning($"The method \"{methodName}\" should exist but does not exist!");
                return false;
            }
            return deletionMethod.DeclaringType == baseType;
        }
    }
}
