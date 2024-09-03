using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SmtpServer.IO
{
    /// <summary>
    /// Buffers Extension
    /// </summary>
    public static class BuffersExtension
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
        public static bool TryFind(this ReadOnlySequence<byte> source, ReadOnlySpan<byte> sequence, ref SequencePosition head, out SequencePosition tail)
        {
            tail = default;

            // move to the first span
            var position = head;

            if (TryMoveNext(ref source, ref position, out var span) == false)
            {
                return false;
            }

            var index = span.IndexOf(sequence);

            if (index != -1)
            {
                head = source.GetPosition(index, head);
                tail = source.GetPosition(sequence.Length, head);

                return true;
            }

            if (source.IsSingleSegment)
            {
                // nothing else can be done here
                return false;
            }

            while (true)
            {
                tail = position;

                // move to the next span
                if (TryMoveNext(ref source, ref position, out var next) == false)
                {
                    return false;
                }

                if (TryMatchAcrossBoundary(ref span, ref next, ref sequence, out index))
                {
                    head = source.GetPosition(index, head);
                    tail = source.GetPosition(sequence.Length - (span.Length - index), tail);

                    return true;
                }

                span = next;
                head = tail;

                index = span.IndexOf(sequence);

                if (index != -1)
                {
                    head = source.GetPosition(index, head);
                    tail = source.GetPosition(sequence.Length, head);

                    return true;
                }
            }
        }

        static bool TryMatchAcrossBoundary(ref ReadOnlySpan<byte> previous, ref ReadOnlySpan<byte> next, ref ReadOnlySpan<byte> sequence, out int index)
        {
            // we will only call this if a complete match in the previous span isnt found 
            // so we only need to start matching from one byte short of the full sequence
            var partial = sequence.Slice(0, sequence.Length - 1);

            if (TryMatchEnd(ref previous, ref partial, out index))
            {
                partial = sequence.Slice(index);

                if (next.StartsWith(partial))
                {
                    // adjust the index to the position it was found in the previous span
                    index = previous.Length - index;
                    return true;
                }
            }

            return false;
        }

        static bool TryMatchEnd(ref ReadOnlySpan<byte> span, ref ReadOnlySpan<byte> sequence, out int index)
        {
            var partial = sequence;

            while (partial.Length > 0)
            {
                if (span.EndsWith(partial))
                {
                    index = partial.Length;
                    return true;
                }

                partial = partial.Slice(0, partial.Length - 1);
            }

            index = default;
            return false;
        }

        static bool TryMoveNext(ref ReadOnlySequence<byte> source, ref SequencePosition position, out ReadOnlySpan<byte> span)
        {
            while (source.TryGet(ref position, out var memory, advance: true))
            {
                if (memory.Length > 0)
                {
                    span = memory.Span;
                    return true;
                }
            }

            span = default;
            return false;
        }

        internal static bool CaseInsensitiveStringEquals(this ReadOnlySequence<byte> buffer, ref Span<char> text)
        {
            if (buffer.IsSingleSegment)
            {
                var span = buffer.First.Span;

                return text.Length == span.Length && CaseInsensitiveStringEquals(ref span, ref text, 0);
            }

            var i = 0;
            var position = buffer.GetPosition(0);

            while (buffer.TryGet(ref position, out var memory, advance: true))
            {
                var span = memory.Span;

                if (CaseInsensitiveStringEquals(ref span, ref text, i) == false)
                {
                    return false;
                }

                i += span.Length;
            }

            return i == text.Length;
        }

        static bool CaseInsensitiveStringEquals(ref ReadOnlySpan<byte> span, ref Span<char> text, int offset)
        {
            for (var i = 0; i < span.Length; i++)
            {
                var ch = (char)span[i];

                if (char.ToUpper(ch) != char.ToUpper(text[i + offset]))
                {
                    return false;
                }
            }

            return true;
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
