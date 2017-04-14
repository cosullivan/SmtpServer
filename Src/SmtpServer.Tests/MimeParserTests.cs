using SmtpServer.Mime;
using SmtpServer.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public sealed class MimeParserTests
    {
        [Fact]
        public void CanMakeMimeVersion()
        {
            // arrange
            var parser = new MimeParser(new TokenEnumerator(new StringTokenReader("MIME-Version: 1.0")));
            
            // act
            var made = parser.TryMakeMimeVersion(out MimeVersion version);

            // assert
            Assert.True(made);
            Assert.Equal(1.0m, version.Number);
        }
    }
}