using System;
using System.Collections.Generic;
using System.Linq;
using SmtpServer.Text;

namespace SmtpServer.Mime
{
    public sealed class MimeHeader : IMimeHeader
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the MIME header.</param>
        /// <param name="tokens">The list of tokens that make up the field body.</param>
        public MimeHeader(string name, IReadOnlyList<Token> tokens)
        {
            Name = name;
            Tokens = tokens;
        }

        /// <summary>
        /// Returns a string representation of the header.
        /// </summary>
        /// <returns>The string representation of the header.</returns>
        public override string ToString()
        {
            return $"{Name}:{Value}";
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The list of tokens that make up the field body.
        /// </summary>
        public IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        /// The value for the header.
        /// </summary>
        public string Value
        {
            get { return String.Concat(Enumerable.Select<Token, string>(Tokens, token => token.TextValue)); }
        }
    }
}