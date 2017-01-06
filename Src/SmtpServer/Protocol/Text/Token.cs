using System;
using System.Diagnostics;
using System.Globalization;

namespace SmtpServer.Protocol.Text
{
    [DebuggerDisplay("[{Kind}] {Text}")]
    public struct Token
    {
        /// <summary>
        /// Represents no token.
        /// </summary>
        public static readonly Token None = new Token(TokenKind.None);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        Token(TokenKind kind) : this(kind, String.Empty)
        {
            Kind = kind;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="text">The token text.</param>
        public Token(TokenKind kind, string text) : this()
        {
            Text = text;
            Kind = kind;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="ch">The character to create the token from.</param>
        public Token(TokenKind kind, char ch) : this()
        {
            Text = ch.ToString();
            Kind = kind;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">Another object to compare to. </param>
        /// <returns>true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public bool Equals(Token other)
        {
            return String.Equals(Text, other.Text, StringComparison.OrdinalIgnoreCase) && Kind == other.Kind;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to. </param>
        /// <returns>true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
            {
                return false;
            }

            return obj is Token && Equals((Token)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Text != null ? Text.GetHashCode() : 0) * 397) ^ (int)Kind;
            }
        }

        /// <summary>
        /// Returns a value indicating the equality of the two objects.
        /// </summary>
        /// <param name="left">The left hand side of the comparisson.</param>
        /// <param name="right">The right hand side of the comparisson.</param>
        /// <returns>true if the left and right side are equal, false if not.</returns>
        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating the inequality of the two objects.
        /// </summary>
        /// <param name="left">The left hand side of the comparisson.</param>
        /// <param name="right">The right hand side of the comparisson.</param>
        /// <returns>false if the left and right side are equal, true if not.</returns>
        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the string representation of the token.
        /// </summary>
        /// <returns>The string representation of the token.</returns>
        public override string ToString()
        {
            return String.Format("[{0}] {1}", Kind, Text);
        }

        /// <summary>
        /// Gets the token text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets the token kind.
        /// </summary>
        public TokenKind Kind { get; private set; }
    }
}
