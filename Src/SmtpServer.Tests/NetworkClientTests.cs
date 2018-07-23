using System.Linq;
using System.Text;
using SmtpServer.IO;
using Xunit;

namespace SmtpServer.Tests
{
    public sealed class NetworkClientTests
    {
        static INetworkClient CreateNetworkClient(string text, int bufferLength = 64)
        {
            var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(text));

            return new NetworkClient(stream, bufferLength);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void CanReadLineAndRemoveTrailingCRLF()
        {
            // arrange
            var client = CreateNetworkClient("abcde\r\n");

            // act
            var line = await client.ReadLineAsync(Encoding.ASCII);

            // assert
            Assert.Equal(5, line.Length);
            Assert.Equal("abcde", line);
        }

        [Fact]
        public async void CanReadLineAndEndOfStream()
        {
            // arrange
            var client = CreateNetworkClient("abcde");

            // act
            var line = await client.ReadLineAsync(Encoding.ASCII);

            // assert
            Assert.Equal(5, line.Length);
            Assert.Equal("abcde", line);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void CanReadLinesWithInconsistentCRLF()
        {
            // arrange
            var client = CreateNetworkClient("ab\rcd\ne\r\n");

            // act
            var line = await client.ReadLineAsync(Encoding.ASCII);

            // assert
            Assert.Equal(7, line.Length);
            Assert.Equal("ab\rcd\ne", line);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void CanReadMultipleLines()
        {
            // arrange
            var client = CreateNetworkClient("abcde\r\nfghij\r\nklmno\r\n");

            // act
            var line1 = await client.ReadLineAsync(Encoding.ASCII);
            var line2 = await client.ReadLineAsync(Encoding.ASCII);
            var line3 = await client.ReadLineAsync(Encoding.ASCII);

            // assert
            Assert.Equal("abcde", line1);
            Assert.Equal("fghij", line2);
            Assert.Equal("klmno", line3);
        }

        [Fact]
        public async void CanReadBlockWithDotStuffingRemoved()
        {
            // arrange
            var client = CreateNetworkClient("abcd\r\n..1234\r\n.\r\n", 3);

            // act
            var blocks = await client.ReadDotBlockAsync();

            // assert
            Assert.Equal(11, blocks.Sum(b => b.Count));
            Assert.Equal("abcd\r\n.1234", Encoding.ASCII.GetString(blocks.SelectMany(block => block).ToArray()));
        }
    }
}