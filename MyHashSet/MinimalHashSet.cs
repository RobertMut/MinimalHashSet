using System.Buffers;
using System.Runtime.CompilerServices;

namespace MyHashSet.MinimalHashSet;

public ref struct MinimalHashSet<T> : IDisposable
{
    private const double LoadFactor = 0.75;
    
    private Entry[] _entries;
    private int[] _buckets;
    private int _capacity;
    private int _startingIdx;
    private int _threshold;
    private EqualityComparer<T> _comparer;

    public MinimalHashSet()
    {
        _comparer = EqualityComparer<T>.Default;
        Construct(0);
    }
    
    public MinimalHashSet(EqualityComparer<T>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        Construct(0);
    }
    
    public MinimalHashSet(int count, EqualityComparer<T>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        Construct(count);
    }

    public MinimalHashSet(T[] collection, EqualityComparer<T>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        
        Construct(collection.Length);
        
        for (int i = 0; i < collection.Length; i++)
        {
            Add(collection[i]);
        }
    }
    
    public bool Contains(T item)
    {
        if (_buckets == null)
        {
            return false;
        }

        return Lookup(item, out _);
    }
    
    public bool Add(T item)
    {
        if (_buckets == null)
        {
            Initialize(0);
        }

        bool isCollisionPresent = Lookup(item, out uint hashCode);

        if (isCollisionPresent)
        {
            return false;
        }
        
        ref int bucket = ref _buckets[GetIndex(ref hashCode)];
        if (_startingIdx > _threshold)
        {
            Resize();
            bucket = ref _buckets[GetIndex(ref hashCode)];
        }

        ref var newEntry = ref _entries[_startingIdx];
        newEntry.Hash = hashCode;
        newEntry.Value = item;
        newEntry.Next = bucket - 1;
        bucket = _startingIdx + 1;
        _startingIdx++;
        
        return true;
    }

    private void Initialize(int size)
    {
        if (size < 16)
        {
            size = 16;
        }

        _capacity = size;
        _entries = ArrayPool<Entry>.Shared.Rent(size);
        _buckets = ArrayPool<int>.Shared.Rent(size);
    }

    private void Resize()
    {
        uint newSize = System.Numerics.BitOperations.RoundUpToPowerOf2((uint)_entries.Length * 2);
        _capacity = (int)newSize;
        _threshold = (int)(newSize * LoadFactor);

        Entry[] newEntries = ArrayPool<Entry>.Shared.Rent((int)newSize);
        int[] newBuckets = ArrayPool<int>.Shared.Rent((int)newSize);
        Array.Clear(newBuckets); //same as AsSpan().Clear()

        _entries.AsSpan().CopyTo(newEntries);
        
        for(int i =0; i < _startingIdx; i++)
        {
            ref Entry entry = ref newEntries[i];

            if (entry.Next >= -1)
            {
                int bucketIdx = GetIndex(ref entry.Hash);
                ref var bucket = ref newBuckets[bucketIdx];
                entry.Next = bucket - 1;
                bucket = i + 1;
            }
        }
        
        ArrayPool<Entry>.Shared.Return(_entries, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<Entry>());
        ArrayPool<int>.Shared.Return(_buckets);
        _entries = newEntries;
        _buckets = newBuckets;
    }

    private void Construct(int count)
    {
        if (count == null || count < 16)
        {
            count = Math.Max((int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)count), 16);
        }

        _capacity = count;
        _threshold = (int)(count * LoadFactor);
        _entries = ArrayPool<Entry>.Shared.Rent(count);
        _buckets = ArrayPool<int>.Shared.Rent(count);
        Array.Clear(_buckets);
    }
    
    private bool Lookup(T item, out uint hashCode)
    {
        hashCode = InternalGetHashCode(item);
        ref int bucket = ref _buckets[GetIndex(ref hashCode)];
        var i = bucket - 1;
        uint collisions = 0;

        while (i >= 0)
        {
            ref Entry entry = ref _entries[i];
            if (entry.Hash == hashCode && _comparer.Equals(entry.Value, item))
            {
                return true;
            }

            i = entry.Next;
            collisions++;

            if (collisions > _entries.Length)
            {
                throw new InvalidOperationException("Concurrent operations not supported.");
            }
        }

        return false;
    }
    
    private int GetIndex(ref uint hashCode)
    {
        return (int)(hashCode % (_capacity - 1));
    }

    private uint InternalGetHashCode(T item)
    {
        if (item == null)
        {
            return 0;
        }

        return (uint)(_comparer.GetHashCode(item) & int.MaxValue);
    }


    
    struct Entry
    {
        public uint Hash;
        public T Value;
        public int Next;
    }

    public void Dispose()
    {
        if (_buckets != null)
        {
            ArrayPool<int>.Shared.Return(_buckets);
            _buckets = null!;
        }

        if (_entries != null)
        {
            ArrayPool<Entry>.Shared.Return(_entries);
            _entries = null!;
        }
    }
}