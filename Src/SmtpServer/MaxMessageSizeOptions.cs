using System;
using System.Collections.Generic;
using System.Text;

namespace SmtpServer
{
    /// <summary>
    /// Represents configuration settings for enforcing a maximum message size in SMTP,
    /// including the size limit in bytes and the behavior when that limit is exceeded.
    /// </summary>
    public class MaxMessageSizeOptions : IMaxMessageSizeOptions
    {
        /// <summary>
        /// Gets or sets the maximum allowed message size in bytes,
        /// as specified by the SMTP SIZE extension (RFC 1870).
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the handling strategy for messages that exceed the maximum allowed size.
        /// </summary>
        public MaxMessageSizeHandling Handling { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxMessageSizeOptions"/> class 
        /// with the specified handling strategy and message size limit.
        /// </summary>
        /// <param name="handling">The strategy for handling messages that exceed the maximum allowed size.</param>
        /// <param name="length">The maximum allowed message size in bytes.</param>
        public MaxMessageSizeOptions(MaxMessageSizeHandling handling, int length)
        {
            Length = length;
            Handling = handling;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxMessageSizeOptions"/> class with default values.
        /// </summary>
        public MaxMessageSizeOptions()
        {

        }
    }
}
