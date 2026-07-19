using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.IO;
using SmtpServer.Protocol;

namespace SmtpServer.Benchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class AuthEhloBenchmarks
    {
        readonly EhloCommand _ehloCommand = new EhloCommand("example.com");
        readonly AuthCommand _authPlainCommand = new AuthCommand(AuthenticationMethod.Plain, "AHVzZXIAcGFzc3dvcmQ=");
        readonly AuthCommand _invalidAuthPlainCommand = new AuthCommand(AuthenticationMethod.Plain, "not-base64");

        MemoryStream _ehloStream;
        SmtpSessionContext _ehloContext;
        MemoryStream _authStream;
        SmtpSessionContext _authContext;

        [GlobalSetup]
        public void Setup()
        {
            _ehloStream = new MemoryStream();
            _ehloContext = CreateContext(_ehloStream, addAuthenticator: false);

            _authStream = new MemoryStream();
            _authContext = CreateContext(_authStream, addAuthenticator: true);
        }

        [Benchmark]
        public Task Ehlo()
        {
            Reset(_ehloStream);

            return _ehloCommand.ExecuteAsync(_ehloContext, CancellationToken.None);
        }

        [Benchmark]
        public Task AuthPlain()
        {
            Reset(_authStream);

            return _authPlainCommand.ExecuteAsync(_authContext, CancellationToken.None);
        }

        [Benchmark]
        public Task InvalidAuthPlain()
        {
            Reset(_authStream);

            return _invalidAuthPlainCommand.ExecuteAsync(_authContext, CancellationToken.None);
        }

        static SmtpSessionContext CreateContext(Stream stream, bool addAuthenticator)
        {
            var endpointDefinition = new EndpointDefinitionBuilder()
                .AllowUnsecureAuthentication()
                .Build();
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Endpoint(endpointDefinition)
                .Build();
            var serviceProvider = new ServiceProvider();

            if (addAuthenticator)
            {
                serviceProvider.Add(new DelegatingUserAuthenticator((user, password) => true));
            }

            var context = new SmtpSessionContext(serviceProvider, options, endpointDefinition)
            {
                Pipe = new SecurableDuplexPipe(stream, 128, () => { })
            };

            return context;
        }

        static void Reset(MemoryStream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
        }
    }
}
