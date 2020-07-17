using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    public class SmtpParserBenchmarks
    {
        static readonly ISmtpServerOptions _smtpServerOptions = new SmtpServerOptionsBuilder().Logger(new NullLogger()).Build();

        readonly SmtpParser _tryMake16BitHex = new SmtpParser(_smtpServerOptions, Tokenize("A1B2"));
        readonly SmtpParser _tryMakeIPv4AddressLiteral = new SmtpParser(_smtpServerOptions, Tokenize("192.168.0.100"));

        readonly SmtpParser[] _tryMakeIPv6Address = new[]
        {
            new SmtpParser(_smtpServerOptions, Tokenize("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789")),
            new SmtpParser(_smtpServerOptions, Tokenize("2001:DB8::8:800:200C:417A")),
            new SmtpParser(_smtpServerOptions, Tokenize("FF01::101")),
            new SmtpParser(_smtpServerOptions, Tokenize("::1")),
            new SmtpParser(_smtpServerOptions, Tokenize("::")),
            new SmtpParser(_smtpServerOptions, Tokenize("0:0:0:0:0:0:13.1.68.3")),
            new SmtpParser(_smtpServerOptions, Tokenize("0:0:0:0:0:FFFF:129.144.52.38")),
            new SmtpParser(_smtpServerOptions, Tokenize("::13.1.68.3")),
            new SmtpParser(_smtpServerOptions, Tokenize("::FFFF:129.144.52.38"))
        };

        static TokenReader CreateTokenReader(string text)
        {
            return new ByteArrayTokenReader(new[] { new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)) });
        }

        static TokenEnumerator Tokenize(string text)
        {
            return new TokenEnumerator(CreateTokenReader(text));
        }

        [Benchmark]
        public void TryMake16BitHex()
        {
            _tryMake16BitHex.TryMake16BitHex(out _);
        }

        [Benchmark]
        public void TryMakeIPv4AddressLiteral()
        {
            _tryMakeIPv4AddressLiteral.TryMakeIPv4AddressLiteral(out _);
        }

        [Benchmark]
        public void TryMakeIPv6Address()
        {
            for (var i = 0; i < _tryMakeIPv6Address.Length; i++)
            {
                _tryMakeIPv6Address[i].TryMakeIPv6Address(out _);
            }
        }
    }
}