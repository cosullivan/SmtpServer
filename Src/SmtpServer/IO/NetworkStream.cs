using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    public sealed class NetworkStream : INetworkStream
    {
        Stream _stream;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="bufferReadTimeout">The timeout to apply to each buffer read.</param>
        public NetworkStream(Stream stream, TimeSpan bufferReadTimeout)
        {
            _stream = stream;
            _stream.ReadTimeout = (int)bufferReadTimeout.TotalMilliseconds;
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"></see>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"></see>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult">TResult</paramref> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return _stream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Upgrade to a secure stream.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        public async Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default)
        {
            var stream = new SslStream(_stream, true);

            await stream.AuthenticateAsServerAsync(certificate, false, protocols, true).ConfigureAwait(false);

            _stream = stream;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stream?.Dispose();
        }

        /// <summary>
        /// Returns a value indicating whether or not the current client is secure.
        /// </summary>
        public bool IsSecure => _stream is SslStream;
    }
}