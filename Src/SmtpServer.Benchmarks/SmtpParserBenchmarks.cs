using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SmtpServer.Benchmarks
{
    public class SmtpParserBenchmarks
    {
        static readonly ISmtpServerOptions _smtpServerOptions = new SmtpServerOptionsBuilder().Logger(new NullLogger()).Build();

        readonly ArraySegment<byte> _tryMake16BitHex = Tokenize("A1B2");
        readonly ArraySegment<byte> _tryMakeIPv4AddressLiteral = Tokenize("192.168.0.100");

        readonly ArraySegment<byte>[] _tryMakeIPv6Address = new[]
        {
            Tokenize("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789"),
            Tokenize("2001:DB8::8:800:200C:417A"),
            Tokenize("FF01::101"),
            Tokenize("::1"),
            Tokenize("::"),
            Tokenize("0:0:0:0:0:0:13.1.68.3"),
            Tokenize("0:0:0:0:0:FFFF:129.144.52.38"),
            Tokenize("::13.1.68.3"),
            Tokenize("::FFFF:129.144.52.38")
        };

        static ArraySegment<byte> Tokenize(string text)
        {
            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(text));
        }

        [Benchmark]
        public void TryMake16BitHex()
        {
            var parser = new SmtpParser(_smtpServerOptions, new TokenEnumerator(new ByteArrayTokenReader(new[] { _tryMake16BitHex })));
            parser.TryMake16BitHex(out _);
        }

        [Benchmark]
        public void TryMakeIPv4AddressLiteral()
        {
            var parser = new SmtpParser(_smtpServerOptions, new TokenEnumerator(new ByteArrayTokenReader(new[] { _tryMakeIPv4AddressLiteral })));
            parser.TryMakeIPv4AddressLiteral(out _);
        }

        [Benchmark]
        public void TryMakeIPv6AddressLiteral()
        {
            for (var i = 0; i < _tryMakeIPv6Address.Length; i++)
            {
                var parser = new SmtpParser(_smtpServerOptions, new TokenEnumerator(new ByteArrayTokenReader(new[] { _tryMakeIPv6Address[i] })));
                parser.TryMakeIPv6Address(out _);
            }
        }
    }
}