# What's new in Version 8
Version 8 contains substantial refactoring to take advantage of [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines/) in an effort to improve throughput performance and reduce memory allocations. 

In addition to this there are also changes to make service resolution easier via Dependency Injection through utilizing the [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider) interface. 

## Performance
The throuhput performance and memory allocation of version 7.2 and version 8 were benchmarked using [BenchmarkDotNet](https://www.nuget.org/packages/BenchmarkDotNet/). The benchmark consisted of a single *Send* test which is the result of having a basic SmtpServer configuration and sending a 143kb email that was take from the Enron Corpus dataset.

### Version 7.2

| Method |     Mean |     Error |    StdDev |   Gen 0 |   Gen 1 | Gen 2 | Allocated |
|------- |---------:|----------:|----------:|--------:|--------:|------:|----------:|
|   Send | 3.885 ms | 0.0760 ms | 0.0781 ms | 93.7500 | 31.2500 |     - | 435.73 KB |

### Version 8

| Method |     Mean |     Error |    StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|------- |---------:|----------:|----------:|--------:|------:|------:|----------:|
|   Send | 2.204 ms | 0.0307 ms | 0.0287 ms | 11.7188 |     - |     - |  41.91 KB |

## Breaking Changes
There are a number of breaking changes from v7.

### SmtpServerOptionsBuilder
The SmtpServerOptionsBuilder is now used to configure the SMTP Server options only and the extensibility services such as IMessageStore, IMailboxFilter, IUserAuthenticator, and IEndpointListener have now been removed from the SmtpServerOptionsBuilder.

### Extensibility Services

#### Service Configuration
The extensibility services such as IMessageStore, IMailboxFilter, IUserAuthenticator, and IEndpointListener are now resolved internally via an instance of [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider). This makes it easier to support Dependency Injection, however, there is a default implementation of the [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider) included that will just resolve the specific instances required for the SmtpServer. The [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider) instance is a constructor parameter on the SmtpServer instance. 

#### Service Resolution
The IMessageStore, IMailboxFilter, and IUserAuthenticator instances were previously resolved by calling the appropriate CreateInstance method on the instance factory, for example, IMessageStoreFactory, IMailboxFilterFactory, and IUserAuthenticatorFactory. 

#### IMessageStore
The IMessageStore interface has a breaking change to the SaveAsync method. The message contents is now included as a [ReadOnlySequence<byte>](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.readonlysequence-1) buffer parameter on the method. 

``` csharp
Task<SmtpResponse> SaveAsync(
    ISessionContext context, 
    IMessageTransaction transaction, 
    ReadOnlySequence<byte> buffer, 
    CancellationToken cancellationToken);
```

This is the exact buffer that is read from the input stream so care must be taken when using it. It must not be referenced and used outside of the scope of this method. 