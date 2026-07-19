# What is SmtpServer?

[![NuGet](https://img.shields.io/nuget/v/SmtpServer.svg)](https://www.nuget.org/packages/SmtpServer/)

**SmtpServer** is a lightweight yet powerful SMTP server implementation in C#.  
Built entirely in .NET, it leverages the Task Parallel Library (TPL) for maximum performance.

## 🆕 What's New?

Check the [Changelog](https://github.com/cosullivan/SmtpServer/blob/master/CHANGELOG.md)

## ⚡ Supported ESMTP Extensions

SmtpServer currently supports the following extensions:

- STARTTLS
- SIZE
- PIPELINING
- 8BITMIME
- SMTPUTF8
- DSN
- CHUNKING
- AUTH PLAIN LOGIN

PIPELINING is advertised because the session reads queued command lines sequentially from the pipe. Commands are still validated against the SMTP state machine in order.

8BITMIME is advertised because message content is accepted and stored as bytes without 7-bit rewriting. Applications remain responsible for MIME validation or normalization in their message store if they need stricter policy.

SMTPUTF8 support covers UTF-8 mailbox/domain parsing and message acceptance. Applications remain responsible for downstream delivery compatibility and storage policy.

SIZE is advertised when `MaxMessageSize(...)` is configured. MAIL `SIZE` parameters are checked before accepting the transaction, and strict DATA/BDAT body limits are enforced while reading content.

DSN support parses and exposes `RET`, `ENVID`, `NOTIFY`, and `ORCPT` envelope parameters. Applications remain responsible for generating and delivering delivery status notifications from their message store or mailbox filter code.

CHUNKING support accepts `BDAT <size> [LAST]` message content without DATA dot-stuffing. Multi-chunk messages are stored only after the `LAST` chunk; strict maximum message size limits are enforced across the full BDAT transfer.

SMTPUTF8, DSN, and CHUNKING are enabled by default for compatibility. Applications can disable advertised and accepted support explicitly:

```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Extensions(extensions => extensions
        .SmtpUtf8(false)
        .Dsn(false)
        .Chunking(false))
    .Build();
```

STARTTLS is advertised only when the endpoint has a certificate and the current connection is not already secure.

AUTH PLAIN LOGIN is advertised only when an authenticator is registered and the current connection is secure or the endpoint explicitly allows insecure authentication.

SMTP replies include enhanced status codes for common success, syntax, authentication, mailbox, size, bad sequence, and transaction failure responses. AUTH continuation challenges are left unchanged for SASL compatibility.

HELP is implemented for basic command discovery. VRFY and EXPN are accepted, but the default policy avoids mailbox or mailing list enumeration and returns conservative `252` responses. Applications that intentionally disclose verification or expansion results can register `ISmtpCommandPolicy` or `ISmtpCommandPolicyFactory`.

## Configuration Limits

`MaxMessageSize(length, handling)` applies to DATA and BDAT message content. `MaxCommandLineLength(length)` applies separately to SMTP command lines and AUTH continuation lines; the default is 4096 bytes, excluding the terminating CRLF.

## Session Policy

Connection and HELO/EHLO policy can be configured without replacing command handlers:

```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .SessionPolicy(policy => policy
        .OnConnectionAccepted((context, token) => Task.FromResult(SmtpResponse.Ok))
        .OnHelo((context, name, token) => Task.FromResult(SmtpResponse.Ok)))
    .Build();
```

## Installation

The package is available on [NuGet](https://www.nuget.org/packages/SmtpServer)
```powershell
PM> install-package SmtpServer
```

## 🚀 Getting Started

Starting a basic SMTP server requires only a few lines of code:

```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Port(25, 587)
    .Build();

var smtpServer = new SmtpServer.SmtpServer(options, ServiceProvider.Default);
await smtpServer.StartAsync(CancellationToken.None);
```

### Logging with Generic Host

When the server is created with an `IServiceProvider` that contains `ILoggerFactory`, SMTP lifecycle and protocol diagnostics are written through `Microsoft.Extensions.Logging`.

```cs
Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<IMessageStore, SampleMessageStore>();

        services.AddSingleton(provider =>
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("SMTP Server")
                .Port(9025)
                .Build();

            return new SmtpServer.SmtpServer(options, provider);
        });

        services.AddHostedService<Worker>();
    });
```

Session lifecycle is logged at `Information`, safe command snapshots at `Debug`, expected SMTP response exceptions at `Warning`, and listener/session faults at `Error`. Session logs include a `BeginScope` with `SessionId`, endpoint details, TLS state, and authentication state. AUTH material and message bodies are not logged by default.

### What hooks are provided?

There are four hooks that can be implemented: `IMessageStore`, `IMailboxFilter`, `IUserAuthenticator`, and `ISmtpCommandPolicy`.

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
