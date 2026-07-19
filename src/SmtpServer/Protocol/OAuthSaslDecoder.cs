using System;
using System.Text;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Decodes the SASL initial-response blobs for the bearer-token mechanisms XOAUTH2 and RFC 7628
    /// OAUTHBEARER into the authenticating identity and the bearer token. This mirrors the DevOp IMAP
    /// server's parsing so the two protocol front ends agree on the wire. The server surfaces the token to
    /// <see cref="Authentication.IUserAuthenticator"/> as the password, so a host validates it exactly as it
    /// validates any other credential — the decoded token never leaves this boundary in another shape.
    /// </summary>
    internal static class OAuthSaslDecoder
    {
        // The SASL field separator (SOH, U+0001) both mechanisms delimit their key/value fields with.
        const char Separator = '\u0001';

        /// <summary>
        /// Decodes a base64-encoded XOAUTH2 initial response of the form
        /// <c>user={identity}^Aauth=Bearer {token}^A^A</c>.
        /// </summary>
        /// <param name="base64">The base64-encoded initial response.</param>
        /// <param name="user">The decoded authenticating identity.</param>
        /// <param name="token">The decoded bearer token.</param>
        /// <returns>true if both an identity and a bearer token were present, false otherwise.</returns>
        public static bool TryDecodeXOAuth2(string base64, out string user, out string token)
        {
            user = null;
            token = null;

            if (TryDecodeUtf8(base64, out var payload) == false)
            {
                return false;
            }

            var hasAuthorization = false;
            foreach (var field in payload.Split(Separator))
            {
                if (field.Length == 0)
                {
                    continue;
                }

                if (field.StartsWith("user=", StringComparison.OrdinalIgnoreCase))
                {
                    user = field.Substring("user=".Length);
                }
                else if (field.StartsWith("auth=", StringComparison.OrdinalIgnoreCase))
                {
                    hasAuthorization = true;
                    if (TryParseBearerAuthorization(field.Substring("auth=".Length), out token) == false)
                    {
                        return false;
                    }
                }
            }

            return IsPresent(user) && hasAuthorization && IsPresent(token);
        }

        /// <summary>
        /// Decodes a base64-encoded RFC 7628 OAUTHBEARER initial response of the form
        /// <c>{gs2-header}^A[key=value^A...]auth=Bearer {token}^A^A</c>, where the GS2 header carries the
        /// authorization identity as its <c>a={identity}</c> field.
        /// </summary>
        /// <param name="base64">The base64-encoded initial response.</param>
        /// <param name="user">The decoded authorization identity.</param>
        /// <param name="token">The decoded bearer token.</param>
        /// <returns>true if both an identity and a bearer token were present, false otherwise.</returns>
        public static bool TryDecodeOAuthBearer(string base64, out string user, out string token)
        {
            user = null;
            token = null;

            if (TryDecodeUtf8(base64, out var payload) == false)
            {
                return false;
            }

            var fields = payload.Split(Separator);
            if (fields.Length < 2)
            {
                return false;
            }

            user = ParseGs2Authzid(fields[0]);
            var hasAuthorization = false;
            for (var i = 1; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.Length == 0)
                {
                    continue;
                }

                var separator = field.IndexOf('=');
                if (separator <= 0)
                {
                    return false;
                }

                var key = field.Substring(0, separator);
                var value = field.Substring(separator + 1);
                if (key.Equals("auth", StringComparison.OrdinalIgnoreCase))
                {
                    hasAuthorization = true;
                    if (TryParseBearerAuthorization(value, out token) == false)
                    {
                        return false;
                    }
                }
                else if (key.Equals("user", StringComparison.OrdinalIgnoreCase) && IsPresent(user) == false)
                {
                    user = value;
                }
            }

            return IsPresent(user) && hasAuthorization && IsPresent(token);
        }

        // The authorization field is "Bearer {token}" (the scheme is case-insensitive).
        static bool TryParseBearerAuthorization(string value, out string token)
        {
            token = null;

            const string BearerPrefix = "Bearer ";
            if (value.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            token = value.Substring(BearerPrefix.Length);
            return true;
        }

        // The GS2 header is "{cb-flag},[a={identity}],": read the identity after "a=" up to the next comma.
        static string ParseGs2Authzid(string value)
        {
            const string Prefix = "a=";
            var start = value.IndexOf(Prefix, StringComparison.Ordinal);
            if (start < 0)
            {
                return null;
            }

            start += Prefix.Length;
            var end = value.IndexOf(',', start);
            return end < 0 ? value.Substring(start) : value.Substring(start, end - start);
        }

        static bool IsPresent(string value) => string.IsNullOrEmpty(value) == false;

        static bool TryDecodeUtf8(string base64, out string value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(base64))
            {
                return false;
            }

            try
            {
                value = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
