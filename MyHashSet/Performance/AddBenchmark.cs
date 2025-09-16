using BenchmarkDotNet.Attributes;

namespace MyHashSet.MinimalHashSet.Performance;

[MemoryDiagnoser]
public class AddBenchmark
{
    private const int Count = 10_000_000;
    private int[] _input;

    [GlobalSetup]
    public void Setup()
    {
        _input = new int[Count];
        var random = new Random();

        for (int i = 0; i < Count; i++)
        {
            _input[i] = random.Next(0, Count);
        }
    }

    [Benchmark]
    public void AddToMyHashSet()
    {
        var myHashSet = new MinimalHashSet<int>(Count);
        for (var i = 0; i < _input.Length; i++)
        {
            myHashSet.Add(_input[i]);
        }
    }

    [Benchmark]
    public void AddToHashSet()
    {
        var hashSet = new HashSet<int>();
        for (var i = 0; i < _input.Length; i++)
        {
            hashSet.Add(_input[i]);
        }
    }
}