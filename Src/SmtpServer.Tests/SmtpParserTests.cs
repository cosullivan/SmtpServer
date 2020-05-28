﻿using System;
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
        static SmtpParser CreateParser(string text)
        {
            var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(text));

            var options = new SmtpServerOptionsBuilder().Logger(new NullLogger()).Build();

            return new SmtpParser(options, new TokenEnumerator(new ByteArrayTokenReader(new [] { segment })));
        }

        [Fact]
        public void CanMakeQuit()
        {
            // arrange
            var parser = CreateParser("QUIT");

            // act
            var result = parser.TryMakeQuit(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is QuitCommand);
        }

        [Fact]
        public void CanMakeNoop()
        {
            // arrange
            var parser = CreateParser("NOOP");

            // act
            var result = parser.TryMakeNoop(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is NoopCommand);
        }

        [Fact]
        public void CanMakeHelo()
        {
            // arrange
            var parser = CreateParser("HELO abc-1-def.mail.com");

            // act
            var result = parser.TryMakeHelo(out SmtpCommand command, out SmtpResponse errorResponse);

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
            var parser = CreateParser(input);

            // act
            var result = parser.TryMakeHelo(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.NotNull(errorResponse);
        }

        [Theory]
        [InlineData("EHLO abc-1-def.mail.com")]
        [InlineData("EHLO 192.168.1.200")]
        [InlineData("EHLO [192.168.1.200]")]
        [InlineData("EHLO [IPv6:ABCD:EF01:2345:6789:ABCD:EF01:2345:6789]")]
        public void CanMakeEhlo(string input)
        {
            // arrange
            var parser = CreateParser(input);

            // act
            var result = parser.TryMakeEhlo(out SmtpCommand command, out SmtpResponse errorResponse);

            var ipOrDomainPart = input.Substring(5);

            if (ipOrDomainPart.EndsWith("]"))
            {
                if (ipOrDomainPart.StartsWith("[IPv6:", StringComparison.OrdinalIgnoreCase))
                {
                    ipOrDomainPart = ipOrDomainPart.Substring(6, ipOrDomainPart.Length - 7);
                }
                else
                {
                    ipOrDomainPart = ipOrDomainPart.Substring(1, ipOrDomainPart.Length - 2);
                }
            }

            // assert
            Assert.True(result);
            Assert.True(command is EhloCommand);
            Assert.Equal(ipOrDomainPart, ((EhloCommand)command).DomainOrAddress);
        }

        [Fact]
        public void CanMakeAuthPlain()
        {
            // arrange
            var parser = CreateParser("AUTH PLAIN Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t");

            // act
            var result = parser.TryMakeAuth(out SmtpCommand command, out SmtpResponse errorResponse);

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
            var parser = CreateParser("AUTH LOGIN Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t");

            // act
            var result = parser.TryMakeAuth(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is AuthCommand);
            Assert.Equal(AuthenticationMethod.Login, ((AuthCommand)command).Method);
            Assert.Equal("Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t", ((AuthCommand)command).Parameter);
        }

        [Theory]
        [InlineData("cain.osullivan@gmail.com", "cain.osullivan", "gmail.com")]
        [InlineData(@"""Abc@def""@example.com", "Abc@def", "example.com")]
        [InlineData("pelé@example.com", "pelé", "example.com", "SMTPUTF8")]
        public void CanMakeMail(string email, string user, string host, string extension = null)
        {
            // arrange
            var mailTo = $"MAIL FROM:<{email}>";

            if (!string.IsNullOrWhiteSpace(extension))
            {
                mailTo += $" {extension}";
            }

            var parser = CreateParser(mailTo);

            // act
            var result = parser.TryMakeMail(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.Equal(user, ((MailCommand)command).Address.User);
            Assert.Equal(host, ((MailCommand)command).Address.Host);

            if (!string.IsNullOrWhiteSpace(extension))
            {
                // verify the extension was put in the parameters
                Assert.True(((MailCommand)command).Parameters.ContainsKey(extension));
            }
        }

        [Fact]
        public void CanMakeMailWithNoAddress()
        {
            // arrange
            var parser = CreateParser("MAIL FROM:<>");

            // act
            var result = parser.TryMakeMail(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.NotNull(((MailCommand)command).Address);
            Assert.Equal(String.Empty, ((MailCommand)command).Address.Host);
            Assert.Equal(String.Empty, ((MailCommand)command).Address.User);
        }

        [Fact]
        public void CanMakeMailWithBlankAddress()
        {
            // arrange
            var parser = CreateParser("MAIL FROM:<   >");

            // act
            var result = parser.TryMakeMail(out SmtpCommand command, out SmtpResponse errorResponse);

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
            var parser = CreateParser(input);

            // act
            var result = parser.TryMakeMail(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.False(result);
            Assert.NotNull(errorResponse);
        }

        [Theory]
        [InlineData("cain.osullivan@gmail.com", "cain.osullivan", "gmail.com")]
        [InlineData(@"""Abc@def""@example.com", "Abc@def", "example.com")]
        [InlineData("pelé@example.com", "pelé", "example.com")]
        public void CanMakeRcpt(string email, string user, string host)
        {
            // arrange
            var parser = CreateParser($"RCPT TO:<{email}>");

            // act
            var result = parser.TryMakeRcpt(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is RcptCommand);
            Assert.Equal(user, ((RcptCommand)command).Address.User);
            Assert.Equal(host, ((RcptCommand)command).Address.Host);
        }

        [Fact]
        public void CanMakeProxyIpV4()
        {
            // arrange
            var parser = CreateParser("PROXY TCP4 192.168.1.1 192.168.1.2 1234 16789");

            // act
            var result = parser.TryMakeProxy(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is ProxyProtocolCommand);
            Assert.Equal("192.168.1.1", ((ProxyProtocolCommand)command).SourceEndpoint.Address.ToString());
            Assert.Equal("192.168.1.2", ((ProxyProtocolCommand)command).DestinationEndpoint.Address.ToString());
            Assert.Equal(1234, ((ProxyProtocolCommand)command).SourceEndpoint.Port);
            Assert.Equal(16789, ((ProxyProtocolCommand)command).DestinationEndpoint.Port);
        }

        [Fact]
        public void CanMakeProxyIpV6()
        {
            // arrange
            var parser = CreateParser("PROXY TCP6 2001:1234:abcd::0001 3456:2e76:66d8:f84:abcd:abef:ffff:1234 1234 16789");

            // act
            var result = parser.TryMakeProxy(out SmtpCommand command, out SmtpResponse errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is ProxyProtocolCommand);
            Assert.Equal(IPAddress.Parse("2001:1234:abcd::0001").ToString(), ((ProxyProtocolCommand)command).SourceEndpoint.Address.ToString());
            Assert.Equal(IPAddress.Parse("3456:2e76:66d8:f84:abcd:abef:ffff:1234").ToString(), ((ProxyProtocolCommand)command).DestinationEndpoint.Address.ToString());
            Assert.Equal(1234, ((ProxyProtocolCommand)command).SourceEndpoint.Port);
            Assert.Equal(16789, ((ProxyProtocolCommand)command).DestinationEndpoint.Port);
        }

        [Fact]
        public void CanMakeAtom()
        {
            // arrange
            var parser = CreateParser("hello");
            string atom;

            // act
            var made = parser.TryMakeAtom(out atom);

            // assert
            Assert.True(made);
            Assert.Equal("hello", atom);
        }

        [Fact]
        public void CanMakeDotString()
        {
            // arrange
            var parser = CreateParser("abc.def.hij");
            string dotString;

            // act
            var made = parser.TryMakeDotString(out dotString);

            // assert
            Assert.True(made);
            Assert.Equal("abc.def.hij", dotString);
        }

        [Fact]
        public void CanMakeLocalPart()
        {
            // arrange
            var parser = CreateParser("abc");
            string localPart;

            // act
            var made = parser.TryMakeLocalPart(out localPart);

            // assert
            Assert.True(made);
            Assert.Equal("abc", localPart);
        }

        [Fact]
        public void CanMakeTextOrNumber()
        {
            // arrange
            string textOrNumber1;
            string textOrNumber2;

            // act
            var made1 = CreateParser("abc").TryMakeTextOrNumber(out textOrNumber1);
            var made2 = CreateParser("123").TryMakeTextOrNumber(out textOrNumber2);

            // assert
            Assert.True(made1);
            Assert.Equal("abc", textOrNumber1);
            Assert.True(made2);
            Assert.Equal("123", textOrNumber2);
        }

        [Fact]
        public void CanMakeTextOrNumberOrHyphenString()
        {
            // arrange
            var parser = CreateParser("a1-b2");
            string textOrNumberOrHyphen1;

            // act
            var made1 = parser.TryMakeTextOrNumberOrHyphenString(out textOrNumberOrHyphen1);

            // assert
            Assert.True(made1);
            Assert.Equal("a1-b2", textOrNumberOrHyphen1);
        }

        [Fact]
        public void CanMakeSubdomain()
        {
            // arrange
            var parser = CreateParser("a-1-b-2");
            string subdomain;

            // act
            var made = parser.TryMakeSubdomain(out subdomain);

            // assert
            Assert.True(made);
            Assert.Equal("a-1-b-2", subdomain);
        }

        [Fact]
        public void CanMakeDomain()
        {
            // arrange
            var parser = CreateParser("123.abc.com");
            string domain;

            // act
            var made = parser.TryMakeDomain(out domain);

            // assert
            Assert.True(made);
            Assert.Equal("123.abc.com", domain);
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
        public void CanMakeMailbox(string email, string user, string host)
        {
            // arrange
            var parser = CreateParser(email);
            IMailbox mailbox;

            // act
            var made = parser.TryMakeMailbox(out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal(user, mailbox.User);
            Assert.Equal(host, mailbox.Host);
        }

        [Fact]
        public void CanMakePlusAddressMailBox()
        {
            // arrange
            var parser = CreateParser("cain.osullivan+plus@gmail.com");
            IMailbox mailbox;

            // act
            var made = parser.TryMakeMailbox(out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan+plus", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeAtDomain()
        {
            // arrange
            var parser = CreateParser("@gmail.com");
            string atDomain;

            // act
            var made = parser.TryMakeAtDomain(out atDomain);

            // assert
            Assert.True(made);
            Assert.Equal("@gmail.com", atDomain);
        }

        [Fact]
        public void CanMakeAtDomainList()
        {
            // arrange
            var parser = CreateParser("@gmail.com,@hotmail.com");
            string atDomainList;

            // act
            var made = parser.TryMakeAtDomainList(out atDomainList);

            // assert
            Assert.True(made);
            Assert.Equal("@gmail.com,@hotmail.com", atDomainList);
        }

        [Fact]
        public void CanMakePath()
        {
            // path
            var parser = CreateParser("<@gmail.com,@hotmail.com:cain.osullivan@gmail.com>");
            IMailbox mailbox;

            // act
            var made = parser.TryMakePath(out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeReversePath()
        {
            // path
            var parser = CreateParser("<@gmail.com,@hotmail.com:cain.osullivan@gmail.com>");
            IMailbox mailbox;

            // act
            var made = parser.TryMakePath(out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeAddressLiteral()
        {
            // arrange
            var parser = CreateParser("[ 127.0.0.1 ]");
            string address;

            // act
            var made = parser.TryMakeAddressLiteral(out address);

            // assert
            Assert.True(made);
            Assert.Equal("127.0.0.1", address);
        }

        [Fact]
        public void CanMakeMailParameters()
        {
            // arrange
            var parser = CreateParser("SIZE=123 ABC=DEF ABCDE ZZZ=123");
            IReadOnlyDictionary<string, string> parameters;

            // act
            var made = parser.TryMakeMailParameters(out parameters);

            // assert
            Assert.True(made);
            Assert.Equal(4, parameters.Count);
            Assert.True(parameters.ContainsKey("SIZE"));
            Assert.Equal(parameters["SIZE"], "123");
            Assert.True(parameters.ContainsKey("ABC"));
            Assert.Equal(parameters["ABC"], "DEF");
            Assert.True(parameters.ContainsKey("ZZZ"));
            Assert.Equal(parameters["ZZZ"], "123");
            Assert.True(parameters.ContainsKey("ABCDE"));
        }

        [Theory]
        [InlineData("bWF0dGVvQHBoYXNjb2RlLm9yZw==")]
        [InlineData("AHVzZXIAcGFzc3dvcmQ=")]
        [InlineData("Y2Fpbi5vc3VsbGl2YW5AZ21haWwuY29t")]
        public void CanMakeBase64(string input)
        {
            // arrange
            var parser = CreateParser(input);

            // act
            var made = parser.TryMakeBase64(out string base64);

            // assert
            Assert.True(made);
            Assert.Equal(input, base64);
        }

        [Fact]
        public void CanMakeIpVersion()
        {
            // arrange
            var parser = CreateParser("IPv6:");

            // act
            var result = parser.TryMakeIpVersion(out var version);

            // assert
            Assert.True(result);
            Assert.Equal(6, version);
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
        public void CanMake16BitsHexNumber(string input)
        {
            // arrange
            var parser = CreateParser(input);

            // act
            var result = parser.TryMake16BitsHexNumber(out var hexNumber);

            // assert
            Assert.True(result);
            Assert.Equal(input, hexNumber);
        }

        [Theory]
        [InlineData("G")]
        [InlineData("A123B")]
        public void CanNotMake16BitsHexNumber(string input)
        {
            // arrange
            var parser = CreateParser(input);

            // act
            var result = parser.TryMake16BitsHexNumber(out var hexNumber);

            // assert
            Assert.False(result);
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
        public void CanMakeIpv6AddressLiteral(string input)
        {
            // arrange
            var parser = CreateParser("IPv6:" + input);

            // act
            var result = parser.TryMakeIpv6AddressLiteralWithPrefix(out var address);

            // assert
            Assert.True(result);
            Assert.Equal(input, address);
        }

        [Theory]
        [InlineData("ABCD:EF01:2345:6789:ABCD:EF01:2345")]
        [InlineData("ABCD:EF01:2345:6789:ABCD:EF01:2345:6789:0")]
        [InlineData("FF01:::101")]
        [InlineData(":::1")]
        [InlineData(":::")]
        public void CanNotMakeIpv6AddressLiteral(string input)
        {
            // arrange
            var parser = CreateParser("IPv6:" + input);

            // act
            var result = parser.TryMakeIpv6AddressLiteralWithPrefix(out var address);

            // assert
            Assert.False(result);
        }
    }
}