using System.Net.Security;
using SmtpServer.Authentication;

namespace SampleApp
{
    public sealed class SampleClientCertificateValidator : ClientCertificateValidator
    {
        public override RemoteCertificateValidationCallback RemoteClientCertificateValidationCallback { get; set; } =
            (sender, certificate, chain, sslPolicyErrors) =>
            {
                // Provide your custom client certificate validation logic here.
                // Return true if valid; otherwise, false.
                return true;
            };
    }
}