using System.Buffers;
using System.Runtime.CompilerServices;

namespace MyHashSet.MinimalHashSet;

public struct MinimalHashSet<T> : IDisposable
{
    private const double LoadFactor = 0.75;
    
    private Entry[] _entries;
    private int[] _buckets;
    private uint _capacity;
    private int _startingIdx;
    private int _threshold;
    private readonly EqualityComparer<T> _comparer;

    public MinimalHashSet()
    {
        _comparer = EqualityComparer<T>.Default;
        Construct(0);
    }

    public MinimalHashSet(EqualityComparer<T>? comparer)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        Construct(0);
    }
    
    public MinimalHashSet(int count, EqualityComparer<T>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
        Construct((uint)count);
    }

    public MinimalHashSet(T[] collection, EqualityComparer<T>? comparer = null)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
        
        _comparer = comparer ?? EqualityComparer<T>.Default;
        
        Construct((uint)collection.Length);
        
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

        uint hashCode = InternalGetHashCode(ref item);
        ref int bucket = ref _buckets[GetIndex(ref hashCode)];
        int i = bucket - 1;
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
    
    public bool Add(T item)
    {
        if (_buckets == null)
        {
            Construct(0);
        }

        uint hashCode = InternalGetHashCode(ref item);
        ref int bucket = ref _buckets[GetIndex(ref hashCode)];
        int i = bucket - 1;
        uint collisions = 0;

        while (i >= 0)
        {
            ref Entry entry = ref _entries[i];
            if (entry.Hash == hashCode && _comparer.Equals(entry.Value, item))
            {
                return false;
            }

            i = entry.Next;
            collisions++;

            if (collisions > _entries.Length)
            {
                throw new InvalidOperationException("Concurrent operations not supported.");
            }
        }
        
        if (_startingIdx > _threshold)
        {
            Resize();
            bucket = ref _buckets[GetIndex(ref hashCode)];
        }

        ref Entry newEntry = ref _entries[_startingIdx];
        newEntry.Hash = hashCode;
        newEntry.Value = item;
        newEntry.Next = bucket - 1;
        bucket = _startingIdx + 1;
        _startingIdx++;
        
        return true;
    }

    private void Resize()
    {
        uint newSize = System.Numerics.BitOperations.RoundUpToPowerOf2((uint)_entries.Length * 2);
        _capacity = newSize;
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
                ref int bucket = ref newBuckets[bucketIdx];
                entry.Next = bucket - 1;
                bucket = i + 1;
            }
        }
        
        ArrayPool<Entry>.Shared.Return(_entries, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<Entry>());
        ArrayPool<int>.Shared.Return(_buckets);
        _entries = newEntries;
        _buckets = newBuckets;
    }

    private void Construct(uint? count)
    {
        if (count == null || count < 16)
        {
            count = Math.Max(System.Numerics.BitOperations.RoundUpToPowerOf2(count ?? 0), 16);
        }

        _capacity = count.Value;
        _threshold = (int)(count * LoadFactor);
        _entries = ArrayPool<Entry>.Shared.Rent((int)count);
        _buckets = ArrayPool<int>.Shared.Rent((int)count);
        Array.Clear(_buckets);
    }
    
    private int GetIndex(ref uint hashCode)
    {
        return (int)(hashCode % (_capacity - 1));
    }

    private uint InternalGetHashCode(ref T item)
    {
        if (item == null)
        {
            return 0;
        }

        return (uint)(_comparer.GetHashCode(item) & uint.MaxValue);
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