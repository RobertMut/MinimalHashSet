using BenchmarkDotNet.Attributes;

namespace MyHashSet.MinimalHashSet.Performance;

[MemoryDiagnoser]
public class ContainsBenchmark
{
    private const int Count = 10_000_000;
    private int[] _input;
    private HashSet<int> _hashSet;
    private MinimalHashSet<int> _myHashSet;

    [GlobalSetup]
    public void Setup()
    {
        _input = new int[Count];
        _hashSet = new HashSet<int>(Count);
        _myHashSet = new MinimalHashSet<int>(Count);
        
        var random = new Random();

        for (int i = 0; i < Count; i++)
        {
            int integer = random.Next(0, Count);
            _hashSet.Add(integer);
            _myHashSet.Add(integer);
            _input[i] = random.Next(0, Count);
        }
    }

    [Benchmark]
    public void ContainsInMyHashSet()
    {
        for (int i = 0; i < Count; i++)
        {
            _myHashSet.Contains(_input[i]);
        }
    }

    [Benchmark]
    public void ContainsInHashSet()
    {
        for(int i = 0; i < Count; i++)
        {
            _hashSet.Contains(_input[i]);
        }
    }

    [Benchmark]
    public void ContainsInMyHashSetMiss()
    {
        for (int i = 0; i < Count; i++)
        {
            _myHashSet.Contains(_input[i] % 2_000_000);
        }
    }
    
    [Benchmark]
    public void ContainsInHashSetMiss()
    {
        for (int i = 0; i < Count; i++)
        {
            _hashSet.Contains(_input[i] % 2_000_000);
        }
    }
}