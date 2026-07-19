using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public class SmtpParserTests
    {
        static TokenReader CreateReader(string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);

            return new TokenReader(new ReadOnlySequence<byte>(buffer, 0, buffer.Length));
        }

        static TokenReader CreateReader(params string[] values)
        {
            return new TokenReader(CreateSequence(values));
        }

        static ReadOnlySequence<byte> CreateSequence(params string[] values)
        {
            var segments = new ByteArraySegmentList();

            foreach (var value in values)
            {
                segments.Append(Encoding.UTF8.GetBytes(value));
            }

            return segments.Build();
        }

        static SmtpParser Parser => new SmtpParser(new SmtpCommandFactory());

        [Fact]
        public void CanMakeUnrecognized()
        {
            // arrange
            var buffer = Encoding.UTF8.GetBytes("ABCDE FGHIJ KLMNO");
            var sequence = new ReadOnlySequence<byte>(buffer, 0, buffer.Length);

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.Equal(SmtpReplyCode.CommandNotImplemented, errorResponse.ReplyCode);
        }

        [Theory]
        [InlineData("HELO abc.example.com extra")]
        [InlineData("MAIL FROM:<sender@example.com> SIZE=")]
        public void CanReturnSyntaxErrorForMalformedKnownCommand(string input)
        {
            // arrange
            var buffer = Encoding.UTF8.GetBytes(input);
            var sequence = new ReadOnlySequence<byte>(buffer, 0, buffer.Length);

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.Equal(SmtpReplyCode.SyntaxError, errorResponse.ReplyCode);
        }

        [Theory]
        [InlineData("ehlo example.com", typeof(EhloCommand))]
        [InlineData("HELO example.com", typeof(HeloCommand))]
        [InlineData("MAIL FROM:<cain.osullivan@gmail.com>", typeof(MailCommand))]
        [InlineData("RCPT TO:<cain.osullivan@gmail.com>", typeof(RcptCommand))]
        [InlineData("HELP", typeof(HelpCommand))]
        [InlineData("VRFY cain.osullivan@gmail.com", typeof(VrfyCommand))]
        [InlineData("EXPN staff", typeof(ExpnCommand))]
        [InlineData("BDAT 5 LAST", typeof(BdatCommand))]
        [InlineData("DATA", typeof(DataCommand))]
        [InlineData("QUIT", typeof(QuitCommand))]
        [InlineData("RSET", typeof(RsetCommand))]
        [InlineData("NOOP", typeof(NoopCommand))]
        [InlineData("STARTTLS", typeof(StartTlsCommand))]
        [InlineData("AUTH PLAIN Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t", typeof(AuthCommand))]
        [InlineData("PROXY UNKNOWN", typeof(ProxyCommand))]
        public void CanMakeKnownCommandUsingTopLevelDispatch(string input, Type commandType)
        {
            // arrange
            var buffer = Encoding.UTF8.GetBytes(input);
            var sequence = new ReadOnlySequence<byte>(buffer, 0, buffer.Length);

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Equal(commandType, command.GetType());
            Assert.Null(errorResponse);
        }

        [Fact]
        public void CanMakeSplitMailWithEsmtpParameters()
        {
            // arrange
            var sequence = CreateSequence("MA", "IL FROM:<sender", "@example.com> SI", "ZE=123 SMTP", "UTF8");

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Null(errorResponse);

            var mailCommand = Assert.IsType<MailCommand>(command);
            Assert.Equal("sender", mailCommand.Address.User);
            Assert.Equal("example.com", mailCommand.Address.Host);
            Assert.Equal("123", mailCommand.Parameters["SIZE"]);
            Assert.True(mailCommand.Parameters.ContainsKey("SMTPUTF8"));
        }

        [Fact]
        public void CanMakeSplitRcptWithEsmtpParameters()
        {
            // arrange
            var sequence = CreateSequence("RC", "PT TO:<recipient@example.com> NOTIFY=SUCCESS,FAIL", "URE OR", "CPT=rfc822;original@example.com");

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Null(errorResponse);

            var rcptCommand = Assert.IsType<RcptCommand>(command);
            Assert.Equal("recipient", rcptCommand.Address.User);
            Assert.Equal("example.com", rcptCommand.Address.Host);
            Assert.Equal("SUCCESS,FAILURE", rcptCommand.Parameters["NOTIFY"]);
            Assert.Equal("rfc822;original@example.com", rcptCommand.Parameters["ORCPT"]);
        }

        [Fact]
        public void CanMakeSplitAuthPlain()
        {
            // arrange
            var sequence = CreateSequence("AU", "TH PL", "AIN Y2Fpbi5vc3", "VsbGl2YW5AZ21haWwuY29t");

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Null(errorResponse);

            var authCommand = Assert.IsType<AuthCommand>(command);
            Assert.Equal(AuthenticationMethod.Plain, authCommand.Method);
            Assert.Equal("Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t", authCommand.Parameter);
        }

        [Fact]
        public void CanMakeSplitBdatLast()
        {
            // arrange
            var sequence = CreateSequence("BD", "AT 102", "4 LA", "ST");

            // act
            var result = Parser.TryMake(ref sequence, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Null(errorResponse);

            var bdatCommand = Assert.IsType<BdatCommand>(command);
            Assert.Equal(1024, bdatCommand.Size);
            Assert.True(bdatCommand.IsLast);
        }

        [Fact]
        public void CanMakeQuit()
        {
            // arrange
            var reader = CreateReader("QUIT");

            // act
            var result = Parser.TryMakeQuit(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is QuitCommand);
        }

        [Fact]
        public void CanMakeNoop()
        {
            // arrange
            var reader = CreateReader("NOOP");

            // act
            var result = Parser.TryMakeNoop(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is NoopCommand);
        }

        [Theory]
        [InlineData("HELP", "")]
        [InlineData("HELP MAIL", "MAIL")]
        public void CanMakeHelp(string input, string argument)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeHelp(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is HelpCommand);
            Assert.Equal(argument, ((HelpCommand)command).Argument);
            Assert.Null(errorResponse);
        }

        [Fact]
        public void CanMakeVrfy()
        {
            // arrange
            var reader = CreateReader("VRFY user@example.com");

            // act
            var result = Parser.TryMakeVrfy(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is VrfyCommand);
            Assert.Equal("user@example.com", ((VrfyCommand)command).Argument);
            Assert.Null(errorResponse);
        }

        [Fact]
        public void CanNotMakeVrfyWithoutArgument()
        {
            // arrange
            var reader = CreateReader("VRFY");

            // act
            var result = Parser.TryMakeVrfy(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.Equal(SmtpReplyCode.SyntaxError, errorResponse.ReplyCode);
        }

        [Fact]
        public void CanMakeExpn()
        {
            // arrange
            var reader = CreateReader("EXPN staff");

            // act
            var result = Parser.TryMakeExpn(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is ExpnCommand);
            Assert.Equal("staff", ((ExpnCommand)command).Argument);
            Assert.Null(errorResponse);
        }

        [Fact]
        public void CanNotMakeExpnWithoutArgument()
        {
            // arrange
            var reader = CreateReader("EXPN");

            // act
            var result = Parser.TryMakeExpn(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.Equal(SmtpReplyCode.SyntaxError, errorResponse.ReplyCode);
        }

        [Theory]
        [InlineData("BDAT 5", 5, false)]
        [InlineData("BDAT 0 LAST", 0, true)]
        [InlineData("BDAT 1024 last", 1024, true)]
        public void CanMakeBdat(string input, long size, bool isLast)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeBdat(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is BdatCommand);
            Assert.Equal(size, ((BdatCommand)command).Size);
            Assert.Equal(isLast, ((BdatCommand)command).IsLast);
            Assert.Null(errorResponse);
        }

        [Theory]
        [InlineData("BDAT")]
        [InlineData("BDAT LAST")]
        [InlineData("BDAT 5 DONE")]
        [InlineData("BDAT 5 LAST extra")]
        public void CanNotMakeBdat(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeBdat(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.Equal(SmtpReplyCode.SyntaxError, errorResponse.ReplyCode);
        }

        [Fact]
        public void CanMakeHelo()
        {
            // arrange
            var reader = CreateReader("HELO abc-1-def.mail.com");

            // act
            var result = Parser.TryMakeHelo(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is HeloCommand);
            Assert.Equal("abc-1-def.mail.com", ((HeloCommand)command).DomainOrAddress);
        }

        [Theory]
        [InlineData("HELO abc.")]
        [InlineData("HELO -abc.com")]
        [InlineData("HELO ////")]
        [InlineData("HELO abc.example.com extra")]
        [InlineData("HELO [192.168.1.200] extra")]
        public void CanNotMakeHelo(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeHelo(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.NotNull(errorResponse);
        }

        [Theory]
        [InlineData("EHLO abc-1-def.mail.com", "abc-1-def.mail.com")]
        [InlineData("EHLO 192.168.1.200", "192.168.1.200")]
        [InlineData("EHLO [192.168.1.200]", "192.168.1.200")]
        [InlineData("EHLO dæmi.is", "dæmi.is")]
        [InlineData("EHLO [IPv6:ABCD:EF01:2345:6789:ABCD:EF01:2345:6789]", "IPv6:ABCD:EF01:2345:6789:ABCD:EF01:2345:6789")]
        public void CanMakeEhlo(string input, string domainOrAddress)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeEhlo(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is EhloCommand);
            Assert.Equal(domainOrAddress, ((EhloCommand)command).DomainOrAddress);
        }

        [Theory]
        [InlineData("EHLO abc.example.com extra")]
        [InlineData("EHLO [192.168.1.200] extra")]
        public void CanNotMakeEhlo(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeEhlo(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public void CanMakeAuthPlain()
        {
            // arrange
            var reader = CreateReader("AUTH PLAIN Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t");

            // act
            var result = Parser.TryMakeAuth(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is AuthCommand);
            Assert.Equal(AuthenticationMethod.Plain, ((AuthCommand)command).Method);
            Assert.Equal("Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t", ((AuthCommand)command).Parameter);
        }

        [Fact]
        public void CanMakeAuthLogin()
        {
            // arrange
            var reader = CreateReader("AUTH LOGIN Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t");

            // act
            var result = Parser.TryMakeAuth(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is AuthCommand);
            Assert.Equal(AuthenticationMethod.Login, ((AuthCommand)command).Method);
            Assert.Equal("Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t", ((AuthCommand)command).Parameter);
        }

        [Fact]
        public void CanMakeAuthXOAuth2()
        {
            // arrange — the "2" tokenizes separately from "XOAUTH", so this proves both tokens are consumed
            var reader = CreateReader("AUTH XOAUTH2 dXNlcj1hbGljZQ==");

            // act
            var result = Parser.TryMakeAuth(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is AuthCommand);
            Assert.Equal(AuthenticationMethod.XOAuth2, ((AuthCommand)command).Method);
            Assert.Equal("dXNlcj1hbGljZQ==", ((AuthCommand)command).Parameter);
        }

        [Fact]
        public void CanMakeAuthXOAuth2WithoutInitialResponse()
        {
            // arrange
            var reader = CreateReader("AUTH XOAUTH2");

            // act
            var result = Parser.TryMakeAuth(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is AuthCommand);
            Assert.Equal(AuthenticationMethod.XOAuth2, ((AuthCommand)command).Method);
            Assert.Null(((AuthCommand)command).Parameter);
        }

        [Fact]
        public void CanMakeAuthOAuthBearer()
        {
            // arrange
            var reader = CreateReader("AUTH OAUTHBEARER dXNlcj1hbGljZQ==");

            // act
            var result = Parser.TryMakeAuth(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is AuthCommand);
            Assert.Equal(AuthenticationMethod.OAuthBearer, ((AuthCommand)command).Method);
            Assert.Equal("dXNlcj1hbGljZQ==", ((AuthCommand)command).Parameter);
        }

        [Theory]
        [InlineData("MAIL FROM:<cain.osullivan@gmail.com>", "cain.osullivan", "gmail.com")]
        [InlineData(@"MAIL FROM:<""Abc@def""@example.com>", "Abc@def", "example.com")]
        [InlineData("MAIL FROM:<pelé@example.com> SMTPUTF8", "pelé", "example.com", "SMTPUTF8")]
        [InlineData("MAIL FROM:<þorsteinn@dæmi.is> SMTPUTF8", "þorsteinn", "dæmi.is", "SMTPUTF8")]
        public void CanMakeMail(string input, string user, string host, string extension = null)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeMail(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.Equal(user, ((MailCommand)command).Address.User);
            Assert.Equal(host, ((MailCommand)command).Address.Host);

            if (extension != null)
            {
                Assert.True(((MailCommand)command).Parameters.ContainsKey(extension));
            }
        }

        [Fact]
        public void CanMakeMailWithDsnParameters()
        {
            // arrange
            var reader = CreateReader("MAIL FROM:<sender@example.com> ret=FULL ENVID=abc123");

            // act
            var result = Parser.TryMakeMail(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Null(errorResponse);
            var mailCommand = Assert.IsType<MailCommand>(command);
            Assert.Equal("FULL", mailCommand.Parameters["RET"]);
            Assert.Equal("abc123", mailCommand.Parameters["envid"]);
        }

        [Fact]
        public void CanMakeMailWithNoAddress()
        {
            // arrange
            var reader = CreateReader("MAIL FROM:<>");

            // act
            var result = Parser.TryMakeMail(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.NotNull(((MailCommand)command).Address);
            Assert.Equal(string.Empty, ((MailCommand)command).Address.Host);
            Assert.Equal(string.Empty, ((MailCommand)command).Address.User);
        }

        [Fact]
        public void CanMakeMailWithBlankAddress()
        {
            // arrange
            var reader = CreateReader("MAIL FROM:<   >");

            // act
            var result = Parser.TryMakeMail(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.NotNull(((MailCommand)command).Address);
            Assert.Equal(String.Empty, ((MailCommand)command).Address.Host);
            Assert.Equal(String.Empty, ((MailCommand)command).Address.User);
        }

        [Theory]
        [InlineData("MAIL FROM:cain")]
        [InlineData("MAIL FROM:<cain.osullivan@gmail.com> SIZE=")]
        [InlineData("MAIL FROM:<cain.osullivan@gmail.com> =BAD")]
        [InlineData("MAIL FROM:<cain.osullivan@gmail.com> SIZE=123 =BAD")]
        public void CanNotMakeMail(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeMail(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.NotNull(errorResponse);
        }

        [Theory]
        [InlineData("RCPT TO:<cain.osullivan@gmail.com>", "cain.osullivan", "gmail.com")]
        [InlineData(@"RCPT TO:<""Abc@def""@example.com>", "Abc@def", "example.com")]
        [InlineData("RCPT TO:<pelé@example.com>", "pelé", "example.com")]
        [InlineData("RCPT TO:<þorsteinn@dæmi.is>", "þorsteinn", "dæmi.is")]
        [InlineData("RCPT TO:<@example1.com:someone@example.com>", "someone", "example.com")]
        [InlineData("RCPT TO:<@example1.com,@example2.com:someone@example.com>", "someone", "example.com")]
        [InlineData("RCPT TO:<example/example@example.com>", "example/example", "example.com")]
        public void CanMakeRcpt(string input, string user, string host)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeRcpt(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is RcptCommand);
            Assert.Equal(user, ((RcptCommand)command).Address.User);
            Assert.Equal(host, ((RcptCommand)command).Address.Host);
        }

        [Fact]
        public void CanMakeRcptWithDsnParameters()
        {
            // arrange
            var reader = CreateReader("RCPT TO:<recipient@example.com> notify=SUCCESS,FAILURE ORCPT=rfc822;original@example.com");

            // act
            var result = Parser.TryMakeRcpt(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.Null(errorResponse);
            var rcptCommand = Assert.IsType<RcptCommand>(command);
            Assert.Equal("recipient", rcptCommand.Address.User);
            Assert.Equal("example.com", rcptCommand.Address.Host);
            Assert.Equal(2, rcptCommand.Parameters.Count);
            Assert.Equal("SUCCESS,FAILURE", rcptCommand.Parameters["NOTIFY"]);
            Assert.Equal("rfc822;original@example.com", rcptCommand.Parameters["orcpt"]);
        }

        [Fact]
        public void CanNotMakeRcptWithInvalidParameters()
        {
            // arrange
            var reader = CreateReader("RCPT TO:<cain.osullivan@gmail.com> NOTIFY=");

            // act
            var result = Parser.TryMakeRcpt(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.NotNull(errorResponse);
        }

        [Theory]
        [InlineData("RCPT TO:<someone@@example.com>")]
        [InlineData("RCPT TO:<someone@example..com>")]
        [InlineData("RCPT TO:<someone@-examplecom>")]
        public void CanNotMakeRcpt(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeRcpt(ref reader, out _, out _);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void CanMakeProxyUnknown()
        {
            // arrange
            var reader = CreateReader("PROXY UNKNOWN");

            // act
            var result = Parser.TryMakeProxy(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is ProxyCommand);
            Assert.Null(((ProxyCommand)command).SourceEndpoint);
            Assert.Null(((ProxyCommand)command).DestinationEndpoint);
        }

        [Fact]
        public void CanMakeProxyTcp4()
        {
            // arrange
            var reader = CreateReader("PROXY TCP4 192.168.1.1 192.168.1.2 1234 16789");

            // act
            var result = Parser.TryMakeProxy(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is ProxyCommand);
            Assert.Equal("192.168.1.1", ((ProxyCommand)command).SourceEndpoint.Address.ToString());
            Assert.Equal("192.168.1.2", ((ProxyCommand)command).DestinationEndpoint.Address.ToString());
            Assert.Equal(1234, ((ProxyCommand)command).SourceEndpoint.Port);
            Assert.Equal(16789, ((ProxyCommand)command).DestinationEndpoint.Port);
        }

        [Fact]
        public void CanMakeProxyTcp6()
        {
            // arrange
            var reader = CreateReader("PROXY TCP6 2001:1234:abcd::0001 3456:2e76:66d8:f84:abcd:abef:ffff:1234 1234 16789");

            // act
            var result = Parser.TryMakeProxy(ref reader, out var command, out var errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is ProxyCommand);
            Assert.Equal(IPAddress.Parse("2001:1234:abcd::0001").ToString(), ((ProxyCommand)command).SourceEndpoint.Address.ToString());
            Assert.Equal(IPAddress.Parse("3456:2e76:66d8:f84:abcd:abef:ffff:1234").ToString(), ((ProxyCommand)command).DestinationEndpoint.Address.ToString());
            Assert.Equal(1234, ((ProxyCommand)command).SourceEndpoint.Port);
            Assert.Equal(16789, ((ProxyCommand)command).DestinationEndpoint.Port);
        }

        [Theory]
        [InlineData("PROXY TCP5 192.168.1.1 192.168.1.2 1234 16789")]
        [InlineData("PROXY TCP46 192.168.1.1 192.168.1.2 1234 16789")]
        [InlineData("PROXY TCPA 192.168.1.1 192.168.1.2 1234 16789")]
        public void CanNotMakeProxyWithInvalidTcpVersion(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = Parser.TryMakeProxy(ref reader, out var command, out var errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.Null(errorResponse);
        }

        [Fact]
        public void CanMakeAtom()
        {
            // arrange
            var reader = CreateReader("hello");

            // act
            var made = reader.TryMake(Parser.TryMakeAtom, out var atom);

            // assert
            Assert.True(made);
            Assert.Equal("hello", StringUtil.Create(atom));
        }

        [Fact]
        public void CanMakeDotString()
        {
            // arrange
            var reader = CreateReader("abc.def.hij");

            // act
            var made = reader.TryMake(Parser.TryMakeDotString, out var dotString);

            // assert
            Assert.True(made);
            Assert.Equal("abc.def.hij", StringUtil.Create(dotString));
        }

        [Fact]
        public void CanMakeLocalPart()
        {
            // arrange
            var reader = CreateReader("abc");

            // act
            var made = reader.TryMake(Parser.TryMakeLocalPart, out var localPart);

            // assert
            Assert.True(made);
            Assert.Equal("abc", StringUtil.Create(localPart));
        }

        [Fact]
        public void CanMakeTextOrNumber()
        {
            // arrange
            var reader1 = CreateReader("abc");
            var reader2 = CreateReader("123");

            // act
            var made1 = reader1.TryMake(Parser.TryMakeTextOrNumber, out var textOrNumber1);
            var made2 = reader2.TryMake(Parser.TryMakeTextOrNumber, out var textOrNumber2);

            // assert
            Assert.True(made1);
            Assert.Equal("abc", StringUtil.Create(textOrNumber1));
            Assert.True(made2);
            Assert.Equal("123", StringUtil.Create(textOrNumber2));
        }

        [Fact]
        public void CanMakeTextOrNumberOrHyphenString()
        {
            // arrange
            var reader = CreateReader("a1-b2");

            // act
            var made1 = reader.TryMake(Parser.TryMakeTextOrNumberOrHyphenString, out var textOrNumberOrHyphen1);

            // assert
            Assert.True(made1);
            Assert.Equal("a1-b2", StringUtil.Create(textOrNumberOrHyphen1));
        }

        [Fact]
        public void CanMakeSubdomain()
        {
            // arrange
            var reader = CreateReader("a-1-b-2");

            // act
            var made = reader.TryMake(Parser.TryMakeSubdomain, out var subdomain);

            // assert
            Assert.True(made);
            Assert.Equal("a-1-b-2", StringUtil.Create(subdomain));
        }

        [Fact]
        public void CanMakeDomain()
        {
            // arrange
            var reader = CreateReader("123.abc.com");

            // act
            var made = reader.TryMake(Parser.TryMakeDomain, out var domain);

            // assert
            Assert.True(made);
            Assert.Equal("123.abc.com", StringUtil.Create(domain));
        }

        [Theory]
        [InlineData("cain.osullivan@gmail.com", "cain.osullivan", "gmail.com")]
        [InlineData(@"""Abc@def""@example.com", "Abc@def", "example.com")]
        [InlineData(@"""Abc\@def""@example.com", "Abc@def", "example.com")]
        [InlineData(@"""Fred Bloggs""@example.com", "Fred Bloggs", "example.com")]
        [InlineData(@"""Joe\\Blow""@example.com", "Joe\\Blow", "example.com")]
        [InlineData(@"customer/department=shipping@example.com", "customer/department=shipping", "example.com")]
        [InlineData(@"$A12345@example.com", "$A12345", "example.com")]
        [InlineData(@"!def!xyz%abc@example.com", "!def!xyz%abc", "example.com")]
        [InlineData(@"_somename@example.com", "_somename", "example.com")]
        [InlineData(@"somename@127.0.0.1", "somename", "127.0.0.1")]
        public void CanMakeMailbox(string email, string user, string host)
        {
            // arrange
            var reader = CreateReader(email);

            // act
            var made = reader.TryMake(Parser.TryMakeMailbox, out IMailbox mailbox);

            // assert
            Assert.True(made);
            Assert.Equal(user, mailbox.User);
            Assert.Equal(host, mailbox.Host);
        }

        [Fact]
        public void CanMakePlusAddressMailBox()
        {
            // arrange
            var reader = CreateReader("cain.osullivan+plus@gmail.com");

            // act
            var made = reader.TryMake(Parser.TryMakeMailbox, out IMailbox mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan+plus", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeAtDomain()
        {
            // arrange
            var reader = CreateReader("@gmail.com");

            // act
            var made = reader.TryMake(Parser.TryMakeAtDomain, out var atDomain);

            // assert
            Assert.True(made);
            Assert.Equal("@gmail.com", StringUtil.Create(atDomain));
        }

        [Fact]
        public void CanMakeAtDomainList()
        {
            // arrange
            var reader = CreateReader("@gmail.com,@hotmail.com");

            // act
            var made = reader.TryMake(Parser.TryMakeAtDomainList, out var atDomainList);

            // assert
            Assert.True(made);
            Assert.Equal("@gmail.com,@hotmail.com", StringUtil.Create(atDomainList));
        }

        [Fact]
        public void CanMakePath()
        {
            // path
            var reader = CreateReader("<@gmail.com,@hotmail.com:cain.osullivan@gmail.com>");

            // act
            var made = reader.TryMake(Parser.TryMakePath, out IMailbox mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeReversePath()
        {
            // path
            var reader = CreateReader("<@gmail.com,@hotmail.com:cain.osullivan@gmail.com>");

            // act
            var made = reader.TryMake(Parser.TryMakePath, out IMailbox mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeAddressLiteral()
        {
            // arrange
            var reader = CreateReader("[ 127.0.0.1 ]");

            // act
            var made = reader.TryMake(Parser.TryMakeAddressLiteral, out var address);

            // assert
            Assert.True(made);
            Assert.Equal("[ 127.0.0.1 ]", StringUtil.Create(address));
        }

        [Fact]
        public void CanMakeMailParameters()
        {
            // arrange
            var reader = CreateReader("SIZE=123 ABC=DEF ABCDE ZZZ=123");

            // act
            var made = reader.TryMake(Parser.TryMakeMailParameters, out IReadOnlyDictionary<string, string> parameters);

            // assert
            Assert.True(made);
            Assert.Equal(4, parameters.Count);
            Assert.True(parameters.ContainsKey("SIZE"));
            Assert.Equal("123", parameters["SIZE"]);
            Assert.True(parameters.ContainsKey("ABC"));
            Assert.Equal("DEF", parameters["ABC"]);
            Assert.True(parameters.ContainsKey("ZZZ"));
            Assert.Equal("123", parameters["ZZZ"]);
            Assert.True(parameters.ContainsKey("ABCDE"));
        }

        [Theory]
        [InlineData("bWF0dGVvQHBoYXNjb2RlLm9yZw==")]
        [InlineData("AHVzZXIAcGFzc3dvcmQ=")]
        [InlineData("Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t")]
        public void CanMakeBase64(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var made = reader.TryMake(Parser.TryMakeBase64, out var base64);

            // assert
            Assert.True(made);
            Assert.Equal(input, StringUtil.Create(base64));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("A9")]
        [InlineData("ABC")]
        [InlineData("ABCD")]
        [InlineData("1BCD")]
        [InlineData("1BC2")]
        [InlineData("1B2D")]
        [InlineData("1B23")]
        [InlineData("AB23")]
        public void CanMake16BitHexNumber(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = reader.TryMake(Parser.TryMake16BitHex, out var hexNumber);

            // assert
            Assert.True(result);
            Assert.Equal(input, StringUtil.Create(hexNumber));
        }

        [Theory]
        [InlineData("!")]
        [InlineData("G")]
        [InlineData("Z321")]
        public void CanNotMake16BitHex(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = reader.TryMake(Parser.TryMake16BitHex, out _);

            // assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("127.0.0.1")]
        public void CanMakeIPv4AddressLiteral(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var made = reader.TryMake(Parser.TryMakeIPv4AddressLiteral, out var address);

            // assert
            Assert.True(made);
            Assert.Equal(input, StringUtil.Create(address));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("0.0")]
        [InlineData("0.0.0")]
        [InlineData("999.999.999.999")]
        public void CanNotMakeIPv4AddressLiteral(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var made = reader.TryMake(Parser.TryMakeIPv4AddressLiteral, out _);

            // assert
            Assert.False(made);
        }

        [Theory]
        [InlineData("IPv6:ABCD:EF01:2345:6789:ABCD:EF01:2345:6789")]
        [InlineData("IPv6:::1")]
        public void CanMakeIPv6AddressLiteral(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = reader.TryMake(Parser.TryMakeIPv6AddressLiteral, out var address);

            // assert
            Assert.True(result);
            Assert.Equal(input, StringUtil.Create(address));
        }

        [Theory]
        [InlineData("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789")]
        [InlineData("2001:DB8::8:800:200C:417A")]
        [InlineData("FF01::101")]
        [InlineData("::1")]
        [InlineData("::")]
        [InlineData("0:0:0:0:0:0:13.1.68.3")]
        [InlineData("0:0:0:0:0:FFFF:129.144.52.38")]
        [InlineData("::13.1.68.3")]
        [InlineData("::FFFF:129.144.52.38")]
        public void CanMakeIPv6Address(string input)
        {
            // arrange
            var reader = CreateReader(input);

            // act
            var result = reader.TryMake(Parser.TryMakeIPv6Address, out var address);

            // assert
            Assert.True(result);
            Assert.Equal(input, StringUtil.Create(address));
        }
        
        [Theory]
        [InlineData("ABCD:EF01:2345:6789:ABCD:EF01:2345")]
        [InlineData("ABCD:EF01:ZZZZ:6789:ABCD:EF01:2345:6789")]
        public void CanNotMakeIPv6AddressLiteral(string input)
        {
            // arrange
            var reader = CreateReader("IPv6:" + input);

            // act
            var result = reader.TryMake(Parser.TryMakeIPv6AddressLiteral, out _);

            // assert
            Assert.False(result);
        }
    }
}
