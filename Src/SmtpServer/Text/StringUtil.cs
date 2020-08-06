using System;
using System.Buffers;

namespace SmtpServer.Text
{
    internal static class StringUtil
    {
        internal static unsafe string Create(ReadOnlySequence<byte> sequence)
        {
            Span<char> buffer = stackalloc char[(int)sequence.Length];

            if (sequence.IsSingleSegment)
            {
                var span = sequence.First.Span;

                for (var i = 0; i < span.Length; i++)
                {
                    buffer[i] = (char)span[i];
                }
            }
            else
            {
                var i = 0;
                var position = sequence.GetPosition(0);

                while (sequence.TryGet(ref position, out var memory))
                {
                    var span = memory.Span;
                    for (var j = 0; j < span.Length; i++, j++)
                    {
                        buffer[i] = (char)span[j];
                    }
                }
            }

            fixed (char* ptr = buffer)
            {
                return new string(ptr);
            }
        }
    }
}