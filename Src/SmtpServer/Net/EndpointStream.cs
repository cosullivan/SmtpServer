using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Net
{
    internal sealed class EndpointStream : Stream
    {
        readonly TcpClient _tcpClient;
        readonly Stream _stream;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tcpClient">The TCP client to create the stream from.</param>
        internal EndpointStream(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _stream = tcpClient.GetStream();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _stream.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _stream.BeginRead(buffer, offset, count, callback, state);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _stream.BeginWrite(buffer, offset, count, callback, state);

        public override int EndRead(IAsyncResult asyncResult) => _stream.EndRead(asyncResult);

        public override void EndWrite(IAsyncResult asyncResult) => _stream.EndWrite(asyncResult);

        public override void Close() => _stream.Close();

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => _stream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.WriteAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override bool CanRead => _stream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _stream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _stream.CanWrite;

        /// <inheritdoc />
        public override long Length => _stream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        /// <inheritdoc />
        public override int ReadTimeout
        {
            get => _stream.ReadTimeout; 
            set => _stream.ReadTimeout = value;
        }
    }
}