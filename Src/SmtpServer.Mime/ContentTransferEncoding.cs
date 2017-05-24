using System;
using System.Collections.Generic;

namespace SmtpServer.Mime
{
    public sealed class ContentTransferEncoding : IMimeHeader
    {
        public static readonly ContentTransferEncoding SevenBit = new ContentTransferEncoding("7Bit");
        public static readonly ContentTransferEncoding EightBit = new ContentTransferEncoding("8Bit");
        public static readonly ContentTransferEncoding Binary = new ContentTransferEncoding("Binary");
        public static readonly ContentTransferEncoding QuotedPrintable = new ContentTransferEncoding("Quoted-Printable");
        public static readonly ContentTransferEncoding Base64 = new ContentTransferEncoding("Base64");

        internal static readonly IDictionary<string, ContentTransferEncoding> KnownEncodings = new Dictionary<string, ContentTransferEncoding>(StringComparer.OrdinalIgnoreCase)
        {
            { SevenBit.Mechanism, SevenBit },
            { EightBit.Mechanism, EightBit },
            { Binary.Mechanism, Binary },
            { QuotedPrintable.Mechanism, QuotedPrintable },
            { Base64.Mechanism, Base64 }
        };

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mechanism">The transfer encoding mechanism.</param>
        public ContentTransferEncoding(string mechanism)
        {
            Mechanism = mechanism;
        }

        /// <summary>
        /// Returns a value indicating the equality of the two objects.
        /// </summary>
        /// <param name="left">The left hand side of the comparisson.</param>
        /// <param name="right">The right hand side of the comparisson.</param>
        /// <returns>true if the left and right side are equal, false if not.</returns>
        public static bool operator ==(ContentTransferEncoding left, ContentTransferEncoding right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return true;
            }

            return ReferenceEquals(left, null) == false && left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating the inequality of the two objects.
        /// </summary>
        /// <param name="left">The left hand side of the comparisson.</param>
        /// <param name="right">The right hand side of the comparisson.</param>
        /// <returns>false if the left and right side are equal, true if not.</returns>
        public static bool operator !=(ContentTransferEncoding left, ContentTransferEncoding right)
        {
            return (left == right) == false;
        }

        /// <summary>
        /// Returns a value indicating whether or not the given instance is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if the given instance is equal to the current instance, false if not.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ContentTransferEncoding;

            return other?.Mechanism.CaseInsensitiveEquals(Mechanism) == true;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Mechanism.GetHashCode();
        }

        /// <summary>
        /// The name of the header.
        /// </summary>
        public string Name => "Content-Transfer-Encoding";

        /// <summary>
        /// The media type.
        /// </summary>
        public string Mechanism { get; }
    }
}