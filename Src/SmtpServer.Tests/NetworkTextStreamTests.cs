using System.IO;
using System.Text;
using Xunit;

namespace SmtpServer.Tests
{
    public class NetworkTextStreamTests
    {
        private static Stream GenerateStreamFromString(string s, Encoding encode)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, encode))
            {
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }

        [Fact]
        public void MustProcessAsciiCharset()
        {
            var originalString = "áéíóúãõâêîôû";
            using(var stream = GenerateStreamFromString(originalString, Encoding.ASCII))
            {
                using (var streamNetwork = new NetworkTextStream(stream))
                {
                    var text = streamNetwork.ReadLineAsync().ConfigureAwait(true);
                    Assert.Equal(originalString, text.ToString());
                }
            }
        }
    }
}
