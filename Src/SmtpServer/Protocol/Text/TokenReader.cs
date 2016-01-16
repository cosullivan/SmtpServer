namespace SmtpServer.Protocol.Text
{
    public abstract class TokenReader
    {
        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <returns>The next token that was read.</returns>
        public abstract Token NextToken();
    }
}
