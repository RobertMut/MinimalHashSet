// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using MyHashSet.MinimalHashSet.Performance;

BenchmarkRunner.Run(new[]
{
    typeof(AddBenchmark),
    typeof(ContainsBenchmark)
});

Console.ReadKey();