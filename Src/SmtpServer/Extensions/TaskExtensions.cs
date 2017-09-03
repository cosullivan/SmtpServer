using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.
        /// </summary>
        /// <param name="task">The task to modify the capture context on.</param>
        /// <returns>An object used to await this task.</returns>
        public static ConfiguredTaskAwaitable ReturnOnAnyThread(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Configures an awaiter used to await this <see cref="T:System.Threading.Tasks.Task`1" />.
        /// </summary>
        /// <typeparam name="TResult">The result of the Task.</typeparam>
        /// <param name="task">The task to modify the capture context on.</param>
        /// <returns>An object used to await this task.</returns>
        public static ConfiguredTaskAwaitable<TResult> ReturnOnAnyThread<TResult>(this Task<TResult> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false);
        }

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

        /// <summary>
        /// Configures the task to stop waiting when the cancellation has been requested.
        /// </summary>
        /// <typeparam name="T">The return type of the task.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="timeout">The timeout to apply to the task.</param>
        /// <param name="cancellationToken">The cancellation token to watch.</param>
        /// <returns>The original task.</returns>
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (task != await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                throw new TimeoutException();
            }

            return await task.ConfigureAwait(false);
        }
    }
}