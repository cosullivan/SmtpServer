using System;
using System.Text;
using SmtpServer.Protocol;
using Xunit;

namespace SmtpServer.Tests
{
    public class OAuthSaslDecoderTests
    {
        // The SASL field separator (SOH, U+0001).
        const string A = "\u0001";

        static string Encode(string payload)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        }

        [Fact]
        public void XOAuth2_DecodesUserAndBearerToken()
        {
            // arrange — the Google/Microsoft XOAUTH2 form: user={id}^Aauth=Bearer {token}^A^A
            var blob = Encode("user=alice@example.is" + A + "auth=Bearer header.body.signature" + A + A);

            // act
            var result = OAuthSaslDecoder.TryDecodeXOAuth2(blob, out var user, out var token);

            // assert
            Assert.True(result);
            Assert.Equal("alice@example.is", user);
            Assert.Equal("header.body.signature", token);
        }

        [Fact]
        public void OAuthBearer_DecodesAuthzidAndBearerToken()
        {
            // arrange — the RFC 7628 form: n,a={id},^Aauth=Bearer {token}^A^A
            var blob = Encode("n,a=alice@example.is," + A + "auth=Bearer header.body.signature" + A + A);

            // act
            var result = OAuthSaslDecoder.TryDecodeOAuthBearer(blob, out var user, out var token);

            // assert
            Assert.True(result);
            Assert.Equal("alice@example.is", user);
            Assert.Equal("header.body.signature", token);
        }

        [Fact]
        public void OAuthBearer_DecodesAuthzidWithHostAndPortFields()
        {
            // arrange — a fuller GS2 header with host/port fields interleaved before the auth field.
            var blob = Encode("n,a=alice@example.is," + A + "host=mail.example.is" + A + "port=587" + A + "auth=Bearer the.jwt" + A + A);

            // act
            var result = OAuthSaslDecoder.TryDecodeOAuthBearer(blob, out var user, out var token);

            // assert
            Assert.True(result);
            Assert.Equal("alice@example.is", user);
            Assert.Equal("the.jwt", token);
        }

        [Theory]
        [InlineData("not base64 at all")]
        [InlineData("")]
        public void ReturnsFalse_ForInvalidBase64(string blob)
        {
            Assert.False(OAuthSaslDecoder.TryDecodeXOAuth2(blob, out _, out _));
            Assert.False(OAuthSaslDecoder.TryDecodeOAuthBearer(blob, out _, out _));
        }

        [Fact]
        public void XOAuth2_ReturnsFalse_WhenTokenMissing()
        {
            var blob = Encode("user=alice@example.is" + A + A);

            Assert.False(OAuthSaslDecoder.TryDecodeXOAuth2(blob, out _, out _));
        }

        [Fact]
        public void XOAuth2_ReturnsFalse_WhenUserMissing()
        {
            var blob = Encode("auth=Bearer the.jwt" + A + A);

            Assert.False(OAuthSaslDecoder.TryDecodeXOAuth2(blob, out _, out _));
        }

        [Fact]
        public void OAuthBearer_ReturnsFalse_WhenAuthorizationIdentityMissing()
        {
            // "n,," has no a= authzid, so there is no identity to resolve the mailbox with.
            var blob = Encode("n,," + A + "auth=Bearer the.jwt" + A + A);

            Assert.False(OAuthSaslDecoder.TryDecodeOAuthBearer(blob, out _, out _));
        }
    }
}
