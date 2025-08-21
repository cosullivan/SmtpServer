using System;
using System.Buffers;
using System.Text;

namespace SmtpServer.Text
{
    internal static class StringUtil
    {
        internal static string Create(ReadOnlySequence<byte> sequence)
        {
            return Create(sequence, Encoding.ASCII);
        }

        internal static string Create(ReadOnlySequence<byte> sequence, Encoding encoding)
        {
            if (sequence.Length == 0)
            {
                return null;
            }

            if (sequence.Length > short.MaxValue)
            {
                return null;
            }

            if (sequence.IsSingleSegment)
            {
                return encoding.GetString(sequence.First.Span);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[(int)sequence.Length];

                var i = 0;
                var position = sequence.GetPosition(0);

                while (sequence.TryGet(ref position, out var memory))
                {
                    var span = memory.Span;
                    for (var j = 0; j < span.Length; i++, j++)
                    {
                        buffer[i] = span[j];
                    }
                }

                return encoding.GetString(buffer);
            }
        }

        internal static string Create(ref ReadOnlySpan<byte> buffer)
        {
            return Create(ref buffer, Encoding.ASCII);
        }

        internal static unsafe string Create(ref ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            fixed (byte* ptr = buffer)
            {
                return encoding.GetString(ptr, buffer.Length);
            }
        }
    }
}
