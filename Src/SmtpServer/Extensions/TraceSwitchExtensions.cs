using System;

namespace SmtpServer
{
    internal static class TraceSwitchExtensions
    {
        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogError(string format, params object[] args)
        {
            Console.WriteLine("[ERROR] " + String.Format(format, args));
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogWarning(string format, params object[] args)
        {
            Console.WriteLine("[WARNING] " + String.Format(format, args));
        }

        /// <summary>
        /// Log an informational message.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogInfo(string format, params object[] args)
        {
            Console.WriteLine("[INFO] " + String.Format(format, args));
        }

        /// <summary>
        /// Log a verbose message.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogVerbose(string format, params object[] args)
        {
            Console.WriteLine("[WARNING] " + String.Format(format, args));
        }
    }
}
