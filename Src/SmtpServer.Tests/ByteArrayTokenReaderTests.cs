using System;
using System.Collections.Generic;
using System.Text;
using SmtpServer.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public class ByteArrayTokenReaderTests
    {
        [Fact]
        public void CanTokenizeWord()
        {
            // arrange
            var tokens = Tokenize("ABC");

            // assert
            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenKind.Text, tokens[0].Kind);
            Assert.Equal(TokenKind.None, tokens[1].Kind);
            Assert.Equal("ABC", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizeNumber()
        {
            // arrange
            var tokens = Tokenize("123");

            // assert
            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenKind.Number, tokens[0].Kind);
            Assert.Equal(TokenKind.None, tokens[1].Kind);
            Assert.Equal("123", tokens[0].Text);
        }

        [Fact]
        public void CanTokenizeAlhpaNumericWithLeadingText()
        {
            // arrange
            var tokens = Tokenize("abc123");

            // assert
            Assert.Equal(3, tokens.Count);
            Assert.Equal(TokenKind.Text, tokens[0].Kind);
            Assert.Equal(TokenKind.Number, tokens[1].Kind);
            Assert.Equal(TokenKind.None, tokens[2].Kind);
            Assert.Equal("abc", tokens[0].Text);
            Assert.Equal("123", tokens[1].Text);
        }

        [Fact]
        public void CanTokenizeAlhpaNumericWithLeadingNumber()
        {
            // arrange
            var tokens = Tokenize("123abc");

            // assert
            Assert.Equal(3, tokens.Count);
            Assert.Equal(TokenKind.Number, tokens[0].Kind);
            Assert.Equal(TokenKind.Text, tokens[1].Kind);
            Assert.Equal(TokenKind.None, tokens[2].Kind);
            Assert.Equal("123", tokens[0].Text);
            Assert.Equal("abc", tokens[1].Text);
        }

        [Fact]
        public void CanTokenizeOther()
        {
            // arrange
            var tokens = Tokenize("+");

            // assert
            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenKind.Other, tokens[0].Kind);
            Assert.Equal(TokenKind.None, tokens[1].Kind);
            Assert.Equal("+", tokens[0].Text);
        }
        
        [Fact]
        public void CanTokenizeSpace()
        {
            // arrange
            var tokens = Tokenize(" ");

            // assert
            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenKind.Space, tokens[0].Kind);
            Assert.Equal(TokenKind.None, tokens[1].Kind);
        }

        [Fact]
        public void CanTokenizeSentence()
        {
            // arrange
            var tokens = Tokenize("The time has come");

            // assert
            Assert.Equal(8, tokens.Count);
            Assert.Equal(TokenKind.Text, tokens[0].Kind);
            Assert.Equal(TokenKind.Space, tokens[1].Kind);
            Assert.Equal(TokenKind.Text, tokens[2].Kind);
            Assert.Equal(TokenKind.Space, tokens[3].Kind);
            Assert.Equal(TokenKind.Text, tokens[4].Kind);
            Assert.Equal(TokenKind.Space, tokens[5].Kind);
            Assert.Equal(TokenKind.Text, tokens[6].Kind);
            Assert.Equal(TokenKind.None, tokens[7].Kind);
            Assert.Equal("The", tokens[0].Text);
            Assert.Equal("time", tokens[2].Text);
            Assert.Equal("has", tokens[4].Text);
            Assert.Equal("come", tokens[6].Text);
        }

        static IReadOnlyList<Token> Tokenize(string input)
        {
            var tokenReader = new ByteArrayTokenReader(new [] { new ArraySegment<byte>(Encoding.ASCII.GetBytes(input)) });

            return tokenReader.ToList();
        }
    }
}
