using System;
using System.Diagnostics;

namespace SmtpServer
{
    internal static class TraceSwitchExtensions
    {
        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="traceSwitch">The trace switch to log the message with.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogError(this TraceSwitch traceSwitch, string format, params object[] args)
        {
            if (traceSwitch == null)
            {
                throw new ArgumentNullException(nameof(traceSwitch));
            }

            Trace.WriteLineIf(traceSwitch.TraceError, String.Format(format, args));
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="traceSwitch">The trace switch to log the message with.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogWarning(this TraceSwitch traceSwitch, string format, params object[] args)
        {
            if (traceSwitch == null)
            {
                throw new ArgumentNullException(nameof(traceSwitch));
            }

            Trace.WriteLineIf(traceSwitch.TraceError, String.Format(format, args));
        }

        /// <summary>
        /// Log an informational message.
        /// </summary>
        /// <param name="traceSwitch">The trace switch to log the message with.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogInfo(this TraceSwitch traceSwitch, string format, params object[] args)
        {
            if (traceSwitch == null)
            {
                throw new ArgumentNullException(nameof(traceSwitch));
            }

            Trace.WriteLineIf(traceSwitch.TraceInfo, String.Format(format, args));
        }

        /// <summary>
        /// Log a verbose message.
        /// </summary>
        /// <param name="traceSwitch">The trace switch to log the message with.</param>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        internal static void LogVerbose(this TraceSwitch traceSwitch, string format, params object[] args)
        {
            if (traceSwitch == null)
            {
                throw new ArgumentNullException(nameof(traceSwitch));
            }

            Trace.WriteLineIf(traceSwitch.TraceVerbose, String.Format(format, args));
        }
    }
}
