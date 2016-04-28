using System.Threading.Tasks;

namespace SampleApp
{
    public static class TaskExtensions
    {
        public static Task WaitWithoutException(this Task task)
        {
            return task.ContinueWith(t => { });
        }
    }
}