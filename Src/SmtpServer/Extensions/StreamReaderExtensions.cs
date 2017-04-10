using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer
{
    public static class StreamReaderExtensions
    {
        /// <summary>
        /// Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="timeout">The timeout to apply when reading from the stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous read operation.</returns>
        public static async Task<string> ReadLineAsync(this StreamReader reader, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (cancellationToken.Register(cancellationTokenSource.Cancel))
                {
                    var task = reader.ReadLineAsync();

                    if (task == await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token)))
                    {
                        cancellationTokenSource.Cancel();

                        return await task;
                    }
                }
            }

            throw new TimeoutException();
        }
    }
}