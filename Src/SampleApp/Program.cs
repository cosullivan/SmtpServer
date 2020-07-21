using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using SampleApp.Examples;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;

            //SimpleExample.Run();

            //var text = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("EHLO abc-1-def.mail.com"));
            var text = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("EHLO [127.0.0.1]"));

            var reader = new TokenReader(text);
            //while (reader.Peek() != default)
            //{
            //    var token = reader.Take();

            //    Console.WriteLine("[{0}] {1}", token.Kind, Encoding.ASCII.GetString(token.Text));
            //}

            var parser = new SmtpParser(new SmtpServerOptionsBuilder().Build());
            Console.WriteLine(parser.TryMakeEhlo(ref reader, out var command, out var errorResponse));
            Console.WriteLine("Command={0} ErrorResponse={1}", command, errorResponse);
        }
    }
}