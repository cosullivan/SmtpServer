using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.IO
{
    internal sealed class SecurableDuplexPipe : ISecurableDuplexPipe
    {
        readonly Action _disposeAction;
        Stream _stream;
        bool _disposed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream that the pipe is reading and writing to.</param>
        /// <param name="disposeAction">The action to execute when the stream has been disposed.</param>
        internal SecurableDuplexPipe(Stream stream, Action disposeAction)
        {
            _stream = stream;
            _disposeAction = disposeAction;

            Input = PipeReader.Create(_stream);
            Output = PipeWriter.Create(_stream);
        }

        /// <summary>
        /// Upgrade to a secure pipeline.
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
            
            Input = PipeReader.Create(_stream);
            Output = PipeWriter.Create(_stream);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the stream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing)
                {
                    _disposeAction();
                    _stream = null;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Gets the <see cref="T:System.IO.Pipelines.PipeReader" /> half of the duplex pipe.
        /// </summary>
        public PipeReader Input { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:System.IO.Pipelines.PipeWriter" /> half of the duplex pipe.
        /// </summary>
        public PipeWriter Output { get; private set; }

        /// <summary>
        /// Returns a value indicating whether or not the current pipeline is secure.
        /// </summary>
        public bool IsSecure => _stream is SslStream;
    }
}