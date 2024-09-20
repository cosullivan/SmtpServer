# Change Log

## v10.0.1
- Fixed a bug that could cause a failure to recognize commands when using a large number of recipients.

## v10.0.0
- Removed MailboxFilterResult in favor of bool result. Impementations can throw SmtpResponseException for more control.
- Handled servers that send the QUIT command and immediately close the connection.
- Added an ICertificateFactory on the Endpoint that allows a new certificate to be created when required without having to restart the server.

## v9.1.0

- Added a ResponseException event handler to the ISessionContext to enable external logging of exceptions.

## v9.0.3

- Fixed a bug with the session not closing when the cancellation token was cancelled.

## v9.0.2

- Fixed a performance issue whereby the server would block incoming connections whilst another connection was upgrading to SSL.

## v9.0.0

- Breaking API change by removing the Certificate from the server options and adding it to the endpoint.

## v8

- Version 8 contains substantial refactoring to take advantage of [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines/) in an effort to improve throughput performance and reduce memory allocations. 
- In addition to this there are also changes to make service resolution easier via Dependency Injection through utilizing the [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider) interface. 