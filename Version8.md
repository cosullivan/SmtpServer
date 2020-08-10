# What's new in Version 8
* Big refactor to Pipeline
* Utilising Span and Memory to dramatically reduce the allocations

## Performance
The *Send* test is the resul of having a basic SmtpServer configuration and sending a 143kb email that was take from the Enron Corpus.

### Version 7.2

| Method |     Mean |     Error |    StdDev |   Gen 0 |   Gen 1 | Gen 2 | Allocated |
|------- |---------:|----------:|----------:|--------:|--------:|------:|----------:|
|   Send | 3.885 ms | 0.0760 ms | 0.0781 ms | 93.7500 | 31.2500 |     - | 435.73 KB |

### Version 8

| Method |     Mean |     Error |    StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|------- |---------:|----------:|----------:|--------:|------:|------:|----------:|
|   Send | 2.204 ms | 0.0307 ms | 0.0287 ms | 11.7188 |     - |     - |  41.91 KB |

## Extensibility
ISmtpCommandFactory
- add Sample for overriding Greeting

