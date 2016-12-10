using System.IO;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace SmtpServer
{
    public class NetworkTextStream : ITextStream
    {
        readonly Stream _stream;
        readonly StreamReader _reader;
        readonly StreamWriter _writer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream for reading and writing.</param>
        public NetworkTextStream(Stream stream)
        {
            _stream = stream;

            _reader = new StreamReader(stream, Encoding.ASCII);
            _writer = new StreamWriter(stream, Encoding.ASCII);
        }

        /// <summary>
        /// Returns the inner stream.
        /// </summary>
        /// <returns>The inner stream.</returns>
        public Stream GetInnerStream()
        {
            return _stream;
        }

        /// <summary>
        /// Reads a line of characters asynchronously from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation.</returns>
        public Task<string> ReadLineAsync()
        {
            return _reader.ReadLineAsync();
        }

        /// <summary>
        /// Writes a line of characters asynchronously to the current stream.
        /// </summary>
        /// <param name="text">The text to write to the stream.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public Task WriteLineAsync(string text)
        {
            return _writer.WriteLineAsync(text);
        }

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public Task FlushAsync()
        {
            return _writer.FlushAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();

            _stream.Dispose();
        }

        /// <summary>
        /// Gets a value indicating whether or not the text stream is secure.
        /// </summary>
        public bool IsSecure
        {
            get { return _stream is SslStream; }
        }
    }
}