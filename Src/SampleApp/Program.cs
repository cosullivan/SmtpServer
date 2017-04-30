using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Tracing;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using SmtpServer.Content;
using SmtpServer.Mime;
using SmtpServer.Text;
using ContentType = SmtpServer.Mime.ContentType;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace SampleApp
{
    // https://tools.ietf.org/html/rfc2045
    class Program
    {
        static string MIME = @"MIME-Version: 1.0
Content-Type: multipart/mixed; boundary=frontier

This is a message with multiple parts in MIME format.
--frontier
Content-Type: text/plain

This is the body of the message.
--frontier
Content-Type: application/octet-stream
Content-Transfer-Encoding: base64

PGh0bWw+CiAgPGhlYWQ+CiAgPC9oZWFkPgogIDxib2R5PgogICAgPHA+VGhpcyBpcyB0aGUg
Ym9keSBvZiB0aGUgbWVzc2FnZS48L3A+CiAgPC9ib2R5Pgo8L2h0bWw+Cg==
--frontier--";

        
        static void Main(string[] args)
        {


            //Console.WriteLine(Char.IsPunctuation((char)34));
            //return;
            //var text = "MIME-Version: 1.0";
            //var parser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new StringTokenReader(text)));

            //parser.TryMakeMimeVersion(out MimeVersion version);
            //Console.WriteLine(version);

            //var text = "Content-Type: text/plain";
            //var parser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new StringTokenReader(text)));

            //parser.TryMakeContentType(out ContentType contentType);
            //Console.WriteLine(contentType);

            ////var mimeMessageReader = new MimeMessageReader(File.OpenRead(@"C:\Dev\temp\msg.txt"));
            //var mimeMessageReader = new MimeMessageReader(File.OpenRead(@"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_"));
            ////var mimeMessageReader = new MimeMessageReader(new MemoryStream(Encoding.ASCII.GetBytes(MIME)));
            //try
            //{
            //    var mimeMessage = mimeMessageReader.ReadAsync().Result;
            //    Console.WriteLine(mimeMessage);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}

            ////var mimeMessageReader = new MimeMessageReader(File.OpenRead(@"C:\Dev\temp\msg.txt"));
            //var mimeMessageSerializer = new MimeMessageSerializer();
            ////var mimeMessageReader = new MimeMessageReader(new MemoryStream(Encoding.ASCII.GetBytes(MIME)));
            //try
            //{
            //    //File.OpenRead(@"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_")
            //    var mimeMessage = mimeMessageSerializer.DeserializeAsync(File.OpenRead(@"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_")).Result;
            //    Console.WriteLine(mimeMessage);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}

            //var webClient = new WebClient();
            ////using (var stream = webClient.OpenRead("http://memberzonedev.org"))
            //using (var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes("abcd\r\n1234")))
            //{
            //    var reader = new StreamTokenReader(stream, 5);

            //    var enumerator = new TokenEnumerator2(reader, ignoreWhiteSpace: false);

            //    while (enumerator.PeekAsync().Result != Token.None)
            //    {
            //        Console.WriteLine(enumerator.TakeAsync().Result);
            //    }
            //}

            //return;

            using (var file = File.OpenRead(@"C:\Dev\Enron Corpus\maildir\allen-p\inbox\31_"))
            {
                var stream = new ByteStream(file);

                HERE: what other methods on the stream will be needed?
                how is the best way to filter the dot stuffing and is that up to the server?

                //string line;
                //while ((line = stream.ReadLineAsync(Encoding.ASCII).Result) != null)
                //{
                //    Console.WriteLine(line);
                //}

                var buffers = stream.ReadBlockAsync().Result;
                Console.WriteLine(buffers.Count);
            }

            return;

            var cancellationTokenSource = new CancellationTokenSource();

            //var certificate = CreateCertificate();

            //ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            var options = new OptionsBuilder()
                .ServerName("SmtpServer SampleApp")
                .Port(9025)
                //.Certificate(certificate)
                .SupportedSslProtocols(SslProtocols.Default)
                .MessageStore(new ConsoleMessageStore())
                .MailboxFilter(new ConsoleMailboxFilter())
                .Build();

            var s = RunServerAsync(options, cancellationTokenSource.Token);
            var c = RunClientAsync("A", 1, cancellationTokenSource.Token);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            cancellationTokenSource.Cancel();

            s.WaitWithoutException();
            c.WaitWithoutException();

            return;

            if (args == null || args.Length == 0)
            {
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);
                var clientTask1 = RunClientAsync("A", cancellationToken: cancellationTokenSource.Token);
                var clientTask2 = RunClientAsync("B", cancellationToken: cancellationTokenSource.Token);
                var clientTask3 = RunClientAsync("C", cancellationToken: cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                serverTask.WaitWithoutException();
                clientTask1.WaitWithoutException();
                clientTask2.WaitWithoutException();
                clientTask3.WaitWithoutException();

                return;
            }

            if (args[0] == "server")
            {
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                serverTask.WaitWithoutException();

                return;
            }

            if (args[0] == "client")
            {
                var clientTask = RunClientAsync(args[1], cancellationToken: cancellationTokenSource.Token);

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                cancellationTokenSource.Cancel();

                clientTask.WaitWithoutException();
            }
        }

        static async Task RunServerAsync(ISmtpServerOptions options, CancellationToken cancellationToken)
        {
            var smtpServer = new SmtpServer.SmtpServer(options);

            smtpServer.SessionCreated += OnSmtpServerSessionCreated;
            smtpServer.SessionCompleted += OnSmtpServerSessionCompleted;

            await smtpServer.StartAsync(cancellationToken);

            smtpServer.SessionCreated -= OnSmtpServerSessionCreated;
            smtpServer.SessionCompleted -= OnSmtpServerSessionCompleted;
        }

        static async Task RunClientAsync(string name, int limit = Int32.MaxValue, CancellationToken cancellationToken = default(CancellationToken))
        {
            var counter = 1;
            while (limit-- >= 0 && cancellationToken.IsCancellationRequested == false)
            {
                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync("localhost", 9025, false, cancellationToken);
                    //smtpClient.EnableSsl = true;

                    try
                    {
                        var message = new MimeKit.MimeMessage();
                        message.From.Add(new MimeKit.MailboxAddress($"{name}{counter}@test.com"));
                        message.To.Add(new MimeKit.MailboxAddress("sample@test.com"));
                        message.Subject = $"{name} {counter}";

                        message.Body = new TextPart(TextFormat.Plain)
                        {
                            Text = "Assunto teste acento çãõáéíóú",
                            //Text = "Assunto teste acento",
                        };

                        await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                    await smtpClient.DisconnectAsync(true, cancellationToken);
                }

                counter++;
            }
        }

        static void OnSmtpServerSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCreated: {0}", e.Context.RemoteEndPoint);

            e.Context.CommandExecuting += OnCommandExecuting;
        }

        static void OnCommandExecuting(object sender, SmtpCommandExecutingEventArgs e)
        {
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSmtpServerSessionCompleted(object sender, SessionEventArgs e)
        {
            e.Context.CommandExecuting -= OnCommandExecuting;

            Console.WriteLine("SessionCompleted: {0}", e.Context.RemoteEndPoint);
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        
        static X509Certificate2 CreateCertificate()
        {
            // to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
            // http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development

            var certificate = File.ReadAllBytes(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx");
            var password = File.ReadAllText(@"C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServerPassword.txt");

            return new X509Certificate2(certificate, password);
        }
    }

    public interface IByteStream
    {
        /// <summary>
        /// Returns a series a buffer segments whilst the predicate is satisfied.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the bytes for the continuous segment.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of buffers that contain the bytes matching while the predicate was true.</returns>
        Task<IReadOnlyList<ArraySegment<byte>>> ReadAsync(Func<byte, bool> predicate, long count, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Write a list of byte array segments.
        /// </summary>
        /// <param name="buffers">The list of array segment buffers to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        Task WriteAsync(IReadOnlyList<ArraySegment<byte>> buffers, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Upgrade to a secure stream.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a value indicating whether or not the stream is secure.
        /// </summary>
        bool IsSecure { get; }
    }

    public static class ByteStreamExtensions
    {
        /// <summary>
        /// Returns a continuous segment of bytes until the given sequence is reached.
        /// </summary>
        /// <param name="stream">The byte stream to perform the operation on.</param>
        /// <param name="sequence">The sequence to match to enable the read operation to complete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        public static async Task<IReadOnlyList<ArraySegment<byte>>> ReadUntilAsync(this IByteStream stream, byte[] sequence, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var found = 0;
            return await stream.ReadAsync(current =>
            {
                if (found >= sequence.Length)
                {
                    return false;
                }

                if (current == sequence[found])
                {
                    found++;
                }
                else
                {
                    found = 0;
                }

                return true;
            },
            Int32.MaxValue,
            cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="stream">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static Task<string> ReadLineAsync(this IByteStream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return ReadLineAsync(stream, Encoding.ASCII, cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream.
        /// </summary>
        /// <param name="stream">The stream to read a line from.</param>
        /// <param name="encoding">The encoding to use when converting the bytes to a text representation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The string that was read from the stream.</returns>
        public static async Task<string> ReadLineAsync(this IByteStream stream, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var buffers = await stream.ReadUntilAsync(new byte[] { 13, 10 }, cancellationToken);
            var count = buffers.Sum(buffer => buffer.Count);

            return buffers.Count == 0
                ? null
                : encoding.GetString(buffers.SelectMany(buffer => buffer).Take(count - 2).ToArray());
        }

        /// <summary>
        /// Read a blank-line delimated block.
        /// </summary>
        /// <param name="stream">The stream to read a line from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffers that were read until the block was terminated.</returns>
        public static Task<IReadOnlyList<ArraySegment<byte>>> ReadBlockAsync(this IByteStream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return stream.ReadUntilAsync(new byte[] { 13, 10, 13, 10 }, cancellationToken);
        }
    }

    public sealed class ByteStream : IByteStream
    {
        readonly Stream _stream;
        readonly int _bufferLength;
        byte[] _buffer;
        int _bytesRead = -1;
        int _index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stream">The stream to return the tokens from.</param>
        /// <param name="bufferLength">The buffer length to read.</param>
        internal ByteStream(Stream stream, int bufferLength = 64)
        {
            _stream = stream;
            _bufferLength = bufferLength;
        }

        /// <summary>
        /// Returns a series a buffer segments whilst the predicate is satisfied.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the bytes for the continuous segment.</param>
        /// <param name="count">The number of bytes to consume.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of buffers that contain the bytes matching while the predicate was true.</returns>
        public async Task<IReadOnlyList<ArraySegment<byte>>> ReadAsync(Func<byte, bool> predicate, long count, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ReadBufferAsync(cancellationToken) == false)
            {
                return new List<ArraySegment<byte>>();
            }

            var segments = new List<ArraySegment<byte>> { Consume(predicate, count) };

            while (_index >= _bytesRead)
            {
                if (await ReadBufferAsync(cancellationToken) == false)
                {
                    return segments;
                }

                if (count <= 0 || predicate(_buffer[0]) == false)
                {
                    return segments;
                }

                segments.Add(Consume(predicate, count));
            }

            return segments;
        }

        /// <summary>
        /// Write a list of byte array segments.
        /// </summary>
        /// <param name="buffers">The list of array segment buffers to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        public Task WriteAsync(IReadOnlyList<ArraySegment<byte>> buffers, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Flush the write buffers to the stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Upgrade to a secure stream.
        /// </summary>
        /// <param name="certificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="protocols">The value that represents the protocol used for authentication.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that asynchronously performs the operation.</returns>
        public Task UpgradeAsync(X509Certificate certificate, SslProtocols protocols, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensure that the buffer is full for a read operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns a value indicating whether there was no more data to fill the buffer.</returns>
        async Task<bool> ReadBufferAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_index >= _bytesRead)
            {
                _index = 0;
                _buffer = new byte[_bufferLength];
                _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ReturnOnAnyThread();
            }

            return _bytesRead > 0;
        }

        /// <summary>
        /// Returns a continuous segment of characters matching the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
        /// <param name="limit">The limit to the number of characters to consume.</param>
        /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
        ArraySegment<byte> Consume(Func<byte, bool> predicate, long limit)
        {
            var start = _index;

            var current = _buffer[_index];
            while (limit-- > 0 && predicate(current) && ++_index < _bytesRead)
            {
                current = _buffer[_index];
            }

            return new ArraySegment<byte>(_buffer, start, _index - start);
        }

        /// <summary>
        /// Returns a value indicating whether or not the stream is secure.
        /// </summary>
        public bool IsSecure { get; private set; }
    }
}