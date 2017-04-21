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
        /// A symbol.
        /// </summary>
        Symbol = 4,

        /// <summary>
        /// A punctuation.
        /// </summary>
        Punctuation = 5,

        /// <summary>
        /// A single space character.
        /// </summary>
        Space = 6,

        /// <summary>
        /// A new line token, defined as CRLF.
        /// </summary>
        NewLine = 7,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other = 8,
    }
}