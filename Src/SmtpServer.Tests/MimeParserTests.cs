using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SmtpServer.Mime;
using SmtpServer.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public sealed class MimeParserTests
    {
        static MimeParser CreateParser(string text)
        {
            var tokens = new StringTokenReader2(text).ToList();

            return new MimeParser(new TokenEnumerator2(tokens));
        }

        [Fact]
        public void CanMakeMimeVersion()
        {
            // arrange
            var parser = CreateParser("MIME-Version: 1.0");
            
            // act
            var made = parser.TryMakeMimeVersion(out IMimeHeader header);

            // assert
            Assert.True(made);
            Assert.Equal(1, ((MimeVersion)header).Major);
            Assert.Equal(0, ((MimeVersion)header).Minor);
        }

        [Fact]
        public void CanMakeContentType()
        {
            // arrange
            var parser = CreateParser("Content-Type: text/plain; charset=us-ascii");

            // act
            var made = parser.TryMakeContentType(out IMimeHeader header);

            // assert
            Assert.True(made);
            Assert.Equal("text", ((ContentType)header).MediaType);
            Assert.Equal("plain", ((ContentType)header).MediaSubType);
            Assert.Equal(1, ((ContentType)header).Parameters.Count);
            Assert.Equal("us-ascii", ((ContentType)header).Parameters["charset"]);
        }
    }
}