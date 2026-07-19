namespace SmtpServer
{
    /// <summary>
    /// Defines the SMTP protocol extensions that the server advertises and accepts.
    /// </summary>
    public sealed class SmtpServerExtensionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpServerExtensionOptions"/> class.
        /// </summary>
        public SmtpServerExtensionOptions()
        {
            SmtpUtf8Enabled = true;
            DsnEnabled = true;
            ChunkingEnabled = true;
            OAuthEnabled = false;
        }

        /// <summary>
        /// Enables or disables SMTPUTF8 advertisement and SMTPUTF8-specific envelope handling.
        /// </summary>
        /// <param name="enabled">A value indicating whether SMTPUTF8 is enabled.</param>
        /// <returns>The current options instance.</returns>
        public SmtpServerExtensionOptions SmtpUtf8(bool enabled)
        {
            SmtpUtf8Enabled = enabled;
            return this;
        }

        /// <summary>
        /// Enables or disables DSN advertisement and DSN envelope parameters.
        /// </summary>
        /// <param name="enabled">A value indicating whether DSN is enabled.</param>
        /// <returns>The current options instance.</returns>
        public SmtpServerExtensionOptions Dsn(bool enabled)
        {
            DsnEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Enables or disables CHUNKING advertisement and BDAT command processing.
        /// </summary>
        /// <param name="enabled">A value indicating whether CHUNKING is enabled.</param>
        /// <returns>The current options instance.</returns>
        public SmtpServerExtensionOptions Chunking(bool enabled)
        {
            ChunkingEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Enables or disables advertisement of the XOAUTH2 and OAUTHBEARER bearer-token SASL mechanisms.
        /// Off by default: a host should only advertise them once it has wired a bearer-token authenticator,
        /// otherwise clients would negotiate a mechanism the host cannot honour. The mechanisms are always
        /// parsed and processed when a client sends them; this flag only controls EHLO discovery.
        /// </summary>
        /// <param name="enabled">A value indicating whether the OAuth bearer-token mechanisms are advertised.</param>
        /// <returns>The current options instance.</returns>
        public SmtpServerExtensionOptions OAuth(bool enabled)
        {
            OAuthEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Gets a value indicating whether SMTPUTF8 is enabled.
        /// </summary>
        public bool SmtpUtf8Enabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the XOAUTH2 and OAUTHBEARER bearer-token mechanisms are advertised.
        /// </summary>
        public bool OAuthEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether DSN is enabled.
        /// </summary>
        public bool DsnEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether CHUNKING is enabled.
        /// </summary>
        public bool ChunkingEnabled { get; private set; }
    }
}
