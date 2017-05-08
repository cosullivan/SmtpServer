namespace SmtpServer.Text
{
    public enum TokenKind
    {
        /// <summary>
        /// No token has been defined.
        /// </summary>
        None = 1,

        /// <summary>
        /// A text.
        /// </summary>
        Text = 2,

        /// <summary>
        /// A number.
        /// </summary>
        Number = 3,

        /// <summary>
        /// A single space character.
        /// </summary>
        Space = 4,

        /// <summary>
        /// A new line token, defined as CRLF.
        /// </summary>
        NewLine = 5,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other = 6,
    }
}