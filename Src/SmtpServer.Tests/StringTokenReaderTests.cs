using System.Collections.Generic;
using SmtpServer.Protocol.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public class StringTokenReaderTests
    {
        [Fact]
        public void CanTokenizeWord()
        {
            // arrange
            var tokens = Tokenize("ABC");

            // assert
            Assert.Equal(1, tokens.Count);
            Assert.Equal(TokenKind.Text, tokens[0].Kind);
            Assert.Equal("ABC", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizeNumber()
        {
            // arrange
            var tokens = Tokenize("123");

            // assert
            Assert.Equal(1, tokens.Count);
            Assert.Equal(TokenKind.Number, tokens[0].Kind);
            Assert.Equal("123", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizeAlhpaNumeric()
        {
            // arrange
            var tokens = Tokenize("123abc");

            // assert
            Assert.Equal(1, tokens.Count);
            Assert.Equal(TokenKind.Text, tokens[0].Kind);
            Assert.Equal("123abc", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizeSymbol()
        {
            // arrange
            var tokens = Tokenize("+");

            // assert
            Assert.Equal(1, tokens.Count);
            Assert.Equal(TokenKind.Symbol, tokens[0].Kind);
            Assert.Equal("+", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizePunctuation()
        {
            // arrange
            var tokens = Tokenize(".");

            // assert
            Assert.Equal(1, tokens.Count);
            Assert.Equal(TokenKind.Punctuation, tokens[0].Kind);
            Assert.Equal(".", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizeSpace()
        {
            // arrange
            var tokens = Tokenize(" ");

            // assert
            Assert.Equal(1, tokens.Count);
            Assert.Equal(TokenKind.Space, tokens[0].Kind);
        }

        [Fact]
        public void CanTokenizeSentence()
        {
            // arrange
            var tokens = Tokenize("The time has come");

            // assert
            Assert.Equal(7, tokens.Count);
            Assert.Equal(TokenKind.Text, tokens[0].Kind);
            Assert.Equal(TokenKind.Space, tokens[1].Kind);
            Assert.Equal(TokenKind.Text, tokens[2].Kind);
            Assert.Equal(TokenKind.Space, tokens[3].Kind);
            Assert.Equal(TokenKind.Text, tokens[4].Kind);
            Assert.Equal(TokenKind.Space, tokens[5].Kind);
            Assert.Equal(TokenKind.Text, tokens[6].Kind);
            Assert.Equal("The", tokens[0].Text);
            Assert.Equal("time", tokens[2].Text);
            Assert.Equal("has", tokens[4].Text);
            Assert.Equal("come", tokens[6].Text);
        }

        static IReadOnlyList<Token> Tokenize(string input)
        {
            return Tokenize(new StringTokenReader(input));
        }

        static IReadOnlyList<Token> Tokenize(TokenReader tokenReader)
        {
            var tokens = new List<Token>();

            var token = tokenReader.NextToken();
            while (token != Token.None)
            {
                tokens.Add(token);

                token = tokenReader.NextToken();
            }

            return tokens;
        }
    }
}
