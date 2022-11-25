using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string exportsPath = Path.Combine(userPath, "DoxygenEditor.Benchmarks");

            DateTime dt = DateTime.Now;

            string entryName = dt.ToString("yyyyMMdd-HHmmss");

            string entryPath = Path.Combine(exportsPath, entryName);

            var config = new ManualConfig();
            config = config.AddJob(Job.Default.WithLaunchCount(1).WithWarmupCount(7).WithIterationCount(20));
            config = config.AddLogger(ConsoleLogger.Default);
            config = config.AddDiagnoser(MemoryDiagnoser.Default);
            config = config.WithOptions(ConfigOptions.DisableOptimizationsValidator);
            config = config.AddColumnProvider(DefaultColumnProviders.Instance);
            config = config.WithArtifactsPath(entryPath);

            BenchmarkRunner.Run<CppBenchmarks>(config);
            //BenchmarkRunner.Run<DoxygenBenchmarks>();
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
