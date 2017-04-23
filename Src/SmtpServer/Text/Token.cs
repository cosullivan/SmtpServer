using System;
using System.Diagnostics;

namespace SmtpServer.Text
{
    [DebuggerDisplay("[{Kind}] {Text}")]
    public struct Token
    {
        /// <summary>
        /// Represents no token.
        /// </summary>
        public static readonly Token None = new Token(TokenKind.None);

        /// <summary>
        /// Represents a new line token.
        /// </summary>
        public static readonly Token NewLine = new Token(TokenKind.NewLine);

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
        /// Create a token for the given character.
        /// </summary>
        /// <param name="ch">The character to create the token for.</param>
        /// <returns>The token that was created.</returns>
        public static Token Create(char ch)
        {
            return new Token(KindOf(ch), ch);
        }

        /// <summary>
        /// Returns the token kind for the given character.
        /// </summary>
        /// <param name="ch">The character to return the token kind for.</param>
        /// <returns>The token kind for the given character.</returns>
        public static TokenKind KindOf(char ch)
        {
            if (Char.IsLetter(ch))
            {
                return TokenKind.Text;
            }

            if (Char.IsDigit(ch))
            {
                return TokenKind.Number;
            }

            if (Char.IsSymbol(ch))
            {
                return TokenKind.Symbol;
            }

            if (Char.IsPunctuation(ch))
            {
                return TokenKind.Punctuation;
            }

            if (Char.IsWhiteSpace(ch))
            {
                return TokenKind.Space;
            }

            return TokenKind.Other;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">Another object to compare to. </param>
        /// <returns>true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public bool Equals(Token other)
        {
            return Kind == other.Kind && String.Equals(Text, other.Text, StringComparison.OrdinalIgnoreCase);
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
                return ((Text?.GetHashCode() ?? 0) * 397) ^ (int)Kind;
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
        public string Text { get; }

        /// <summary>
        /// Gets the token kind.
        /// </summary>
        public TokenKind Kind { get; }
    }
}