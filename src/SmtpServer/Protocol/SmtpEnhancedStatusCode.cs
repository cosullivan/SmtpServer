using System;

namespace SmtpServer.Protocol
{
    /// <summary>
    /// Enhanced SMTP status code as defined by RFC 3463.
    /// </summary>
    public readonly struct SmtpEnhancedStatusCode : IEquatable<SmtpEnhancedStatusCode>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="class">The status class.</param>
        /// <param name="subject">The status subject.</param>
        /// <param name="detail">The status detail.</param>
        public SmtpEnhancedStatusCode(int @class, int subject, int detail)
        {
            if (@class < 2 || @class > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(@class));
            }

            if (subject < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(subject));
            }

            if (detail < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(detail));
            }

            Class = @class;
            Subject = subject;
            Detail = detail;
        }

        /// <summary>
        /// Gets the status class.
        /// </summary>
        public int Class { get; }

        /// <summary>
        /// Gets the status subject.
        /// </summary>
        public int Subject { get; }

        /// <summary>
        /// Gets the status detail.
        /// </summary>
        public int Detail { get; }

        /// <inheritdoc />
        public bool Equals(SmtpEnhancedStatusCode other)
        {
            return Class == other.Class && Subject == other.Subject && Detail == other.Detail;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SmtpEnhancedStatusCode other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Class, Subject, Detail);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Class}.{Subject}.{Detail}";
        }
    }
}
