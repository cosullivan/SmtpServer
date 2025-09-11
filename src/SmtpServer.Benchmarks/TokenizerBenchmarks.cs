using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using SmtpServer.Text;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    public class TokenizerBenchmarks
    {
        static readonly IReadOnlyList<ArraySegment<byte>> Segments = Tokenize("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789");

        //[Benchmark]
        //public void EnumerateTokens()
        //{
        //    var tokenizer = new TokenEnumerator(new ByteArrayTokenReader(Segments));
            
        //    while (tokenizer.Peek() != Token.None)
        //    {
        //        tokenizer.Take();
        //    }
        //}

        static IReadOnlyList<ArraySegment<byte>> Tokenize(params string[] text)
        {
            return text.Select(t => new ArraySegment<byte>(Encoding.ASCII.GetBytes(t))).ToList();
        }
    }
}
