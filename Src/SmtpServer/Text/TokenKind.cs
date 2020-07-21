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
        /// -
        /// </summary>
        Hyphen = 4,
        
        /// <summary>
        /// .
        /// </summary>
        Period = 5,

        /// <summary>
        /// [
        /// </summary>
        LeftBracket = 6,

        /// <summary>
        /// ]
        /// </summary>
        RightBracket = 7,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other = 99,
    }
}