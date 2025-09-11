using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SmtpServer.Tests
{
    internal class RawSmtpClient : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly string _host;
        private readonly int _port;

        internal RawSmtpClient(string host, int port)
        {
            _host = host;
            _port = port;

            _tcpClient = new TcpClient();
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
            _tcpClient.Dispose();
        }

        internal async Task<bool> ConnectAsync()
        {
            await _tcpClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(_host), _port));
            _networkStream = _tcpClient.GetStream();

            var greetingResponse = await WaitForDataAsync();
            if (greetingResponse.StartsWith("220"))
            {
                return true;
            }

            return false;
        }

        internal async Task<string> SendCommandAsync(string command)
        {
            var commandData = Encoding.UTF8.GetBytes($"{command}\r\n");

            await _networkStream.WriteAsync(commandData, 0, commandData.Length);
            return await WaitForDataAsync();
        }

        internal async Task SendDataAsync(string data)
        {
            var mailData = Encoding.UTF8.GetBytes(data);

            await _networkStream.WriteAsync(mailData, 0, mailData.Length);
        }

        internal async Task<string> WaitForDataAsync()
        {
            var buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return receivedData;
            }

            return null;
        }
    }
}
