//using System;
//using System.Buffers;
//using System.IO.Pipelines;
//using System.Security.Authentication;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using SmtpServer.Protocol;

//namespace SmtpServer.IO
//{
//    public interface INetworkPipe : IDuplexPipe, IDisposable
//    {
//        /// <summary>
//        /// Upgrade to a secure pipeline.
//        /// </summary>
//        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
//        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
//        /// <param name="cancellationToken">The cancellation token.</param>
//        /// <returns>A task that asynchronously performs the operation.</returns>
//        Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default);

//        /// <summary>
//        /// Returns a value indicating whether or not the current pipeline is secure.
//        /// </summary>
//        bool IsSecure { get; }
//    }


//    //public static class NetworkPipeExtensions
//    //{
//    //    //public static async Task<T> ReadLineAsync<T>(this INetworkPipe pipe, Func<ReadOnlySequence<byte>, T> func, CancellationToken cancellationToken)
//    //    //{
//    //    //    if (pipe == null)
//    //    //    {
//    //    //        throw new ArgumentNullException(nameof(pipe));
//    //    //    }

//    //    //    HERE: probably still INetworkClient but it has a Pipe? or an INetworkPipe instead? or ISecureDuplexPipe??

//    //    //    while (true)
//    //    //    {
//    //    //        var read = await pipe.Input.ReadAsync(cancellationToken);
                
//    //    //        if (read.IsCanceled)
//    //    //        {
//    //    //            // TODO: throw exception here
//    //    //            throw new Exception("what type??");
//    //    //        }

//    //    //        var buffer = read.Buffer;

//    //    //        if (TryReadLine(buffer, out var position))
//    //    //        {
//    //    //            // ReSharper disable once PossibleInvalidOperationException
//    //    //            var result = func(buffer.Slice(buffer.Start, position.Value));

//    //    //            pipe.Input.AdvanceTo(position.Value);

//    //    //            return result;
//    //    //        }

//    //    //        pipe.Input.AdvanceTo(buffer.Start, buffer.End);

//    //    //        if (read.IsCompleted)
//    //    //        {
//    //    //            // TODO: throw exception here
//    //    //            throw new Exception("what type??");
//    //    //        }
//    //    //    }

//    //    //    static bool TryReadLine(ReadOnlySequence<byte> buffer, out SequencePosition? position)
//    //    //    {
//    //    //        // ReSharper disable once InconsistentNaming
//    //    //        const byte CR = 13;
//    //    //        // ReSharper disable once InconsistentNaming
//    //    //        const byte LF = 10;

//    //    //        position = buffer.Find(new[] { CR, LF });

//    //    //        return position != null;
//    //    //    }
//    //    //}

//    //    /// <summary>
//    //    /// Write a line of text to the pipe.
//    //    /// </summary>
//    //    /// <param name="pipe">The pipe to perform the operation on.</param>
//    //    /// <param name="text">The text to write to the writer.</param>
//    //    public static void WriteLine(this INetworkPipe pipe, string text)
//    //    {
//    //        if (pipe == null)
//    //        {
//    //            throw new ArgumentNullException(nameof(pipe));
//    //        }

//    //        WriteLine(pipe, Encoding.ASCII, text);
//    //    }

//    //    /// <summary>
//    //    /// Write a line of text to the writer.
//    //    /// </summary>
//    //    /// <param name="pipe">The pipe to perform the operation on.</param>
//    //    /// <param name="encoding">The encoding to use for the text.</param>
//    //    /// <param name="text">The text to write to the writer.</param>
//    //    static unsafe void WriteLine(this INetworkPipe pipe, Encoding encoding, string text)
//    //    {
//    //        if (pipe == null)
//    //        {
//    //            throw new ArgumentNullException(nameof(pipe));
//    //        }

//    //        fixed (char* ptr = text)
//    //        {
//    //            var count = encoding.GetByteCount(ptr, text.Length);

//    //            fixed (byte* b = pipe.Output.GetSpan(count + 2))
//    //            {
//    //                encoding.GetBytes(ptr, text.Length, b, count);

//    //                b[count + 0] = 13;
//    //                b[count + 1] = 10;
//    //            }

//    //            pipe.Output.Advance(count + 2);
//    //        }
//    //    }

//    //    /// <summary>
//    //    /// Flush the output of the pipe.
//    //    /// </summary>
//    //    /// <param name="pipe">The pipe to perform the operation on.</param>
//    //    /// <param name="cancellationToken">The cancellation token.</param>
//    //    /// <returns>A value indicating whether any more data should be written.</returns>
//    //    public static async ValueTask<bool> FlushAsync(this INetworkPipe pipe, CancellationToken cancellationToken = default)
//    //    {
//    //        var flush = await pipe.Output.FlushAsync(cancellationToken);

//    //        return flush.IsCanceled == false && flush.IsCompleted == false;
//    //    }

//    //    /// <summary>
//    //    /// Write a reply to the client.
//    //    /// </summary>
//    //    /// <param name="pipe">The pipe to perform the operation on.</param>
//    //    /// <param name="response">The response to write.</param>
//    //    /// <param name="cancellationToken">The cancellation token.</param>
//    //    /// <returns>A task which performs the operation.</returns>
//    //    public static ValueTask<bool> ReplyAsync(this INetworkPipe pipe, SmtpResponse response, CancellationToken cancellationToken)
//    //    {
//    //        if (pipe == null)
//    //        {
//    //            throw new ArgumentNullException(nameof(pipe));
//    //        }

//    //        pipe.WriteLine($"{(int)response.ReplyCode} {response.Message}");

//    //        return pipe.FlushAsync(cancellationToken);
//    //    }
//    //}
//}