using System;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.IO
{
    /// <summary>
    /// Pipe Writer Extensions
    /// </summary>
    public static class PipeWriterExtensions
    {
        /// <summary>
        /// Write a line of text to the pipe.
        /// </summary>
        /// <param name="writer">The writer to perform the operation on.</param>
        /// <param name="text">The text to write to the writer.</param>
        internal static void WriteLine(this PipeWriter writer, string text)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteLine(writer, Encoding.ASCII, text);
        }

        /// <summary>
        /// Write a line of text to the writer.
        /// </summary>
        /// <param name="writer">The writer to perform the operation on.</param>
        /// <param name="encoding">The encoding to use for the text.</param>
        /// <param name="text">The text to write to the writer.</param>
        static unsafe void WriteLine(this PipeWriter writer, Encoding encoding, string text)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            fixed (char* ptr = text)
            {
                var count = encoding.GetByteCount(ptr, text.Length);

                fixed (byte* b = writer.GetSpan(count + 2))
                {
                    encoding.GetBytes(ptr, text.Length, b, count);

                    b[count + 0] = 13;
                    b[count + 1] = 10;
                }

                writer.Advance(count + 2);
            }
        }

        /// <summary>
        /// Write a reply to the client.
        /// </summary>
        /// <param name="writer">The writer to perform the operation on.</param>
        /// <param name="response">The response to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public static ValueTask<FlushResult> WriteReplyAsync(this PipeWriter writer, SmtpResponse response, CancellationToken cancellationToken)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteLine($"{(int)response.ReplyCode} {response.Message}");

            return writer.FlushAsync(cancellationToken);
        }
    }
}
