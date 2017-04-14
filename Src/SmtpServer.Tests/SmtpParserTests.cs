using System.Collections.Generic;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Text;
using Xunit;
using Xunit.Extensions;

namespace SmtpServer.Tests
{
    public class SmtpParserTests
    {
        [Fact]
        public void CanMakeAtom()
        {
            // arrange
            var parser = new SmtpParser();
            string atom;

            // act
            var made = parser.TryMakeAtom(new TokenEnumerator(new StringTokenReader("hello")), out atom);

            // assert
            Assert.True(made);
            Assert.Equal("hello", atom);
        }

        [Fact]
        public void CanMakeDotString()
        {
            // arrange
            var parser = new SmtpParser();
            string dotString;

            // act
            var made = parser.TryMakeDotString(new TokenEnumerator(new StringTokenReader("abc.def.hij")), out dotString);

            // assert
            Assert.True(made);
            Assert.Equal("abc.def.hij", dotString);
        }

        [Fact]
        public void CanMakeLocalPart()
        {
            // arrange
            var parser = new SmtpParser();
            string localPart;

            // act
            var made = parser.TryMakeLocalPart(new TokenEnumerator(new StringTokenReader("abc")), out localPart);

            // assert
            Assert.True(made);
            Assert.Equal("abc", localPart);
        }

        [Fact]
        public void CanMakeTextOrNumber()
        {
            // arrange
            var parser = new SmtpParser();
            string textOrNumber1;
            string textOrNumber2;

            // act
            var made1 = parser.TryMakeTextOrNumber(new TokenEnumerator(new StringTokenReader("abc")), out textOrNumber1);
            var made2 = parser.TryMakeTextOrNumber(new TokenEnumerator(new StringTokenReader("123")), out textOrNumber2);

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
            var parser = new SmtpParser();
            string textOrNumberOrHyphen1;

            // act
            var made1 = parser.TryMakeTextOrNumberOrHyphenString(new TokenEnumerator(new StringTokenReader("a1-b2")), out textOrNumberOrHyphen1);

            // assert
            Assert.True(made1);
            Assert.Equal("a1-b2", textOrNumberOrHyphen1);
        }

        [Fact]
        public void CanMakeSubdomain()
        {
            // arrange
            var parser = new SmtpParser();
            string subdomain;

            // act
            var made = parser.TryMakeSubdomain(new TokenEnumerator(new StringTokenReader("a-1-b-2")), out subdomain);

            // assert
            Assert.True(made);
            Assert.Equal("a-1-b-2", subdomain);
        }

        [Fact]
        public void CanMakeDomain()
        {
            // arrange
            var parser = new SmtpParser();
            string domain;

            // act
            var made = parser.TryMakeDomain(new TokenEnumerator(new StringTokenReader("123.abc.com")), out domain);

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
            var parser = new SmtpParser();
            IMailbox mailbox;

            // act
            var made = parser.TryMakeMailbox(new TokenEnumerator(new StringTokenReader(email)), out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal(user, mailbox.User);
            Assert.Equal(host, mailbox.Host);
        }

        [Fact]
        public void CanMakePlusAddressMailBox()
        {
            // arrange
            var parser = new SmtpParser();
            IMailbox mailbox;

            // act
            var made = parser.TryMakeMailbox(new TokenEnumerator(new StringTokenReader("cain.osullivan+plus@gmail.com")), out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan+plus", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeAtDomain()
        {
            // arrange
            var parser = new SmtpParser();
            string atDomain;

            // act
            var made = parser.TryMakeAtDomain(new TokenEnumerator(new StringTokenReader("@gmail.com")), out atDomain);

            // assert
            Assert.True(made);
            Assert.Equal("@gmail.com", atDomain);
        }

        [Fact]
        public void CanMakeAtDomainList()
        {
            // arrange
            var parser = new SmtpParser();
            string atDomainList;

            // act
            var made = parser.TryMakeAtDomainList(new TokenEnumerator(new StringTokenReader("@gmail.com,@hotmail.com")), out atDomainList);

            // assert
            Assert.True(made);
            Assert.Equal("@gmail.com,@hotmail.com", atDomainList);
        }

        [Fact]
        public void CanMakePath()
        {
            // path
            var parser = new SmtpParser();
            IMailbox mailbox;

            // act
            var made = parser.TryMakePath(new TokenEnumerator(new StringTokenReader("<@gmail.com,@hotmail.com:cain.osullivan@gmail.com>")), out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeReversePath()
        {
            // path
            var parser = new SmtpParser();
            IMailbox mailbox;

            // act
            var made = parser.TryMakePath(new TokenEnumerator(new StringTokenReader("<@gmail.com,@hotmail.com:cain.osullivan@gmail.com>")), out mailbox);

            // assert
            Assert.True(made);
            Assert.Equal("cain.osullivan", mailbox.User);
            Assert.Equal("gmail.com", mailbox.Host);
        }

        [Fact]
        public void CanMakeAddressLiteral()
        {
            // arrange
            var parser = new SmtpParser();
            string address;

            // act
            var made = parser.TryMakeAddressLiteral(new TokenEnumerator(new StringTokenReader("[ 127.0.0.1 ]")), out address);

            // assert
            Assert.True(made);
            Assert.Equal("127.0.0.1", address);
        }

        [Fact]
        public void CanMakeMailParameters()
        {
            // arrange
            var parser = new SmtpParser();
            IReadOnlyDictionary<string, string> parameters;

            // act
            var made = parser.TryMakeMailParameters(new TokenEnumerator(new StringTokenReader("SIZE=123 ABC=DEF ABCDE ZZZ=123")), out parameters);

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