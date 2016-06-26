using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer
{
    public interface ITextStream
    {
        /// <summary>
        /// Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation.</returns>
        Task<string> ReadLineAsync();

        /// <summary>
        /// Writes a line of characters asynchronously to the current stream.
        /// </summary>
        /// <param name="text">The text to write to the stream.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        Task WriteLineAsync(string text);

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        Task FlushAsync();

        /// <summary>
        /// Gets the inner stream.
        /// </summary>
        /// <returns>The inner stream.</returns>
        Stream GetInnerStream();

        /// <summary>
        /// Gets a value indicating whether or not the text stream is secure.
        /// </summary>
        bool IsSecure { get; }
    }

    public static class TextStreamExtensions
    {
        /// <summary>
        /// Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <param name="stream">The stream to perform the operation on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous read operation.</returns>
        public static async Task<string> ReadLineAsync(this ITextStream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                return await stream.ReadLineAsync().WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <param name="stream">The stream to perform the operation on.</param>
        /// <param name="timeout">The timeout to apply when reading from the stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous read operation.</returns>
        public static async Task<string> ReadLineAsync(this ITextStream stream, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationToken.Register(cancellationTokenSource.Cancel);

            var task = stream.ReadLineAsync();

            if (task == await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token)))
            {
                cancellationTokenSource.Cancel();

                return await task;
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Writes a line of characters asynchronously to the current stream.
        /// </summary>
        /// <param name="stream">The stream to perform the operation on.</param>
        /// <param name="text">The text to write to the stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task WriteLineAsync(this ITextStream stream, string text, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                await stream.WriteLineAsync(text).WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="stream">The stream to perform the operation on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public static async Task FlushAsync(this ITextStream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                await stream.FlushAsync().WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Reply to the client.
        /// </summary>
        /// <param name="stream">The text stream to perform the operation on.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public static async Task ReplyAsync(this ITextStream stream, SmtpResponse response, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            await stream.WriteLineAsync($"{(int)response.ReplyCode} {response.Message}", cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }
}