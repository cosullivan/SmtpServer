using System;
using System.Buffers;

namespace SmtpServer.IO
{
    /// <summary>
    /// Byte Array Segment
    /// </summary>
    internal sealed class ByteArraySegment : ReadOnlySequenceSegment<byte>
    {
        internal ByteArraySegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        internal ByteArraySegment Append(ReadOnlyMemory<byte> memory)
        {
            var segment = new ByteArraySegment(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;

            return segment;
        }
    }
}
