using System;
using System.Buffers;

namespace SmtpServer.IO
{
    internal sealed class ByteArraySegment : ReadOnlySequenceSegment<byte>
    {
        internal ByteArraySegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        internal ByteArraySegment Append(ref ReadOnlySequence<byte> sequence)
        {
            var segment = this;

            var position = sequence.GetPosition(0);
            
            while (sequence.TryGet(ref position, out var memory))
            {
                segment = segment.Append(memory);
            }

            return segment;
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