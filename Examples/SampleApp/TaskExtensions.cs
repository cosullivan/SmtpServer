using System;
using System.Threading.Tasks;

namespace SampleApp
{
    public static class TaskExtensions
    {
        public static void WaitWithoutException(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(exception => exception is OperationCanceledException);
            }
        }
    }
}
