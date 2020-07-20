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

                for (var i = 0; i < sequence.Length; i++)
                {
                    buffer[i] = (char)span[i];
                }

                fixed (char* ptr = buffer)
                { 
                    return new string(ptr);
                }
            }

            throw new NotImplementedException();
        }
    }
}