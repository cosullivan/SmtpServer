using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer
{
    /// <summary>
    /// Task Extensions
    /// </summary>
    static class TaskExtensions
    {
        /// <summary>
        /// Configures the task to stop waiting when the cancellation has been requested.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token to watch.</param>
        /// <returns>The original task.</returns>
        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Configures the task to stop waiting when the cancellation has been requested.
        /// </summary>
        /// <typeparam name="T">The return type of the task.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token to watch.</param>
        /// <returns>The original task.</returns>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return await task.ConfigureAwait(false);
        }
    }
}
