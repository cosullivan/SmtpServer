using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SmtpServer.Text
{
    [DebuggerDisplay("[{Kind}] {Text}")]
    public readonly ref struct Token
    {
        ///// <summary>
        ///// Constructor.
        ///// </summary>
        ///// <param name="kind">The token kind.</param>
        ///// <param name="ch">The character that the token represents.</param>
        //public Token(TokenKind kind, char ch)
        //{
        //    Kind = kind;
        //    Text = new ReadOnlySequence<char>(new[] { ch });
        //}

        ///// <summary>
        ///// Constructor.
        ///// </summary>
        ///// <param name="kind">The token kind.</param>
        ///// <param name="text">The text that the token represents.</param>
        //public Token(TokenKind kind, string text) : this(kind, new ReadOnlySequence<char>(text.ToCharArray())) { }

        ///// <summary>
        ///// Constructor.
        ///// </summary>
        ///// <param name="kind">The token kind.</param>
        ///// <param name="text">The text that the token represents.</param>
        //public Token(TokenKind kind, ReadOnlySequence<char> text)
        //{
        //    Kind = kind;
        //    Text = text;
        //}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="text">The text that the token represents.</param>
        public Token(TokenKind kind, ReadOnlySpan<char> text)
        {
            Kind = kind;
            Text = text;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a text character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a text character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsText(char value)
        {
            return IsBetween(value, 'a', 'z') || IsBetween(value, 'A', 'Z') || IsUtf8(value);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is a UTF-8 encoded character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a UTF-8 character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUtf8(char value)
        {
            return value >= 0x80;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a digit character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a digit character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumber(char value)
        {
            return IsBetween(value, '0', '9');
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a whitespace character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(char value)
        {
            return value == 32 || IsBetween(value, 9, 13);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given value is inclusively between a given range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="low">The lower value of the range.</param>
        /// <param name="high">The higher value of the range.</param>
        /// <returns>true if the value is between the range, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsBetween(char value, char low, char high)
        {
            return value >= low && value <= high;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given value is inclusively between a given range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="low">The lower value of the range.</param>
        /// <param name="high">The higher value of the range.</param>
        /// <returns>true if the value is between the range, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsBetween(char value, byte low, byte high)
        {
            return value >= low && value <= high;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="other">Another object to compare to. </param>
        /// <returns>true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public bool Equals(Token other)
        {
            //return Kind == other.Kind && Text.ToString().Equals(other.Text.ToString(), StringComparison.OrdinalIgnoreCase);
            return Kind == other.Kind && Text.Equals(other.Text, StringComparison.OrdinalIgnoreCase);
        }

        ///// <summary>
        ///// Indicates whether this instance and a specified object are equal.
        ///// </summary>
        ///// <param name="obj">Another object to compare to. </param>
        ///// <returns>true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. </returns>
        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj))
        //    {
        //        return false;
        //    }

        //    return obj is Token && Equals((Token)obj);
        //}

        ///// <summary>
        ///// Returns the hash code for this instance.
        ///// </summary>
        ///// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        return (Text.GetHashCode() * 397) ^ (int)Kind;
        //    }
        //}

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
            return $"[{Kind}] {Text.ToString()}";
        }

        /// <summary>
        /// Gets the token kind.
        /// </summary>
        public TokenKind Kind { get; }

        /// <summary>
        /// Returns the text representation of the token.
        /// </summary>
        public ReadOnlySpan<char> Text { get; }
    }
}