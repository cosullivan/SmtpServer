using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace SmtpServer.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<TokenizerBenchmarks>(
            //    ManualConfig
            //        .Create(DefaultConfig.Instance)
            //        .With(ConfigOptions.DisableOptimizationsValidator));

            //var summary = BenchmarkRunner.Run<ThroughputBenchmarks>();

            var summary = BenchmarkRunner.Run<ThroughputBenchmarks>(
                ManualConfig
                    .Create(DefaultConfig.Instance)
                    .With(ConfigOptions.DisableOptimizationsValidator));
        }
    }
}