using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Protocol;

namespace SmtpServer.IO
{
    public interface INetworkPipe : IDuplexPipe, IDisposable { }

    public static class NetworkPipeExtensions
    {
        /// <summary>
        /// Write a reply to the client.
        /// </summary>
        /// <param name="pipe">The pipe to perform the operation on.</param>
        /// <param name="response">The response to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public static Task ReplyAsync(this INetworkPipe pipe, SmtpResponse response, CancellationToken cancellationToken)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException(nameof(pipe));
            }

            //await client.WriteLineAsync($"{(int)response.ReplyCode} {response.Message}", cancellationToken).ConfigureAwait(false);
            //await client.FlushAsync(cancellationToken).ConfigureAwait(false);

            throw new NotImplementedException();
        }
    }
}