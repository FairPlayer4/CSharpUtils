#region Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CSharpUtilsNETStandard.Utils.Extensions.Collections.ToString;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    [PublicAPI]
    public static class ConversionExtensions
    {
        private const string Category = nameof(ConversionExtensions);

        public static T As<T>([CanBeNull] this object obj)
            where T : class
        {
            return obj as T;
        }

        public static T UnboxOrDefault<T>([CanBeNull] this object o, T defaultValue = default)
            where T : struct, IConvertible
        {
            switch (o)
            {
                case null:          return defaultValue;
                case T convertible: return convertible;
                default:            return o.ToString().ConvertOrDefault(defaultValue); //Last effort
            }
        }

        [NotNull]
        public static T CastOrDefault<T>([CanBeNull] this object o, [NotNull] T defaultValue)
        {
            if (o is T o1) return o1;
            return defaultValue;
        }

        [CanBeNull]
        public static T CastOrDefaultAllowNull<T>([CanBeNull] this object o, [CanBeNull] T defaultValue)
        {
            if (o is T o1) return o1;
            return defaultValue;
        }

        [CanBeNull]
        public static string AsStringOrDefault([CanBeNull] this object o, [CanBeNull] string defaultString)
        {
            if (o == null) return defaultString;
            return o as string ?? o.ToString();
        }

        [CanBeNull]
        public static TTo SafeCastWithFailureWarning<TFrom, TTo>([CanBeNull] this TFrom toBeCast)
            where TTo : class
            where TFrom : class
        {
            return toBeCast as TTo ?? toBeCast.ReturnCastFailure<TFrom, TTo>();
        }

        [CanBeNull]
        private static TTo ReturnCastFailure<TFrom, TTo>([CanBeNull] this TFrom original)
            where TTo : class
            where TFrom : class
        {
            //Does not have to be efficient because it will rarely happen
            string interfaceName = typeof(TFrom).Name;
            string castName = typeof(TTo).Name;
            string message = $"Cast from {interfaceName} to {castName} failed!";
            if (original == null)
            {
                message += $" Reason: {interfaceName} was null!";
                Logger.PrintTrace(message);
            }
            else
            {
                Logger.PrintWarning(message);
            }
            return null;
        }

        [NotNull]
        private delegate (object, bool) ConvertOrDefaultFunc([NotNull] string value, [NotNull] object defaultValue);

        [NotNull]
        private static readonly IReadOnlyDictionary<Type, ConvertOrDefaultFunc> DefaultStringToEnumConverters = new Dictionary<Type, ConvertOrDefaultFunc>();

        public static bool IsEnumDefined<T>(this T enumValue)
            where T : struct, IConvertible
        {
            return Enum.IsDefined(typeof(T), enumValue);
        }

        public static T GetEnumOrDefault<T>(this int enumValue, T defaultValue)
            where T : struct, IConvertible
        {
            try
            {
                if (Enum.IsDefined(typeof(T), enumValue)) return (T)Enum.ToObject(typeof(T), enumValue);
            }
            catch (Exception e)
            {
                Logger.PrintWarning($"An exception during Int32 (Value: {enumValue}) to Enum (Type: {typeof(T).Name}) conversion occurred!", exception: e);
            }
            return defaultValue;
        }

        public static double ConvertToDouble([NotNull] this string value, double defaultValue, NumberStyles numberStyles = NumberStyles.Any, [CanBeNull] IFormatProvider formatProvider = null, LogLevel failureLogLevel = LogLevel.WARNING)
        {
            if (double.TryParse(value, numberStyles, formatProvider ?? CultureInfo.InvariantCulture, out double result)) return result;
            Logger.PrintLogLevel(failureLogLevel, $"The conversion of the string \"{value}\" to double failed.\nThe default value {defaultValue} will be returned.", Category);
            return defaultValue;
        }

        [NotNull]
        public static T ConvertOrDefault<T>([CanBeNull] this string value, [NotNull] T defaultValue, [CanBeNull] IFormatProvider formatProvider = null, [CanBeNull] LogLevel? failureLogLevel = LogLevel.WARNING)
            where T : IConvertible
        {
            return ConvertOrDefault(value, defaultValue, out bool _, formatProvider, failureLogLevel);
        }

        [NotNull]
        public static T ConvertOrDefaultNoLogging<T>([CanBeNull] this string value, [NotNull] T defaultValue, [CanBeNull] IFormatProvider formatProvider = null)
            where T : IConvertible
        {
            return ConvertOrDefault(value, defaultValue, formatProvider, null);
        }

        public static T? ConvertOrNull<T>([CanBeNull] this string value, [CanBeNull] IFormatProvider formatProvider = null, [CanBeNull] LogLevel? failureLogLevel = null)
            where T : struct, IConvertible
        {
            T conversionValue = ConvertOrDefault<T>(value, default, out bool success, formatProvider, failureLogLevel);
            if (!success) return null;
            return conversionValue;
        }

        /// <summary>
        /// Safely converts a string to the given type.
        /// Safely means that no known exceptions will be thrown.
        /// The method never returns null because T is a struct.
        /// By default the conversion uses the IFormatProvider CultureInfo.InvariantCulture for the conversion.
        /// In case of double, DateTime, etc. it can make sense to provide a different IFormatProvider.
        /// However, it makes sense to format the string that is converted in advance to avoid conversion failure.
        /// For example replacing the ',' with '.' in a string that is converted to a double.
        /// This method only works for structs which implement IConvertible meaning the standard .NET structs like bool, int, double, DateTime, etc.
        /// You can see all structs that implement IConvertible under https://docs.microsoft.com/de-de/dotnet/api/system.iconvertible?view=netframework-4.7.2#derived
        /// A default value can be provided which will be returned if the conversion fails for any reason.
        /// If both warning and error messages are deactivated by setting the parameters to false then a trace message is printed for debugging purposes.
        /// </summary>
        /// <typeparam name="T">Can only be a struct that implements IConvertible</typeparam>
        /// <param name="value">A string that can be null (if it is null the default value is returned)</param>
        /// <param name="defaultValue">The default value that is returned if the conversion fails for any reason. (default: default(T))</param>
        /// <param name="success">Is true if the conversion was successful otherwise false</param>
        /// <param name="formatProvider">The IFormatProvider that is used for the conversion. (default: CultureInfo.InvariantCulture)</param>
        /// <param name="failureLogLevel">Prints a message with the specified LogLevel if the conversion fails except if the string value is null. (default: LogLevel.WARNING)</param>
        /// <returns></returns>
        [NotNull]
        public static T ConvertOrDefault<T>([CanBeNull] this string value, [NotNull] T defaultValue, out bool success, [CanBeNull] IFormatProvider formatProvider = null, [CanBeNull] LogLevel? failureLogLevel = LogLevel.WARNING)
            where T : IConvertible
        {
            try
            {
                Type returnType = typeof(T);
                success = true;
                if (returnType.IsEnum)
                {
                    value = value ?? "";
                    if (DefaultStringToEnumConverters.TryGetValue(returnType, out ConvertOrDefaultFunc converter))
                    {
                        (object item1, bool item2) = converter(value, defaultValue);
                        success = item2;
                        if (success) return (T)item1;
                        Logger.PrintWarning($"Could not convert the string value \"{value}\" into a valid {returnType.Name}.\nUsing the default value: {defaultValue}");
                        return defaultValue;
                    }
                    if (value.Trim().All(char.IsDigit)) //Special Case
                    {
                        if (int.TryParse(value.Trim(), NumberStyles.Any, formatProvider ?? CultureInfo.InvariantCulture, out int intValue))
                        {
                            return (T)Convert.ChangeType(intValue, Enum.GetUnderlyingType(returnType), formatProvider ?? CultureInfo.InvariantCulture);
                        }
                    }
                    return (T)Enum.Parse(returnType, value, true);
                }
                if (value == null || returnType.IsValueType && string.IsNullOrWhiteSpace(value))
                {
                    success = false;
                    return PrintConversionFailureMessageAndReturnDefault(value, null, defaultValue, failureLogLevel);
                }
                return (T)Convert.ChangeType(value, returnType, formatProvider ?? CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException e)
            {
                success = false;
                return PrintConversionFailureMessageAndReturnDefault(value, e, defaultValue, failureLogLevel);
            }
            catch (FormatException e)
            {
                success = false;
                return PrintConversionFailureMessageAndReturnDefault(value, e, defaultValue, failureLogLevel);
            }
            catch (ArgumentException e)
            {
                success = false;
                return PrintConversionFailureMessageAndReturnDefault(value, e, defaultValue, failureLogLevel);
            }
            catch (OverflowException e)
            {
                success = false;
                return PrintConversionFailureMessageAndReturnDefault(value, e, defaultValue, failureLogLevel);
            }
        }

        [NotNull]
        private static T PrintConversionFailureMessageAndReturnDefault<T>([CanBeNull] string value, [CanBeNull] Exception exception, [NotNull] T defaultValue, [CanBeNull] LogLevel? failureLogLevel)
            where T : IConvertible
        {
            if (failureLogLevel.HasValue)
            {
                string message = GetMessage(value, defaultValue, exception == null);
                Logger.PrintLogLevel(failureLogLevel.Value, message, Category, exception);
            }
            return defaultValue;
        }

        [NotNull]
        private static string GetMessage<T>([CanBeNull] string value, [NotNull] T defaultValue, bool nullOrWhiteSpaceMessage)
            where T : IConvertible
        {
            string nonNullValue = value == null ? $"(without first and last \") \"{value}\"" : "NULL";
            string defaultValueString = defaultValue.ToStringOrEmpty();
            if (string.IsNullOrWhiteSpace(defaultValueString)) defaultValueString = $"(without first and last \") \"{defaultValueString}\"";
            StringBuilder message = new StringBuilder();
            message.AppendFormat("The string {0} could not be converted to the type {1}.\nReason: ", nonNullValue, typeof(T).Name);
            message.Append(nullOrWhiteSpaceMessage ? "The string was null, empty or only whitespaces." : "See exception below.");
            if (typeof(T).IsEnum) message.AppendFormat("\nAllowed enum values: {0}", typeof(T).GetEnumNames()?.ToReadableString() ?? $"ERROR! {typeof(T).GetEnumNames()} returned null!");
            message.AppendFormat("\nThis error could be due to invalid data stored in the database or bugs in the code.\nThe default value {0} will be returned.", defaultValueString);
            message.AppendFormat("\n> Full Stack-Trace:\n{0}", new StackTrace(true));
            return message.ToString();
        }
    }
}
