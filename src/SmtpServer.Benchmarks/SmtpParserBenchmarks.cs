using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class SmtpParserBenchmarks
    {
        static readonly SmtpResponse UnrecognizedCommand = new SmtpResponse(SmtpReplyCode.CommandNotImplemented, "Unrecognized command");

        readonly SmtpParser _parser = new SmtpParser(new SmtpCommandFactory());
        byte[] _buffer;

        [Params(
            "EHLO example.com",
            "MAIL FROM:<sender@example.com> SIZE=12345 SMTPUTF8",
            "RCPT TO:<recipient@example.com>",
            "AUTH PLAIN Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t",
            "HELP",
            "VRFY recipient@example.com",
            "EXPN staff",
            "BDAT 1024 LAST",
            "PROXY TCP4 192.168.1.1 192.168.1.2 1234 16789",
            "ABCDE FGHIJ KLMNO")]
        public string Input { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _buffer = Encoding.UTF8.GetBytes(Input);
        }

        [Benchmark(Baseline = true)]
        public bool LegacySequentialCandidateLoop()
        {
            var sequence = new ReadOnlySequence<byte>(_buffer);

            return LegacyTryMake(ref sequence, out _, out _);
        }

        [Benchmark]
        public bool SinglePassVerbDispatch()
        {
            var sequence = new ReadOnlySequence<byte>(_buffer);

            return _parser.TryMake(ref sequence, out _, out _);
        }

        bool LegacyTryMake(ref ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            return TryMakeEhlo(buffer, out command, out errorResponse)
                || TryMakeHelo(buffer, out command, out errorResponse)
                || TryMakeMail(buffer, out command, out errorResponse)
                || TryMakeRcpt(buffer, out command, out errorResponse)
                || TryMakeData(buffer, out command, out errorResponse)
                || TryMakeQuit(buffer, out command, out errorResponse)
                || TryMakeRset(buffer, out command, out errorResponse)
                || TryMakeNoop(buffer, out command, out errorResponse)
                || TryMakeStartTls(buffer, out command, out errorResponse)
                || TryMakeAuth(buffer, out command, out errorResponse)
                || TryMakeHelp(buffer, out command, out errorResponse)
                || TryMakeVrfy(buffer, out command, out errorResponse)
                || TryMakeExpn(buffer, out command, out errorResponse)
                || TryMakeBdat(buffer, out command, out errorResponse)
                || TryMakeProxy(buffer, out command, out errorResponse)
                || MakeUnrecognized(out command, out errorResponse);
        }

        bool TryMakeEhlo(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeEhlo(ref reader, out command, out errorResponse);
        }

        bool TryMakeHelo(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeHelo(ref reader, out command, out errorResponse);
        }

        bool TryMakeMail(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeMail(ref reader, out command, out errorResponse);
        }

        bool TryMakeRcpt(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeRcpt(ref reader, out command, out errorResponse);
        }

        bool TryMakeData(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeData(ref reader, out command, out errorResponse);
        }

        bool TryMakeQuit(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeQuit(ref reader, out command, out errorResponse);
        }

        bool TryMakeRset(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeRset(ref reader, out command, out errorResponse);
        }

        bool TryMakeNoop(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeNoop(ref reader, out command, out errorResponse);
        }

        bool TryMakeStartTls(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeStartTls(ref reader, out command, out errorResponse);
        }

        bool TryMakeAuth(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeAuth(ref reader, out command, out errorResponse);
        }

        bool TryMakeHelp(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeHelp(ref reader, out command, out errorResponse);
        }

        bool TryMakeVrfy(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeVrfy(ref reader, out command, out errorResponse);
        }

        bool TryMakeExpn(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeExpn(ref reader, out command, out errorResponse);
        }

        bool TryMakeBdat(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeBdat(ref reader, out command, out errorResponse);
        }

        bool TryMakeProxy(ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            var reader = new TokenReader(buffer);

            return _parser.TryMakeProxy(ref reader, out command, out errorResponse);
        }

        static bool MakeUnrecognized(out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = UnrecognizedCommand;

            return false;
        }
    }
}
