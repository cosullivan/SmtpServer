using SmtpServer.Protocol;
using SmtpServer.Protocol.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public class SmtpCommandFactoryTests
    {
        readonly SmtpCommandFactory _smtpCommandFactory;

        public SmtpCommandFactoryTests()
        {
            var options = new SmtpServerOptions { ServerName = "test.smtp.com" };

            _smtpCommandFactory = new SmtpCommandFactory(options, new SmtpParser());
        }

        [Fact]
        public void CanMakeQuit()
        {
            // arrange
            var enumerator = new TokenEnumerator(new Token(TokenKind.Text, "QUIT"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeQuit(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is QuitCommand);
        }

        [Fact]
        public void CanMakeNoop()
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader("NOOP"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeNoop(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is NoopCommand);
        }

        [Fact]
        public void CanMakeHelo()
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader("HELO abc-1-def.mail.com"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeHelo(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is HeloCommand);
            Assert.Equal("abc-1-def.mail.com", ((HeloCommand)command).Domain);
        }

        [Theory]
        [InlineData("HELO abc.")]
        [InlineData("HELO -abc.com")]
        [InlineData("HELO ////")]
        public void CanNotMakeHelo(string input)
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader(input));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeHelo(enumerator, out command, out errorResponse);

            // assert
            Assert.False(result);
            Assert.Null(command);
            Assert.NotNull(errorResponse);
        }

        [Theory]
        [InlineData("EHLO abc-1-def.mail.com")]
        [InlineData("EHLO 192.168.1.200")]
        public void CanMakeEhlo(string input)
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader(input));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeEhlo(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is EhloCommand);
            Assert.Equal(input.Substring(5), ((EhloCommand)command).DomainOrAddress);
        }

        [Fact]
        public void CanMakeMail()
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader("MAIL FROM:<cain.osullivan@gmail.com>"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeMail(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.Equal("cain.osullivan", ((MailCommand)command).Address.User);
            Assert.Equal("gmail.com", ((MailCommand)command).Address.Host);
        }

        [Fact]
        public void CanMakeMailWithNoAddress()
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader("MAIL FROM:<>"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeMail(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.Null(((MailCommand)command).Address);
        }

        [Fact]
        public void CanMakeMailWithBlankAddress()
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader("MAIL FROM:<   >"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeMail(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is MailCommand);
            Assert.Null(((MailCommand)command).Address);
        }

        [Theory]
        [InlineData("MAIL FROM:cain")]
        public void CanNotMakeMail(string input)
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader(input));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeMail(enumerator, out command, out errorResponse);

            // assert
            Assert.False(result);
            Assert.NotNull(errorResponse);
        }

        [Fact]
        public void CanMakeRcpt()
        {
            // arrange
            var enumerator = new TokenEnumerator(new StringTokenReader("RCPT TO:<cain.osullivan@gmail.com>"));

            // act
            SmtpCommand command;
            SmtpResponse errorResponse;
            var result = _smtpCommandFactory.TryMakeRcpt(enumerator, out command, out errorResponse);

            // assert
            Assert.True(result);
            Assert.True(command is RcptCommand);
            Assert.Equal("cain.osullivan", ((RcptCommand)command).Address.User);
            Assert.Equal("gmail.com", ((RcptCommand)command).Address.Host);
        }
    }
}
