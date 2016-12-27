using System;
using System.Diagnostics;

namespace SmtpServer.Benchmark
{
    public sealed class ConsoleTimer : IDisposable
    {
        readonly string _message;
        readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public ConsoleTimer(string message = "Time Taken")
        {
            _message = message;
            _stopwatch.Start();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();

            Console.WriteLine("{0} {1}ms", _message, _stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// The time taken for the operation.
        /// </summary>
        public long TimeTaken
        {
            get { return _stopwatch.ElapsedMilliseconds; }
        }
    }
}