using System.Buffers;

namespace SmtpServer.IO
{
    internal sealed class ByteArraySegmentList
    {
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
        
        internal ByteArraySegment Start { get; private set; }

        internal ByteArraySegment End { get; private set; }
    }
}