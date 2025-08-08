// See https://aka.ms/new-console-template for more information

using MyHashSet.MinimalHashSet;
using Shouldly;


int[] inputCollection = { 2, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36 };
int[] collection = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19};

var myHashSet = new MinimalHashSet<int>(inputCollection);
var original = new HashSet<int>(inputCollection);

bool[] myResult = new bool[collection.Length];
bool[] originalResult = new bool[collection.Length];

for (int i = 0; i < collection.Length; i++)
{
    myResult[i] = myHashSet.Contains(collection[i]);
    originalResult[i] = original.Contains(collection[i]);
}

myResult.ShouldBeEquivalentTo(originalResult);

Console.ReadKey();