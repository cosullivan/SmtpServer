using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SmtpServer.IO
{
    internal static class BuffersExtension
    {
        /// <summary>
        /// Try to find the first occurrance of a sequence in the given buffer.
        /// </summary>
        /// <param name="source">The source to find the sequence in.</param>
        /// <param name="sequence">The sequence to find in the source.</param>
        /// <param name="head">The position that the sequence was found.</param>
        /// <param name="tail">The position that the sequence ended.</param>
        /// <returns>Returns true if the sequence could be found, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryFind(this ReadOnlySequence<byte> source, ReadOnlySpan<byte> sequence, out SequencePosition head, out SequencePosition tail)
        {
            head = default;
            tail = default;

            if (source.IsSingleSegment)
            {
                var index = source.First.Span.IndexOf(sequence);

                if (index != -1)
                {
                    head = source.GetPosition(index);
                    tail = source.GetPosition(index + sequence.Length);

                    return true;
                }

                return false;
            }

            throw new NotImplementedException("TODO: need to implement for multiple segments");
        }
    }
}