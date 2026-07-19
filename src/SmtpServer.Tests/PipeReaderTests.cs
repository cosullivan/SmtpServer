using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;
using SmtpServer.Protocol;
using SmtpServer.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public sealed class PipeReaderExtensionTests
    {
        static PipeReader CreatePipeReader(string text)
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));

            return PipeReader.Create(stream);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task CanReadLineAndRemoveTrailingCRLF()
        {
            // arrange
            var reader = CreatePipeReader("abcde\r\n");

            var maxMessageSizeOptions = new MaxMessageSizeOptions();

            // act
            var line = await reader.ReadLineAsync(Encoding.ASCII, maxMessageSizeOptions).ConfigureAwait(false);

            // assert
            Assert.Equal(5, line.Length);
            Assert.Equal("abcde", line);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task CanReadLinesWithInconsistentCRLF()
        {
            // arrange
            var reader = CreatePipeReader("ab\rcd\ne\r\n");

            var maxMessageSizeOptions = new MaxMessageSizeOptions();

            // act
            var line = await reader.ReadLineAsync(Encoding.ASCII, maxMessageSizeOptions).ConfigureAwait(false);

            // assert
            Assert.Equal(7, line.Length);
            Assert.Equal("ab\rcd\ne", line);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task CanReadMultipleLines()
        {
            // arrange
            var reader = CreatePipeReader("abcde\r\nfghij\r\nklmno\r\n");

            var maxMessageSizeOptions = new MaxMessageSizeOptions();

            // act
            var line1 = await reader.ReadLineAsync(Encoding.ASCII, maxMessageSizeOptions).ConfigureAwait(false);
            var line2 = await reader.ReadLineAsync(Encoding.ASCII, maxMessageSizeOptions).ConfigureAwait(false);
            var line3 = await reader.ReadLineAsync(Encoding.ASCII, maxMessageSizeOptions).ConfigureAwait(false);

            // assert
            Assert.Equal("abcde", line1);
            Assert.Equal("fghij", line2);
            Assert.Equal("klmno", line3);
        }

        [Fact]
        public async Task CanEnforceMaxCommandLineLength()
        {
            // arrange
            var reader = CreatePipeReader("abcdef\r\n");

            // act
            var exception = await Assert.ThrowsAsync<SmtpResponseException>(
                async () => await reader.ReadLineAsync(Encoding.ASCII, 5));

            // assert
            Assert.True(exception.IsQuitRequested);
            Assert.Equal(SmtpReplyCode.SyntaxError, exception.Response.ReplyCode);
        }

        [Fact]
        public async Task CanReadLineAtMaxCommandLineLength()
        {
            // arrange
            var reader = CreatePipeReader("abcde\r\n");

            // act
            var line = await reader.ReadLineAsync(Encoding.ASCII, 5);

            // assert
            Assert.Equal("abcde", line);
        }

        [Fact]
        public async Task CanWriteEnhancedStatusCodeReply()
        {
            // arrange
            var pipe = new Pipe();

            // act
            await pipe.Writer.WriteReplyAsync(SmtpResponse.AuthenticationFailed, CancellationToken.None);
            pipe.Writer.Complete();

            // assert
            Assert.Equal("535 5.7.8 authentication failed\r\n", await ReadAllAsync(pipe.Reader));
        }

        [Fact]
        public async Task CanWriteAuthContinuationWithoutEnhancedStatusCode()
        {
            // arrange
            var pipe = new Pipe();

            // act
            await pipe.Writer.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.ContinueWithAuth, "VXNlcm5hbWU6"), CancellationToken.None);
            pipe.Writer.Complete();

            // assert
            Assert.Equal("334 VXNlcm5hbWU6\r\n", await ReadAllAsync(pipe.Reader));
        }

        [Fact]
        public async Task CanReadBlockWithDotStuffingRemoved()
        {
            // arrange
            var reader = CreatePipeReader("abcd\r\n..1234\r\n.\r\n");

            var maxMessageSizeOptions = new MaxMessageSizeOptions();

            // act
            var text = "";
            await reader.ReadDotBlockAsync(
                buffer => 
                {
                    text = StringUtil.Create(buffer);

                    return Task.CompletedTask;
                },
                maxMessageSizeOptions);

            // assert
            Assert.Equal("abcd\r\n.1234", text);
        }

        [Fact]
        public async Task CanStreamBlockWithDotStuffingRemoved()
        {
            // arrange
            var reader = CreatePipeReader("abcd\r\n..1234\r\n.\r\n");
            var writer = new Pipe();

            var maxMessageSizeOptions = new MaxMessageSizeOptions();

            // act
            await reader.ReadDotBlockAsync(writer.Writer, maxMessageSizeOptions);
            var text = await ReadAllAsync(writer.Reader);

            // assert
            Assert.Equal("abcd\r\n.1234", text);
        }

        [Fact]
        public async Task CanEnforceMaxMessageSizeWhenStreamingBlock()
        {
            // arrange
            var reader = CreatePipeReader("abcd\r\n1234\r\n.\r\n");
            var writer = new Pipe();

            var maxMessageSizeOptions = new MaxMessageSizeOptions(MaxMessageSizeHandling.Strict, 5);

            // act
            var exception = await Assert.ThrowsAsync<SmtpResponseException>(
                async () => await reader.ReadDotBlockAsync(writer.Writer, maxMessageSizeOptions));

            // assert
            Assert.True(exception.IsQuitRequested);
        }

        static async Task<string> ReadAllAsync(PipeReader reader)
        {
            using var stream = new MemoryStream();

            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    stream.Write(segment.Span);
                }

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();

            return Encoding.ASCII.GetString(stream.ToArray());
        }
    }
}
