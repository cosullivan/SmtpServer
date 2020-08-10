using System.Text;
using SmtpServer.IO;
using SmtpServer.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public sealed class TokenReaderTests
    {
        static TokenReader CreateReader(params string[] values)
        {
            var segments = new ByteArraySegmentList();

            foreach (var value in values)
            {
                segments.Append(Encoding.UTF8.GetBytes(value));
            }

            return new TokenReader(segments.Build());
        }

        [Fact]
        public void CanTokenizeWord()
        {
            // arrange
            var reader = CreateReader("ABC");

            // assert
            Assert.Equal(TokenKind.Text, reader.Peek().Kind);
            Assert.Equal("ABC", reader.Take().ToText());
            Assert.Equal(TokenKind.None, reader.Take().Kind);
        }

        [Fact]
        public void CanTokenizeMultiSegmentWord()
        {
            // arrange
            var reader = CreateReader("ABC", "DEF");

            // assert
            Assert.Equal(TokenKind.Text, reader.Peek().Kind);
            Assert.Equal("ABC", reader.Take().ToText());
            Assert.Equal(TokenKind.Text, reader.Peek().Kind);
            Assert.Equal("DEF", reader.Take().ToText());
            Assert.Equal(TokenKind.None, reader.Take().Kind);
        }
    }
}