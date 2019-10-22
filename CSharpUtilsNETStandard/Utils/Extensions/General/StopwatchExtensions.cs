#region Imports

using System.Diagnostics;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    public static class StopwatchExtensions
    {

        public static void PrintElapsedAndRestartTrace([NotNull]this Stopwatch stopwatch, string action, LogLevel logLevel = LogLevel.WARNING)
        {
            stopwatch.PrintElapsedAndRestart(action, LogLevel.TRACE);
        }

        public static void PrintElapsedAndRestart([NotNull]this Stopwatch stopwatch, string action, LogLevel logLevel = LogLevel.WARNING)
        {
            stopwatch.PrintLogLevel(logLevel, stopwatch.GetElapsedText(action));
            stopwatch.Restart();
        }

        public static void PrintElapsedTrace([NotNull]this Stopwatch stopwatch, string action)
        {
            stopwatch.PrintElapsed(action, LogLevel.TRACE);
        }

        public static void PrintElapsed([NotNull]this Stopwatch stopwatch, string action, LogLevel logLevel = LogLevel.WARNING)
        {
            stopwatch.PrintLogLevel(logLevel, stopwatch.GetElapsedText(action));
        }

        [NotNull]
        private static string GetElapsedText([NotNull]this Stopwatch stopwatch, string action)
        {
            return $"{action} took (ms): {stopwatch.TotalMilliseconds()}";
        }

        public static double TotalMilliseconds([NotNull] this Stopwatch stopwatch)
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
