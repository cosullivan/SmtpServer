using System.Threading.Tasks;
using SampleApp.Examples;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;

            SimpleExample.Run();
        }
    }
}