using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SampleApp.Examples;
using SmtpServer;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Text;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;
            var reader = CreateReader("ABC", "DE");

            while (reader.Peek().Kind != TokenKind.None)
            {
                Console.WriteLine(reader.Take().ToString());
            }

            //SimpleExample.Run();
        }

        static TokenReader CreateReader(params string[] values)
        {
            var segments = new ByteArraySegmentList();

            foreach (var value in values)
            {
                segments.Append(Encoding.UTF8.GetBytes(value));
            }

            return new TokenReader(segments.Build());
        }
    }
}