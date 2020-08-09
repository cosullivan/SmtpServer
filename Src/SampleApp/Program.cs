using System;
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
            //ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;
            
            SimpleExample.Run();
        }
    }
}