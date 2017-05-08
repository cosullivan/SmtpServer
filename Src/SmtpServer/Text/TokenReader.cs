using System;
using System.Collections.Generic;
using System.Linq;

namespace SmtpServer.Text
{
    public abstract class TokenReader
    {
        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <returns>The next token that was read.</returns>
        public abstract Token NextToken();
    }

    public static class TokenReaderExtensions
    {
        /// <summary>
        /// Returns the complete list of tokens from the token reader.
        /// </summary>
        /// <param name="reader">The reader to return the list of tokens from.</param>
        /// <returns>The list of tokens from the token reader.</returns>
        public static IReadOnlyList<Token> ToList(this TokenReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return ToEnumerable(reader).ToList();
        }

        /// <summary>
        /// Returns the complete list of tokens from the token reader.
        /// </summary>
        /// <param name="reader">The reader to return the list of tokens from.</param>
        /// <returns>The list of tokens from the token reader.</returns>
        static IEnumerable<Token> ToEnumerable(TokenReader reader)
        {
            Token token;
            while ((token = reader.NextToken()) != Token.None)
            {
                yield return token;
            }

            yield return token;
        }
    }
}