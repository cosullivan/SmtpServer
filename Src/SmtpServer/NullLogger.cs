namespace SmtpServer
{
    internal sealed class NullLogger : ILogger
    {
        /// <summary>
        /// Log a verbose message.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">The message arguments.</param>
        public void LogVerbose(string format, params object[] args) { }
    }
}