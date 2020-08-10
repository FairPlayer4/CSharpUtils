#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CSharpUtilsNETStandard.Utils.Extensions.Collections.ToString;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    /// <summary>
    /// These Extensions are meant for reading XML Data from XElements.
    /// It can be applied to Elements and Attributes which are accessed by name.
    /// They can be used for all basic struct types that implement IConvertible (int, long, double, bool, ...) and for string.
    /// The methods will not throw any exceptions and will never return null.
    /// The methods will return the default value in case of any failure which can be specified with the parameters.
    /// The methods will check that the Attribute/Element Name exists in the XElement.
    /// If the Element/Attribute Name exists then their Value is converted using the ConversionExtensions
    /// </summary>
    [PublicAPI]
    public static class XmlExtensions
    {
        private const string Category = nameof(XmlExtensions);

        private const string XElementString = "XElement";
        private const string XAttributeString = "XAttribute";

        public static T GetAttributeValueOrDefault<T>([NotNull] this XElement xElement, [NotNull] string attributeName, T defaultValue = default, [CanBeNull] IFormatProvider formatProvider = null,
                                                      LogLevel failureLogLevel = LogLevel.WARNING) where T : struct, IConvertible
        {
            XAttribute xAttribute = xElement.Attribute(attributeName);
            return xAttribute != null
                       ? xAttribute.Value.ConvertOrDefault(defaultValue, formatProvider, failureLogLevel)
                       : PrintMissingMessageAndReturnDefault(xElement, XAttributeString, attributeName, defaultValue, failureLogLevel);
        }

        [NotNull]
        public static string GetAttributeStringValueOrDefault([NotNull] this XElement xElement, [NotNull] string attributeName, [NotNull] string defaultValue = "",
                                                        LogLevel failureLogLevel = LogLevel.WARNING)
        {
            XAttribute xAttribute = xElement.Attribute(attributeName);
            return xAttribute != null ? xAttribute.Value : PrintMissingMessageAndReturnDefault(xElement, XAttributeString, attributeName, defaultValue, failureLogLevel);
        }

        [NotNull]
        public static IEnumerable<T> GetAttributeValueAsEnumerableOrEmpty<T>([NotNull] this XElement xElement, [NotNull] string attributeName, [NotNull] string separator,
                                                                             T defaultValueOfElements = default, [CanBeNull] IFormatProvider formatProvider = null,
                                                                             LogLevel failureLogLevel = LogLevel.WARNING) where T : struct, IConvertible
        {
            XAttribute xAttribute = xElement.Attribute(attributeName);
            return xAttribute != null
                       ? xAttribute.Value.Split(separator.ToCharArray()).Select(x => x.ConvertOrDefault(defaultValueOfElements, formatProvider, failureLogLevel))
                       : PrintMissingMessageAndReturnEmpty<T>(xElement, XAttributeString, attributeName, failureLogLevel);
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetAttributeValueAsEnumerableOrEmpty([NotNull] this XElement xElement, [NotNull] string attributeName, [NotNull] string separator,
                                                                               LogLevel failureLogLevel = LogLevel.WARNING)
        {
            XAttribute xAttribute = xElement.Attribute(attributeName);
            return xAttribute != null ? xAttribute.Value.Split(separator.ToCharArray()) : PrintMissingMessageAndReturnEmpty<string>(xElement, XAttributeString, attributeName, failureLogLevel);
        }

        public static T GetElementValueOrDefault<T>([NotNull] this XElement xElement, [NotNull] string elementName, T defaultValue = default, [CanBeNull] IFormatProvider formatProvider = null,
                                                    LogLevel failureLogLevel = LogLevel.WARNING) where T : struct, IConvertible
        {
            XElement element = xElement.Element(elementName);
            return element != null
                       ? element.Value.ConvertOrDefault(defaultValue, formatProvider, failureLogLevel)
                       : PrintMissingMessageAndReturnDefault(xElement, XElementString, elementName, defaultValue, failureLogLevel);
        }

        [NotNull]
        public static string GetElementStringValueOrDefault([NotNull] this XElement xElement, [NotNull] string elementName, [NotNull] string defaultValue = "",
                                                            LogLevel failureLogLevel = LogLevel.WARNING)
        {
            XElement element = xElement.Element(elementName);
            return element != null ? element.Value : PrintMissingMessageAndReturnDefault(xElement, XElementString, elementName, defaultValue, failureLogLevel);
        }

        [NotNull]
        public static IEnumerable<T> GetElementValueAsEnumerableOrEmpty<T>([NotNull] this XElement xElement, [NotNull] string elementName, [NotNull] string separator,
                                                                           T defaultValueOfElements = default, [CanBeNull] IFormatProvider formatProvider = null,
                                                                           LogLevel failureLogLevel = LogLevel.WARNING) where T : struct, IConvertible
        {
            XElement element = xElement.Element(elementName);
            return element != null
                       ? element.Value.Split(separator.ToCharArray()).Select(x => x.ConvertOrDefault(defaultValueOfElements, formatProvider, failureLogLevel))
                       : PrintMissingMessageAndReturnEmpty<T>(xElement, XElementString, elementName, failureLogLevel);
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetElementValueAsEnumerableOrEmpty([NotNull] this XElement xElement, [NotNull] string elementName, [NotNull] string separator,
                                                                             LogLevel failureLogLevel = LogLevel.WARNING)
        {
            XElement element = xElement.Element(elementName);
            return element != null ? element.Value.Split(separator.ToCharArray()) : PrintMissingMessageAndReturnEmpty<string>(xElement, XElementString, elementName, failureLogLevel);
        }

        [NotNull]
        public static IEnumerable<XElement> GetSubElementsOrEmpty([NotNull] this XElement xElement, [NotNull] string elementName, LogLevel failureLogLevel = LogLevel.WARNING)
        {
            XElement element = xElement.Element(elementName);
            return element != null ? element.Elements() : PrintMissingMessageAndReturnEmpty<XElement>(xElement, XElementString, elementName, failureLogLevel);
        }

        [NotNull]
        private static T PrintMissingMessageAndReturnDefault<T>([NotNull] XElement xElement, [NotNull] string whatIsMissing, [NotNull] string name, [NotNull] T defaultValue,
                                                                LogLevel failureLogLevel)
        {
            PrintMissingMessage(xElement, whatIsMissing, name, defaultValue.ToString(), failureLogLevel);
            return defaultValue;
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<T> PrintMissingMessageAndReturnEmpty<T>([NotNull] XElement xElement, [NotNull] string whatIsMissing, [NotNull] string name,
                                                                           LogLevel failureLogLevel)
        {
            PrintMissingMessage(xElement, whatIsMissing, name, string.Format("Enumerable.Empty<{0}>()", typeof(T).Name), failureLogLevel);
            return Enumerable.Empty<T>();
        }

        private static void PrintMissingMessage([NotNull] XElement xElement, [NotNull] string whatIsMissing, [NotNull] string name, [NotNull] string defaultValue,
                                                LogLevel failureLogLevel)
        {
            string message = string.Format(
                "The {0} with the Name {1} was null in the XElement with the Name {2} and the Attributes {3} and Elements {4}!\nThis may be caused by an invalid XML String or an incorrect Database SQL Query!\nThe default value {5} will be returned.",
                whatIsMissing,
                name,
                xElement.Name,
                xElement.Attributes().Select(x => x != null ? string.Format("(Name: {0}, Value: {1})", x.Name, x.Value) : "(NULL)").ToReadableString(),
                xElement.Elements().Select(x => string.Format("(Name: {0}, Value: {1})", x.Name, x.Value)).ToReadableString(),
                defaultValue);
            Logger.PrintLogLevel(failureLogLevel, message, Category);
        }
    }
}
