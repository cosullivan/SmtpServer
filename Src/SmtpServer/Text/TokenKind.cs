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
        /// A single space character.
        /// </summary>
        Space,

        /// <summary>
        /// -
        /// </summary>
        Hyphen,
        
        /// <summary>
        /// .
        /// </summary>
        Period,

        /// <summary>
        /// [
        /// </summary>
        LeftBracket,

        /// <summary>
        /// ]
        /// </summary>
        RightBracket,

        /// <summary>
        /// :
        /// </summary>
        Colon,

        /// <summary>
        /// >
        /// </summary>
        GreaterThan,

        /// <summary>
        /// <
        /// </summary>
        LessThan = 10,

        /// <summary>
        /// ,
        /// </summary>
        Comma,

        /// <summary>
        /// @
        /// </summary>
        At,

        /// <summary>
        /// "
        /// </summary>
        Quote,

        /// <summary>
        /// =
        /// </summary>
        Equal,

        /// <summary>
        /// /
        /// </summary>
        Slash,

        /// <summary>
        /// \
        /// </summary>
        Backslash,

        /// <summary>
        /// +
        /// </summary>
        Plus,

        /// <summary>
        /// Unknown.
        /// </summary>
        Other,
    }
}