# What is SmtpServer?

[![NuGet](https://img.shields.io/nuget/v/SmtpServer.svg)](https://www.nuget.org/packages/SmtpServer/)

SmtpServer is a simple, but highly functional SMTP server implementation. Written entirely in C# it takes full advantage of the .NET TPL to achieve maximum performance.

SmtpServer is available via [NuGet](https://www.nuget.org/packages/SmtpServer/)

# What does it support?
SmtpServer currently supports the following ESMTP extensions:
* STARTTLS
* SIZE
* PIPELINING
* 8BITMIME
* AUTH PLAIN LOGIN

# How can it be used?

At its most basic, it only takes a few lines of code for the server to be listening to incoming requests.

```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Port(25, 587)
    .Build();

var smtpServer = new SmtpServer.SmtpServer(options);
await smtpServer.StartAsync(CancellationToken.None);
```

# What hooks are provided?
There are three hooks that can be implemented; IMessageStore, IMailboxFilter, and IUserAuthenticator.
```cs
var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Port(25, 587)
    .Port(465, isSecure: true)
    .Certificate(CreateX509Certificate2())
    .MessageStore(new SampleMessageStore())
    .MailboxFilter(new SampleMailboxFilter())
    .UserAuthenticator(new SampleUserAuthenticator())
    .Build();

var smtpServer = new SmtpServer.SmtpServer(options);
await smtpServer.StartAsync(CancellationToken.None);
```

```cs
public class SampleMessageStore : MessageStore
{
    public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
    {
        var textMessage = (ITextMessage)transaction.Message;
        
        var message = MimeKit.MimeMessage.Load(textMessage.Content);
        Console.WriteLine(message.TextBody);
    
        return Task.FromResult(SmtpResponse.Ok);
    }
}
```

```cs
public class SampleMailboxFilter : IMailboxFilter
{
    public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox @from, int size = 0, CancellationToken token)
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
}
```

```cs  
public class SampleUserAuthenticator : IUserAuthenticator
{
    public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken token)
    {
        Console.WriteLine("User={0} Password={1}", user, password);

        return Task.FromResult(user.Length > 4);
    }
}
```
