using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
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

        [Theory]
        [InlineData("MAIL FROM:<cain.osullivan@gmail.com>", "cain.osullivan", "gmail.com")]
        [InlineData(@"MAIL FROM:<""Abc@def""@example.com>", "Abc@def", "example.com")]
        [InlineData("MAIL FROM:<pelé@example.com> SMTPUTF8", "pelé", "example.com", "SMTPUTF8")]
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
