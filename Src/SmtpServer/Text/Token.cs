using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SmtpServer.Text
{
    /// <summary>
    /// Token
    /// </summary>
    [DebuggerDisplay("[{Kind}] {Text}")]
    public readonly ref struct Token
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="kind">The token kind.</param>
        /// <param name="text">The text that the token represents.</param>
        public Token(TokenKind kind, ReadOnlySpan<byte> text = default)
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
        public static bool IsText(byte value)
        {
            return IsBetween(value, 'a', 'z') || IsBetween(value, 'A', 'Z') || IsUtf8(value);
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is a UTF-8 encoded character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a UTF-8 character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUtf8(byte value)
        {
            return value >= 0x80;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a digit character.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a digit character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumber(byte value)
        {
            return IsBetween(value, '0', '9');
        }

        /// <summary>
        /// Returns a value indicating whether or not the given byte is considered a whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the value is considered a whitespace character, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(byte value)
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
        static bool IsBetween(byte value, char low, char high)
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
        static bool IsBetween(byte value, byte low, byte high)
        {
            return value >= low && value <= high;
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
        /// Returns the Text selection as a string.
        /// </summary>
        /// <returns>The string that was created from the selection.</returns>
        public string ToText()
        {
            var text = Text;

            return StringUtil.Create(ref text);
        }

        /// <summary>
        /// Gets the token kind.
        /// </summary>
        public TokenKind Kind { get; }

        /// <summary>
        /// Returns the text representation of the token.
        /// </summary>
        public ReadOnlySpan<byte> Text { get; }
    }
}
