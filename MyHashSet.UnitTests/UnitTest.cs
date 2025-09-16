using MyHashSet.MinimalHashSet;
using Shouldly;

namespace MyHashSet.UnitTests;

public class Tests
{
    private int[] input;
    [SetUp]
    public void Setup()
    {
        input = new int[5_000_000];
        for(int i = 0; i < input.Length; i++)
        {
            input[i] = i;
        }
        
    }

    [Test]
    public void HashSetShouldBeEquivalentToMinimalHashSet()
    {
        HashSet<int> hashSet = new HashSet<int>(input);
        MinimalHashSet<int> myHashSet = new MinimalHashSet<int>(input);
        bool[] hashSetResult = new bool[input.Length];
        bool[] myHashSetResult = new bool[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            hashSetResult[i] = hashSet.Contains(input[i]);
            myHashSetResult[i] = myHashSet.Contains(input[i]);
        }
        
        hashSetResult.ShouldBeEquivalentTo(myHashSetResult);
    }

    [Test]
    public void MinimalHashSetShouldHandleEmptyArray()
    {
        MinimalHashSet<int> hashSet = new MinimalHashSet<int>(Array.Empty<int>());
        
        hashSet.Contains(0).ShouldBeFalse();
        hashSet.Add(1).ShouldBeTrue();
        hashSet.Contains(1).ShouldBeTrue();
    }

    [Test]
    public void MinimalHashSetShouldThrowArgumentNullException()
    {
        Action a = () => new MinimalHashSet<int>(null);
        a.ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void MinimalHashSetShouldExecuteOnObject()
    {
        (int, string)[] newInput = new (int, string)[]
        {
            (5, "five"), (10, "ten"), (15, "fifteen"), (20, "twenty"), (25, "twenty-five")
        };
        
        MinimalHashSet<(int, string)> minimalHashSet = new MinimalHashSet<(int, string)>(newInput);

        minimalHashSet.Contains(newInput[0]).ShouldBeTrue();
        minimalHashSet.Contains(newInput[1]).ShouldBeTrue();
        minimalHashSet.Contains(newInput[2]).ShouldBeTrue();
        minimalHashSet.Contains(newInput[3]).ShouldBeTrue();
        minimalHashSet.Contains(newInput[4]).ShouldBeTrue();
    }
    
    [Test]
    public void MinimalHashSetShouldHandleCustomComparer()
    {
        EqualityComparer<int> customComparer = EqualityComparer<int>.Create((x, y) => x % 2 == y % 3, x => int.RotateLeft(x, 2) * 0xFF951F3);
        MinimalHashSet<int> minimalHashSet = new MinimalHashSet<int>(input, customComparer);
        HashSet<int> hashSet = new HashSet<int>(input, customComparer);
        
        bool[] hashSetResult = new bool[input.Length];
        bool[] minimalHashSetResult = new bool[input.Length];
        
        for (int i = 0; i < input.Length; i++)
        {
            hashSetResult[i] = hashSet.Contains(input[i]);
            minimalHashSetResult[i] = minimalHashSet.Contains(input[i]);
        }
        
        hashSetResult.ShouldBeEquivalentTo(minimalHashSetResult);
    }
}