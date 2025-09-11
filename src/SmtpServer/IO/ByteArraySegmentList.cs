using System.Buffers;

namespace SmtpServer.IO
{
    /// <summary>
    /// Byte Array Segment List
    /// </summary>
    internal sealed class ByteArraySegmentList
    {
        internal void Append(byte[] buffer)
        {
            var sequence = new ReadOnlySequence<byte>(buffer);

            Append(ref sequence);
        }

        internal void Append(ref ReadOnlySequence<byte> sequence)
        {
            var position = sequence.GetPosition(0);

            while (sequence.TryGet(ref position, out var memory))
            {
                if (Start == null)
                {
                    Start = new ByteArraySegment(memory);
                    End = Start;
                }
                else
                {
                    End = End.Append(memory);
                }
            }
        }

        internal ReadOnlySequence<byte> Build()
        {
            return new ReadOnlySequence<byte>(Start, 0, End, End.Memory.Length);
        }
        
        internal ByteArraySegment Start { get; private set; }

        internal ByteArraySegment End { get; private set; }
    }
}
