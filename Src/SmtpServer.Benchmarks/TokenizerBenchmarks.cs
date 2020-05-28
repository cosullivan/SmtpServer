using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using SmtpServer.Text;

namespace SmtpServer.Benchmarks
{
    public class TokenizerBenchmarks
    {
        static readonly IReadOnlyList<ArraySegment<byte>> Segments = Tokenize("The time has come for all good men\nto go to the aid of their country", "1 23 456 7890", "!@#$%^&*()");

        [Benchmark]
        public void EnumerateTokens()
        {
            var tokenizer = new TokenEnumerator(new ByteArrayTokenReader(Segments));
            
            while (tokenizer.Peek() != Token.None)
            {
                tokenizer.Take();
            }
        }

        static IReadOnlyList<ArraySegment<byte>> Tokenize(params string[] text)
        {
            return text.Select(t => new ArraySegment<byte>(Encoding.ASCII.GetBytes(t))).ToList();
        }
    }
}