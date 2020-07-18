using System;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public static class PipeWriterExtensions
    {
        ///// <summary>
        ///// Write a line of text to the writer.
        ///// </summary>
        ///// <param name="writer">The writer to perform the operation on.</param>
        ///// <param name="text">The text to write to the writer.</param>
        //public static void WriteLine(this PipeWriter writer, string text)
        //{
        //    if (writer == null)
        //    {
        //        throw new ArgumentNullException(nameof(writer));
        //    }

        //    WriteLine(writer, Encoding.ASCII, text);
        //}

        ///// <summary>
        ///// Write a line of text to the writer.
        ///// </summary>
        ///// <param name="writer">The writer to perform the operation on.</param>
        ///// <param name="encoding">The encoding to use for the text.</param>
        ///// <param name="text">The text to write to the writer.</param>
        //static unsafe void WriteLine(this PipeWriter writer, Encoding encoding, string text)
        //{
        //    if (writer == null)
        //    {
        //        throw new ArgumentNullException(nameof(writer));
        //    }

        //    fixed (char* ptr = text)
        //    {
        //        var count = encoding.GetByteCount(ptr, text.Length);

        //        fixed (byte* b = writer.GetSpan(count + 2))
        //        {
        //            encoding.GetBytes(ptr, text.Length, b, count);

        //            b[count + 0] = 13;
        //            b[count + 1] = 10;
        //        }

        //        writer.Advance(count + 2);
        //    }
        //}

        ///// <summary>
        ///// Writes a line to the pipeline.
        ///// </summary>
        ///// <param name="writer">The writer to perform the operation on.</param>
        ///// <param name="text">The text to write to the client stream.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>A task which asynchronously performs the operation.</returns>
        //public static Task WriteLineAsync(this PipeWriter writer, string text, CancellationToken cancellationToken = default)
        //{
        //    if (writer == null)
        //    {
        //        throw new ArgumentNullException(nameof(writer));
        //    }

        //    return WriteLineAsync(writer, Encoding.ASCII, text, cancellationToken);
        //}

        //static unsafe Task WriteLineAsync(this PipeWriter writer, Encoding encoding, string text, CancellationToken cancellationToken = default)
        //{
        //    if (writer == null)
        //    {
        //        throw new ArgumentNullException(nameof(writer));
        //    }

        //    // https://github.com/StackExchange/StackExchange.Redis/blob/f52cba7bbe4f22a47a9a7d9c84c9f2824465cae7/toys/StackExchange.Redis.Server/RespServer.cs
        //    // https://github.com/StackExchange/StackExchange.Redis/blob/f52cba7bbe4f22a47a9a7d9c84c9f2824465cae7/src/StackExchange.Redis/PhysicalConnection.cs

        //    fixed (char* ptr = text)
        //    {
        //        var count = encoding.GetByteCount(ptr, text.Length);

        //        fixed (byte* b = writer.GetSpan(count + 2))
        //        {
        //            encoding.GetBytes(ptr, text.Length, b, count);
                    
        //            b[count + 0] = 13;
        //            b[count + 1] = 10;
        //        }
                
        //        writer.Advance(count + 2);
        //    }

        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// Write a reply to the client.
        ///// </summary>
        ///// <param name="writer">The writer to perform the operation on.</param>
        ///// <param name="response">The response to write.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>A task which performs the operation.</returns>
        //public static Task ReplyAsync(this PipeWriter writer, SmtpResponse response, CancellationToken cancellationToken)
        //{
        //    if (writer == null)
        //    {
        //        throw new ArgumentNullException(nameof(writer));
        //    }

        //    //await client.WriteLineAsync($"{(int)response.ReplyCode} {response.Message}", cancellationToken).ConfigureAwait(false);
        //    //await client.FlushAsync(cancellationToken).ConfigureAwait(false);

        //    throw new NotImplementedException();
        //}
    }
}