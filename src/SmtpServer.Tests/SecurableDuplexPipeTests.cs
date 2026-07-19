using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using Xunit;

namespace SmtpServer.Tests
{
    public class SecurableDuplexPipeTests
    {
        [Fact]
        public async Task CanUseConfiguredNetworkBufferSize()
        {
            // arrange
            var stream = new RecordingReadStream();
            using var pipe = new SecurableDuplexPipe(stream, 4096, () => { });

            // act
            var result = await pipe.Input.ReadAsync();
            pipe.Input.AdvanceTo(result.Buffer.End);

            // assert
            Assert.Equal(4096, stream.LastReadBufferSize);
        }

        sealed class RecordingReadStream : Stream
        {
            public int LastReadBufferSize { get; private set; }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => 0;

            public override long Position
            {
                get => 0;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                LastReadBufferSize = count;
                return 0;
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                LastReadBufferSize = count;
                return Task.FromResult(0);
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                LastReadBufferSize = buffer.Length;
                return new ValueTask<int>(0);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return default;
            }
        }
    }
}
