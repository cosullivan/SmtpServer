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

            //var start = new ByteArraySegment(ToByteArray("abcdefgh"));
            //var end = start
            //    .Append(ToByteArray("ijklmno"))
            //    .Append(ToByteArray("pqrstu"))
            //    .Append(ToByteArray("vwxyz"));

            //var sequence = new ReadOnlySequence<byte>(start, 0, end, end.Memory.Length);

            //Span<byte> pattern = stackalloc byte[3];
            //pattern[0] = (byte)'s';
            //pattern[1] = (byte)'t';
            //pattern[2] = (byte)'u';

            //var head = sequence.GetPosition(0);

            //if (sequence.TryFind(pattern, ref head, out var tail))
            //{
            //    var match = sequence.Slice(head, tail);
            //    var found = sequence.Slice(0, head);

            //    Console.WriteLine(StringUtil.Create(match));
            //    Console.WriteLine(StringUtil.Create(found));
            //}

            var text = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(@"""Abc\@def""@example.com"));
            
            //SimpleExample.Run();

            //////var text = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("EHLO abc-1-def.mail.com"));
            ////var text = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("MAIL FROM:<from@sample.com>"));
            //var text = new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("from@sample.com"));

            var reader = new TokenReader(text);
            //while (reader.Peek() != default)
            //{
            //    var token = reader.Take();

            //    Console.WriteLine("[{0}] {1}", token.Kind, Encoding.ASCII.GetString(token.Text));
            //}

            var parser = new SmtpParser(new SmtpCommandFactory(new SmtpServerOptionsBuilder().Build()));
            Console.WriteLine(reader.TryMake(parser.TryMakeMailbox, out IMailbox mailbox));
            Console.WriteLine(mailbox.AsAddress());

            ////Console.WriteLine(parser.TryMakeEhlo(ref reader, out var command, out var errorResponse));
            ////Console.WriteLine("Command={0} ErrorResponse={1}", command, errorResponse);
            ////Console.WriteLine(parser.TryMakeMail(ref reader, out var command, out var errorResponse));
            ////Console.WriteLine("Command={0} ErrorResponse={1}", command, errorResponse);

            //Console.WriteLine(parser.TryMakeMailbox(ref reader, out var mailbox));
            //Console.WriteLine(mailbox.AsAddress());
        }


        static byte[] ToByteArray(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        public sealed class ByteArraySegment : ReadOnlySequenceSegment<byte>
        {
            public ByteArraySegment(ReadOnlyMemory<byte> memory)
            {
                Memory = memory;
            }

            public ByteArraySegment Append(ReadOnlyMemory<byte> memory)
            {
                var segment = new ByteArraySegment(memory)
                {
                    RunningIndex = RunningIndex + Memory.Length
                };

                Next = segment;

                return segment;
            }
        }
    }
}