using System.Threading.Tasks;
using SampleApp.Examples;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //CustomEndpointListenerExample.Run();

            //ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;

            ServerShutdownExample.Run();
        }
    }
}