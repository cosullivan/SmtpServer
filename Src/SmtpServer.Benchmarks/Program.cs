using BenchmarkDotNet.Running;

namespace SmtpServer.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TokenizerBenchmarks>();
        }
    }
}