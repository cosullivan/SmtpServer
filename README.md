# What is SmtpServer?
SmtpServer is a simple, but highly functional SMTP server implementation. Written entirely in C# it takes full advantage of the .NET TPL to achieve maximum performance.

SmtpServer is available via NuGet https://www.nuget.org/packages/SmtpServer/

# What does it support?
SmtpServer currently supports the following ESMTP extensions:
* STARTTLS
* SIZE
* PIPELINING

# How can it be used?

At its most basic, it only takes a few lines of code for the server to be listening to incoming requests.

```cs
var options = new OptionsBuilder()
    .ServerName("localhost")
    .Port(25, 587)
    .Build();

var smtpServer = new SmtpServer.SmtpServer(options);
await smtpServer.StartAsync(CancellationToken.None);
```

# What hooks are provided?
There are three hooks that can be implemented; IMessageStore, IMailboxFilter, and IUserAuthenticator.
```cs
var options = new OptionsBuilder()
    .ServerName("localhost")
    .Port(25, 587)
    .MessageStore(new ConsoleMessageStore())
    .MailboxFilter(new MailboxFilter())
    .UserAuthenticator(new UserAuthenticator())
    .Build();

var smtpServer = new SmtpServer.SmtpServer(options);
await smtpServer.StartAsync(CancellationToken.None);
```

```cs
public class ConsoleMessageStore : IMessageStore, IMessageStoreFactory
{
    public IMessageStore CreateInstance(ISessionContext context)
    {
        return this;
    }

    public Task<SmtpResponse> SaveAsync(ISessionContext context, IMimeMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine(message.Mime);
    
        return Task.FromResult(SmtpResponse.Ok);
    }
}
```

```cs
public class MailboxFilter : IMailboxFilter
{
    public Task<MailboxFilterResult> CanAcceptFromAsync(IMailbox @from, int size = 0)
    {
        if (String.Equals(@from.Host, "test.com"))
        {
            return Task.FromResult(MailboxFilterResult.Yes);   
        }

        return Task.FromResult(MailboxFilterResult.NoPermanently);
    }

    public Task<MailboxFilterResult> CanDeliverToAsync(IMailbox to, IMailbox @from)
    {
        return Task.FromResult(MailboxFilterResult.Yes);
    }
}
```

```cs  
public class UserAuthenticator : IUserAuthenticator
{
    public Task<bool> AuthenticateAsync(string user, string password)
    {
        Console.WriteLine("User={0} Password={1}", user, password);

        return Task.FromResult(user.Length > 4);
    }
}
```