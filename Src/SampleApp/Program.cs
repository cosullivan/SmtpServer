using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SampleApp.Examples;
using SmtpServer.IO;
using SmtpServer.Text;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            //SimpleExample.Run();
            //SimpleServerExample.Run();
            CustomEndpointListenerExample.Run();
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}