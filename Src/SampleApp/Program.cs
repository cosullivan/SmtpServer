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
    class Program
    {
        // https://tools.ietf.org/html/rfc2045

        public sealed class StreamTokenReader
        {
            readonly Stream _stream;
            readonly Encoding _encoding;
            byte[] _buffer;
            int _bytesRead = -1;
            int _index;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stream">The stream to return the tokens from.</param>
            /// <param name="bufferLength">The buffer length to read.</param>
            public StreamTokenReader(Stream stream, int bufferLength = 64) : this(stream, Encoding.ASCII, bufferLength) { }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stream">The stream to return the tokens from.</param>
            /// <param name="encoding">The encoding to use for converting the bytes into strings.</param>
            /// <param name="bufferLength">The buffer length to read.</param>
            public StreamTokenReader(Stream stream, Encoding encoding, int bufferLength = 64)
            {
                _stream = stream;
                _encoding = encoding;
                _buffer = new byte[bufferLength];
            }

            /// <summary>
            /// Reads the next token.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The next token that was read.</returns>
            public async Task<Token> NextTokenAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_index >= _bytesRead)
                {
                    _index = 0;
                    _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ConfigureAwait(false);
                }

                if (_bytesRead == 0)
                {
                    return Token.None;
                }

                var ch = (char)_buffer[_index];

                if (Char.IsLetter(ch))
                {
                    return await TextTokenAsync(cancellationToken).ConfigureAwait(false);
                }

                if (Char.IsNumber(ch))
                {
                    return await NumberTokenAsync(cancellationToken).ConfigureAwait(false);
                }

                return SingleCharacterToken(ch);
            }

            /// <summary>
            /// Creates a single character token that represents the given character.
            /// </summary>
            /// <param name="ch">The character to create the token for.</param>
            /// <returns>The token that represents the given character.</returns>
            Token SingleCharacterToken(char ch)
            {
                _index++;

                if (Char.IsPunctuation(ch))
                {
                    return new Token(TokenKind.Punctuation, ch);
                }

                if (Char.IsSymbol(ch))
                {
                    return new Token(TokenKind.Symbol, ch);
                }

                if (Char.IsWhiteSpace(ch))
                {
                    return new Token(TokenKind.Space, ch);
                }

                return new Token(TokenKind.Other, ch);
            }

            /// <summary>
            /// Returns a Text token from the current position.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The text token that was found at the current position.</returns>
            async Task<Token> TextTokenAsync(CancellationToken cancellationToken)
            {
                return CreateToken(TokenKind.Text, await ConsumeAsync(Char.IsLetterOrDigit, cancellationToken));
            }

            /// <summary>
            /// Returns a Number token from the current position.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The number token that was found at the current position.</returns>
            async Task<Token> NumberTokenAsync(CancellationToken cancellationToken)
            {
                return CreateToken(TokenKind.Number, await ConsumeAsync(Char.IsDigit, cancellationToken));
            }

            /// <summary>
            /// Create a token from the given array segments.
            /// </summary>
            /// <param name="kind">The token kind.</param>
            /// <param name="segments">The list of segments to create the token text from.</param>
            /// <returns>The token that was created from the given list of array segments.</returns>
            Token CreateToken(TokenKind kind, IReadOnlyList<ArraySegment<byte>> segments)
            {
                var text = String.Concat(segments.Select(segment => _encoding.GetString(segment.Array, segment.Offset, segment.Count)));

                return new Token(kind, text);
            }

            /// <summary>
            /// Returns a continuous segment of characters matching the predicate.
            /// </summary>
            /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
            async Task<IReadOnlyList<ArraySegment<byte>>> ConsumeAsync(Func<char, bool> predicate, CancellationToken cancellationToken)
            {
                var segments = new List<ArraySegment<byte>> { Consume(predicate) };

                while (_index >= _bytesRead)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _index = 0;
                    _buffer = new byte[_buffer.Length];
                    _bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken).ConfigureAwait(false);

                    if (_bytesRead == 0 || predicate((char)_buffer[0]) == false)
                    {
                        return segments;
                    }

                    segments.Add(Consume(predicate));
                }

                return segments;
            }

            /// <summary>
            /// Returns a continuous segment of characters matching the predicate.
            /// </summary>
            /// <param name="predicate">The predicate to apply to the characters for the continuous segment.</param>
            /// <returns>The array segment that defines a continuous segment of characters that have matched the predicate.</returns>
            ArraySegment<byte> Consume(Func<char, bool> predicate)
            {
                var start = _index;

                var current = (char)_buffer[_index];
                while (predicate(current) && ++_index < _bytesRead)
                {
                    current = (char)_buffer[_index];
                }

                return new ArraySegment<byte>(_buffer, start, _index - start);
            }
        }

        public sealed class TokenEnumerator2
        {
            readonly StreamTokenReader _tokenReader;
            Token _peek = default(Token);

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="tokenReader">The token reader.</param>
            public TokenEnumerator2(StreamTokenReader tokenReader)
            {
                _tokenReader = tokenReader;
            }

            /// <summary>
            /// Peek at the next token.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The token at the given number of tokens past the current index, or Token.None if no token exists.</returns>
            public async Task<Token> PeekAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                if (_peek == default(Token))
                {
                    _peek = await _tokenReader.NextTokenAsync(cancellationToken);
                }

                return _peek;
            }

            /// <summary>
            /// Take the given number of tokens.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The last token that was consumed.</returns>
            public async Task<Token> TakeAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                var token = _peek;

                if (token == default(Token))
                {
                    return await _tokenReader.NextTokenAsync(cancellationToken);
                }

                _peek = default(Token);

                return token;
            }
        }

        static void Main(string[] args)
        {
            //var text = "MIME-Version: 1.0";
            //var parser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new StringTokenReader(text)));

            //parser.TryMakeMimeVersion(out MimeVersion version);
            //Console.WriteLine(version);

            //var text = "Content-Type: text/plain";
            //var parser = new SmtpServer.Mime.MimeParser(new TokenEnumerator(new StringTokenReader(text)));

            //parser.TryMakeContentType(out ContentType contentType);
            //Console.WriteLine(contentType);

            //var mimeMessageReader = new MimeMessageReader(File.OpenRead(@"C:\Dev\temp\msg.txt"));
            //var mimeMessage = mimeMessageReader.ReadAsync().Result;

            //Console.WriteLine(mimeMessage);

            var webClient = new WebClient();
            using (var stream = webClient.OpenRead("http://memberzonedev.org"))
            //using (var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes("abc-d.3e.fghijkl-mno")))
            {
                var reader = new StreamTokenReader(stream, 5);

                var enumerator = new TokenEnumerator2(reader);
                while (enumerator.PeekAsync().Result != Token.None)
                {
                    Console.WriteLine(enumerator.TakeAsync().Result);
                }
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
            var c = RunClientAsync("A", cancellationTokenSource.Token);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            cancellationTokenSource.Cancel();

            s.WaitWithoutException();
            c.WaitWithoutException();

            return;

            if (args == null || args.Length == 0)
            {
                var serverTask = RunServerAsync(options, cancellationTokenSource.Token);
                var clientTask1 = RunClientAsync("A", cancellationTokenSource.Token);
                var clientTask2 = RunClientAsync("B", cancellationTokenSource.Token);
                var clientTask3 = RunClientAsync("C", cancellationTokenSource.Token);

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
                var clientTask = RunClientAsync(args[1], cancellationTokenSource.Token);

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

        static async Task RunClientAsync(string name, CancellationToken cancellationToken)
        {
            var counter = 1;
            while (cancellationToken.IsCancellationRequested == false)
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
}
