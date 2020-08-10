using System;
using System.Buffers;

namespace SmtpServer.Text
{
    internal static class BufferExtensions
    {
        internal static bool CaseInsensitiveStringEquals(this ReadOnlySequence<byte> buffer, ref Span<char> text)
        {
            if (buffer.IsSingleSegment)
            {
                var span = buffer.First.Span;

                if (text.Length != span.Length)
                {
                    return false;
                }

                for (var i = 0; i < span.Length; i++)
                {
                    var ch = (char)span[i];

                    if (char.ToUpper(ch) != char.ToUpper(text[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            throw new NotImplementedException();
        }

        internal static bool IsHex(this ref ReadOnlySpan<byte> buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                if ((buffer[i] < 'a' || buffer[i] > 'f') && (buffer[i] < 'A' || buffer[i] > 'F'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}