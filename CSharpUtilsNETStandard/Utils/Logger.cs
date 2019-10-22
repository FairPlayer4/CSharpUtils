#region Imports

using System;
using System.Diagnostics;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils
{
    //TODO find out about potential security hazards (redirecting logs)
    public delegate void PrintLogDelegate(LogLevel level, string msg, string category);
    public delegate void PrintLogExceptionDelegate(LogLevel level, string msg, Exception exception, string category);
    public delegate void PrintLogDelegateEx(LogLevel level, string msg, object obj);
    public delegate void PrintLogExceptionDelegateEx(LogLevel level, string msg, Exception exception, object obj);

    public enum LogLevel
    {
        ERROR, WARNING, INFO, TRACE, GARBAGE
    }

    public static class Logger
    {
        [NotNull]
        private const string DefaultCategory = "CSharpUtils";
        [CanBeNull]
        public static PrintLogDelegate PrintLog { get; set; }
        [CanBeNull]
        public static PrintLogExceptionDelegate PrintLogException { get; set; }
        [CanBeNull]
        public static PrintLogDelegateEx PrintLogEx { get; set; }
        [CanBeNull]
        public static PrintLogExceptionDelegateEx PrintLogExceptionEx { get; set; }

        private static void PrintDebug([NotNull]string msg, [NotNull]string debugLevel, [NotNull]string category)
        {
            //TODO define syntax clearly
            debugLevel = "@[" + debugLevel + "]::";
            if (category != "") category = "**[" + category + "]**";
            Debug.WriteLine(debugLevel + category + msg);
        }

        public static void PrintError([NotNull]string msg, [NotNull]string category = DefaultCategory, [CanBeNull]Exception exception = null)
        {
            PrintLogLevel(LogLevel.ERROR, msg, category, exception);
        }

        public static void PrintWarning([NotNull]string msg, [NotNull]string category = DefaultCategory, [CanBeNull]Exception exception = null)
        {
            PrintLogLevel(LogLevel.WARNING, msg, category, exception);
        }

        public static void PrintInfo([NotNull]string msg, [NotNull]string category = DefaultCategory, [CanBeNull]Exception exception = null)
        {
            PrintLogLevel(LogLevel.INFO, msg, category, exception);
        }

        public static void PrintTrace([NotNull]string msg, [NotNull]string category = DefaultCategory, [CanBeNull]Exception exception = null)
        {
            PrintLogLevel(LogLevel.TRACE, msg, category, exception);
        }

        public static void PrintGarbage([NotNull]string msg, [NotNull]string category = DefaultCategory, [CanBeNull]Exception exception = null)
        {
            PrintLogLevel(LogLevel.GARBAGE, msg, category, exception);
        }

        public static void PrintLogLevel(LogLevel logLevel, [NotNull]string msg, [NotNull]string category, [CanBeNull]Exception exception)
        {
            if (exception != null && PrintLogException != null) PrintLogException(logLevel, msg, exception, category);
            else if (PrintLog != null) PrintLog(logLevel, msg, category);
            else PrintDebug(msg, logLevel.ToString(), category);
        }

        public static void PrintError([NotNull]this object obj, [NotNull]string msg, [CanBeNull]Exception exception = null)
        {
            obj.PrintLogLevel(LogLevel.ERROR, msg, exception);
        }

        public static void PrintWarning([NotNull]this object obj, [NotNull]string msg, [CanBeNull]Exception exception = null)
        {
            obj.PrintLogLevel(LogLevel.WARNING, msg, exception);
        }

        public static void PrintInfo([NotNull]this object obj, [NotNull]string msg, [CanBeNull]Exception exception = null)
        {
            obj.PrintLogLevel(LogLevel.INFO, msg, exception);
        }

        public static void PrintTrace([NotNull]this object obj, [NotNull]string msg, [CanBeNull]Exception exception = null)
        {
            obj.PrintLogLevel(LogLevel.TRACE, msg, exception);
        }

        public static void PrintGarbage([NotNull]this object obj, [NotNull]string msg, [CanBeNull]Exception exception = null)
        {
            obj.PrintLogLevel(LogLevel.GARBAGE, msg, exception);
        }

        public static void PrintLogLevel([NotNull]this object obj, LogLevel logLevel, [NotNull]string msg, [CanBeNull] Exception exception = null)
        {
            if (exception != null && PrintLogExceptionEx != null) PrintLogExceptionEx(logLevel, msg, exception, obj);
            else if (PrintLogEx != null) PrintLogEx(logLevel, msg, obj);
            else PrintDebug(msg, logLevel.ToString(), obj.GetType().Name);
        }
    }
}
