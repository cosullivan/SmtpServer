using System;
using System.IO;
using System.IO.Pipelines;

namespace SmtpServer.IO
{
    public sealed class NetworkPipe : INetworkPipe
    {
        readonly Action _disposeAction;
        Stream _stream;
        bool _disposed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream that the pipe is reading and writing to.</param>
        /// <param name="disposeAction">The action to execute when the stream has been disposed.</param>
        public NetworkPipe(Stream stream, Action disposeAction)
        {
            _stream = stream;
            _disposeAction = disposeAction;

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
        public PipeReader Input { get; }

        /// <summary>
        /// Gets the <see cref="T:System.IO.Pipelines.PipeWriter" /> half of the duplex pipe.
        /// </summary>
        public PipeWriter Output { get; }
    }
}