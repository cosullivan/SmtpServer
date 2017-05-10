using System.Text;
using SmtpServer.IO;
using SmtpServer.Mail;
using Xunit;

namespace SmtpServer.Tests
{
    public sealed class MimeMessageSerializerTests
    {
        static INetworkClient CreateNetworkClient(string text, int bufferLength = 64)
        {
            var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(text));

            return new NetworkClient(stream, bufferLength);
        }

        [Fact]
        public async void CanParseMessage()
        {
            // arrange
            var serializer = new MimeMessageSerializer();

            // act
            var document = await serializer.DeserializeDocumentAsync(CreateNetworkClient(Content.CanParseMessage));

            // assert
            Assert.Equal(1, document.Version.Major);
            Assert.Equal(0, document.Version.Minor);
        }

        [Fact]
        public async void CanParseMultiPartMessage()
        {
            // arrange
            var serializer = new MimeMessageSerializer();

            // act
            var document = await serializer.DeserializeDocumentAsync(CreateNetworkClient(Content.CanParseMultiPartMessage));

            // assert
            Assert.Equal(1, document.Version.Major);
            Assert.Equal(0, document.Version.Minor);
        }

        #region Content

        public static class Content
        {
            public static string CanParseMessage = @"MIME-Version: 1.0
Content-Type: text/plain

This is a message with a single part.
";

            public static string CanParseMultiPartMessage = @"MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=frontier

This is a message with multiple parts in MIME format.
--frontier
Content-Type: text/plain

This is the body of the message.
--frontier
Content-Type: application/octet-stream
Content-Transfer-Encoding: base64

PGh0bWw+CiAgPGhlYWQ+CiAgPC9oZWFkPgogIDxib2R5PgogICAgPHA+VGhpcyBpcyB0aGUg
Ym9keSBvZiB0aGUgbWVzc2FnZS48L3A+CiAgPC9ib2R5Pgo8L2h0bWw+Cg==
--frontier--";
        }

        #endregion
    }
}
