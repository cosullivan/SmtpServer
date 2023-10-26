using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using MimeKit;
using SampleApp.Examples;
using SmtpServer;
using SmtpServer.IO;
using SmtpServer.Protocol;
using SmtpServer.Tests;
using SmtpServer.Text;
using SmtpServer.ComponentModel;
using SmtpServer.Net;
using SmtpServer.Tracing;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateValidationFailureForTestingOnly;

            //SimpleExample.Run();
            //SimpleServerExample.Run();
            //CustomEndpointListenerExample.Run();
            //ServerCancellingExample.Run();
            //SessionTracingExample.Run();
            //DependencyInjectionExample.Run();
            SecureServerExample.Run();
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}