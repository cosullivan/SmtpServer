using System;
using System.Buffers;
using System.Threading.Tasks;
using SampleApp.Examples;
using SmtpServer.Text;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //ServicePointManager.ServerCertificateValidationCallback = SmtpServerTests.IgnoreCertificateValidationFailureForTestingOnly;

            //SimpleExample.Run();

            var text = new ReadOnlySequence<char>("The time has come for all good men to go to the aid of their country.".ToCharArray());

            var reader = new TokenReader(text);
            while (reader.Peek() != default)
            {
                Console.WriteLine(reader.Take().ToString());
            }
        }
    }
}