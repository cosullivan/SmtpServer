namespace SmtpServer.Text
{
    public enum TokenKind
    {
        /// <summary>
        /// No token has been defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// A text.
        /// </summary>
        Text = 1,

        /// <summary>
        /// A number.
        /// </summary>
        Number = 2,

        /// <summary>
        /// A single space character.
        /// </summary>
        Space = 3,

        /// <summary>
        /// A new line token, defined as CRLF.
        /// </summary>
        NewLine = 4,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other = 5,
    }
}