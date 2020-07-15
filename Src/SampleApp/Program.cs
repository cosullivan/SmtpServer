using System.Net;
using System.Threading.Tasks;
using SampleApp.Examples;
using SmtpServer.Tests;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;

            SimpleExample.Run();
        }
    }
}