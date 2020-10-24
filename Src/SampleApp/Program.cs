using System;
using System.IO;
using System.Linq;
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
            //CustomEndpointListenerExample.Run();
            //ServerCancellingExample.Run();
            SessionTracingExample.Run();

            //HERE: how can the ReadOnlySequence buffer be included on the IMessageTransaction?
            //Complete Reader??

            //var files = Directory.GetFiles(@"C:\Temp\enron_mail_20150507.tar", "*.*", SearchOption.AllDirectories).ToList();
            //Console.WriteLine(files.OrderByDescending(file => new FileInfo(file).Length).First());
        }

        static bool IgnoreCertificateValidationFailureForTestingOnly(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}