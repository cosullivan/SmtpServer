using System;
using System.Collections.Generic;
using System.Text;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Text;
using Xunit;
using Xunit.Extensions;

namespace SmtpServer.Tests
{
    public class SmtpParserTests
    {
        static SmtpParser2 CreateParser(string text)
        {
            var segment = new ArraySegment<byte>(Encoding.ASCII.GetBytes(text));

            return new SmtpParser2(new TokenEnumerator2(new ByteArrayTokenReader(new [] { segment })));
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
    }
}