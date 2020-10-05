#region Imports

using System.Diagnostics;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Extensions.General
{
    [PublicAPI]
    public static class StopwatchExtensions
    {
        public static void PrintElapsedAndRestartTrace([NotNull] this Stopwatch stopwatch, [CanBeNull] string action)
        {
            stopwatch.PrintElapsedAndRestart(action, LogLevel.TRACE);
        }

        public static void PrintElapsedAndRestart([NotNull] this Stopwatch stopwatch, [CanBeNull] string action, LogLevel logLevel = LogLevel.WARNING)
        {
            stopwatch.PrintLogLevel(logLevel, stopwatch.GetElapsedText(action));
            stopwatch.Restart();
        }

        public static void PrintElapsedTrace([NotNull] this Stopwatch stopwatch, [CanBeNull] string action)
        {
            stopwatch.PrintElapsed(action, LogLevel.TRACE);
        }

        public static void PrintElapsed([NotNull] this Stopwatch stopwatch, [CanBeNull] string action, LogLevel logLevel = LogLevel.WARNING)
        {
            stopwatch.PrintLogLevel(logLevel, stopwatch.GetElapsedText(action));
        }

        [NotNull]
        private static string GetElapsedText([NotNull] this Stopwatch stopwatch, [CanBeNull] string action)
        {
            return $"{action} took (ms): {stopwatch.TotalMilliseconds()}";
        }

        public static double TotalMilliseconds([NotNull] this Stopwatch stopwatch)
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
