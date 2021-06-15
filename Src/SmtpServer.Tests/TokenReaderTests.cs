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

        [Fact]
        public void CanMakeSequences()
        {
            // arrange
            var reader = CreateReader("1", "AB", "CDE", "F234", "5678");

            // act
            var made1 = reader.TryMake(TryMakeNumber, out var sequence1);
            var made2 = reader.TryMake(TryMakeText, out var sequence2);
            var made3 = reader.TryMake(TryMakeNumber, out var sequence3);

            // assert
            Assert.True(made1);
            Assert.Equal("1", StringUtil.Create(sequence1));
            Assert.True(made2);
            Assert.Equal("ABCDEF", StringUtil.Create(sequence2));
            Assert.True(made3);
            Assert.Equal("2345678", StringUtil.Create(sequence3));

            static bool TryMakeText(ref TokenReader r)
            {
                if (r.Peek().Kind == TokenKind.Text)
                {
                    r.Skip(TokenKind.Text);
                    return true;
                }

                return false;
            }

            static bool TryMakeNumber(ref TokenReader r)
            {
                if (r.Peek().Kind == TokenKind.Number)
                {
                    r.Skip(TokenKind.Number);
                    return true;
                }

                return false;
            }
        }
    }
}