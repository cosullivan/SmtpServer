# What's new in Version 8
Version 8 contains substantial refactoring to take advantage of [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines/) in an effort to improve throughput performance and reduce memory allocations. 

In addition to this there are also changes to make service resolution easier via Dependency Injection through utilizing the [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider) interface. 

## Performance
The throuhput performance and memory allocation of version 7.2 and version 8 were benchmarked using [BenchmarkDotNet](https://www.nuget.org/packages/BenchmarkDotNet/). 

The benchmark for version 7.2 consisted of a single *Send* test which is the result of having a basic SmtpServer configuration and sending a 143kb email that was take from the Enron Corpus dataset.

The benchmark for version 8.0 consisted of the exact same configuration but using three different sized emails, 1kb, 143kb (the same email as the v7.2 benchmark), and a 1,991kb email.

### Version 7.2

| Method |     Mean |     Error |    StdDev |   Gen 0 |   Gen 1 | Gen 2 | Allocated |
|------- |---------:|----------:|----------:|--------:|--------:|------:|----------:|
|  143kb | 3.885 ms | 0.0760 ms | 0.0781 ms | 93.7500 | 31.2500 |     - | 435.73 KB |

### Version 8 

#### .NET Core 3.1.9
.NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT

| Method |        Mean |     Error |    StdDev |    Gen 0 |    Gen 1 | Gen 2 |  Allocated |
|------- |------------:|----------:|----------:|---------:|---------:|------:|-----------:|
|    1kb |    368.2 us |   6.41 us |   5.68 us |   8.7891 |        - |     - |   26.52 KB |
|  143kb |  2,131.6 us |  37.74 us |  55.32 us |  11.7188 |        - |     - |   42.66 KB |
| 1991kb | 27,796.1 us | 337.56 us | 299.24 us | 562.5000 | 218.7500 |     - | 1918.65 KB |

#### .NET Core 5.0.0
.NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT

| Method |        Mean |     Error |    StdDev |    Gen 0 |    Gen 1 | Gen 2 |  Allocated |
|------- |------------:|----------:|----------:|---------:|---------:|------:|-----------:|
|    1kb |    334.5 us |   2.00 us |   1.78 us |   8.7891 |        - |     - |   26.41 KB |
|  143kb |  1,873.4 us |  20.46 us |  19.14 us |  13.6719 |        - |     - |   42.43 KB |
| 1991kb | 25,821.3 us | 492.01 us | 436.15 us | 500.0000 | 156.2500 |     - | 1921.11 KB |

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