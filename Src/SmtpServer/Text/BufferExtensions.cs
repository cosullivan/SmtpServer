using System;
using System.Buffers;
using System.Text;

namespace SmtpServer.Text
{
    internal static class BufferExtensions
    {
        internal static bool CaseInsensitiveStringEquals(this ReadOnlySequence<byte> buffer, ref Span<char> text)
        {
            //return string.Equals(Encoding.ASCII.GetString(buffer), value, StringComparison.OrdinalIgnoreCase);

            var index = 0;
            foreach (var segment in buffer)
            {
                var span = segment.Span;

                if (span.Length == 0)
                {
                    continue;
                }

                for (var i = 0; i < span.Length; i++, index++)
                {
                }
            }

            return true;
        }
    }
}