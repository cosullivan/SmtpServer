using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace SmtpServer.Tests
{
    internal class StreamOutputHelper : Stream
    {
        private readonly MemoryStream _memoryStream;
        private readonly ITestOutputHelper _testOutputHelper;

        public StreamOutputHelper(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _memoryStream = new MemoryStream();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Daten in den MemoryStream schreiben
            _memoryStream.Write(buffer, offset, count);

            // Daten als String in die Konsole schreiben
            var writtenText = Encoding.UTF8.GetString(buffer, offset, count);
            _testOutputHelper.WriteLine(writtenText.Replace("\n", "").Replace("\r", ""));
        }

        // Überschreibung der abstrakten Mitglieder von Stream
        public override void Flush() => _memoryStream.Flush();

        public override long Seek(long offset, SeekOrigin origin) => _memoryStream.Seek(offset, origin);

        public override void SetLength(long value) => _memoryStream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) => _memoryStream.Read(buffer, offset, count);

        public override bool CanRead => _memoryStream.CanRead;
        public override bool CanSeek => _memoryStream.CanSeek;
        public override bool CanWrite => _memoryStream.CanWrite;
        public override long Length => _memoryStream.Length;

        public override long Position
        {
            get => _memoryStream.Position;
            set => _memoryStream.Position = value;
        }
    }
}
