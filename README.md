# What is SmtpServer?

[![NuGet](https://img.shields.io/nuget/v/SmtpServer.svg)](https://www.nuget.org/packages/SmtpServer/)

SmtpServer is a simple, but highly functional SMTP server implementation. Written entirely in C# it takes full advantage of the .NET TPL to achieve maximum performance.

SmtpServer is available via [NuGet](https://www.nuget.org/packages/SmtpServer/)

# Whats New?

Check the [Changelog](https://github.com/cosullivan/SmtpServer/blob/master/CHANGELOG.md)

# What does it support?

SmtpServer currently supports the following ESMTP extensions:

- STARTTLS
- SIZE
- PIPELINING
- 8BITMIME
- AUTH PLAIN LOGIN

# How can it be used?

At its most basic, it only takes a few lines of code for the server to be listening to incoming requests.

```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Port(25, 587)
    .Build();

var smtpServer = new SmtpServer.SmtpServer(options, ServiceProvider.Default);
await smtpServer.StartAsync(CancellationToken.None);
```

# What hooks are provided?

There are three hooks that can be implemented; IMessageStore, IMailboxFilter, and IUserAuthenticator.

```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Endpoint(builder =>
        builder
            .Port(9025, true)
            .AllowUnsecureAuthentication(false)
            .Certificate(CreateCertificate()))
    .Build();

var serviceProvider = new ServiceProvider();
serviceProvider.Add(new SampleMessageStore());
serviceProvider.Add(new SampleMailboxFilter());
serviceProvider.Add(new SampleUserAuthenticator());

var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
await smtpServer.StartAsync(CancellationToken.None);

// to create an X509Certificate for testing you need to run MAKECERT.EXE and then PVK2PFX.EXE
// http://www.digitallycreated.net/Blog/38/using-makecert-to-create-certificates-for-development
static X509Certificate2 CreateCertificate()
{
    var certificate = File.ReadAllBytes(@"Certificate.pfx");

    return new X509Certificate2(certificate, "P@ssw0rd");
}
```

```cs
public class SampleMessageStore : MessageStore
{
    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();

        var position = buffer.GetPosition(0);
        while (buffer.TryGet(ref position, out var memory))
        {
            await stream.WriteAsync(memory, cancellationToken);
        }

        stream.Position = 0;

        var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);
        Console.WriteLine(message.TextBody);

        return SmtpResponse.Ok;
    }
}
```

```cs
public class SampleMailboxFilter : IMailboxFilter, IMailboxFilterFactory
{
    public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox @from, int size, CancellationToken cancellationToken)
    {
        if (String.Equals(@from.Host, "test.com"))
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        return Task.FromResult(MailboxFilterResult.NoPermanently);
    }

    public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox @from, CancellationToken token)
    {
        return Task.FromResult(MailboxFilterResult.Yes);
    }

    public IMailboxFilter CreateInstance(ISessionContext context)
    {
        return new SampleMailboxFilter();
    }
}
```

```cs
public class SampleUserAuthenticator : IUserAuthenticator, IUserAuthenticatorFactory
{
    public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken token)
    {
        Console.WriteLine("User={0} Password={1}", user, password);

        return Task.FromResult(user.Length > 4);
    }

    public IUserAuthenticator CreateInstance(ISessionContext context)
    {
        return new SampleUserAuthenticator();
    }
}
```
