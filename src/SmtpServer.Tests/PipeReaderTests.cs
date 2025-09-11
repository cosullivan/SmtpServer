using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using SmtpServer.IO;
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
    }
}
