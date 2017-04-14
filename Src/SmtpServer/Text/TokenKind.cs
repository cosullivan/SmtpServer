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
        Text,

        /// <summary>
        /// A number.
        /// </summary>
        Number,

        /// <summary>
        /// A symbol.
        /// </summary>
        Symbol,

        /// <summary>
        /// A punctuation.
        /// </summary>
        Punctuation,

        /// <summary>
        /// A single space character.
        /// </summary>
        Space,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other,
    }
}
