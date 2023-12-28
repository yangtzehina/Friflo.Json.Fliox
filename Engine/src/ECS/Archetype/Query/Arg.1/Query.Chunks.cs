﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static Friflo.Engine.ECS.StructInfo;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct QueryChunks<T1>  // : IEnumerable <>  // <- not implemented to avoid boxing
    where T1 : struct, IComponent
{
    private readonly ArchetypeQuery<T1> query;

    public  override string         ToString() => query.signatureIndexes.GetString("Chunks: ");

    internal QueryChunks(ArchetypeQuery<T1> query) {
        this.query = query;
    }
    
    public ChunkEnumerator<T1> GetEnumerator() => new (query);
}

public ref struct ChunkEnumerator<T1>
    where T1 : struct, IComponent
{
    private readonly    T1[]                    copyT1;
    private readonly    int                     structIndex1;
    
    private readonly    ReadOnlySpan<Archetype> archetypes;
    private             int                     archetypePos;
    
    private             StructChunk<T1>[]       chunks1;
    private             Chunk<T1>               chunk1;
    private             int                     chunkPos;
    private             int                     chunkEnd;
    
    
    internal  ChunkEnumerator(ArchetypeQuery<T1> query)
    {
        copyT1          = query.copyT1;
        structIndex1    = query.signatureIndexes.T1;
        archetypes      = query.GetArchetypes();
        archetypePos    = 0;
        var archetype   = archetypes[0];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunkEnd        = archetype.ChunkCount();
    }
    
    /// <summary>return Current by reference to avoid struct copy and enable mutation in library</summary>
    public readonly Chunk<T1> Current   => chunk1;
    
    // --- IEnumerator
    public bool MoveNext()
    {
        int componentLen;
        if (chunkPos < chunkEnd) {
            componentLen = ChunkSize;
            goto Next;
        }
        if (chunkPos == chunkEnd)  {
            componentLen    = archetypes[archetypePos].ChunkRest();
            if (componentLen > 0) {
                goto Next;
            }
        }
        if (archetypePos >= archetypes.Length - 1) {
            return false;
        }
        var archetype   = archetypes[++archetypePos];
        var heapMap     = archetype.heapMap;
        chunks1         = ((StructHeap<T1>)heapMap[structIndex1]).chunks;
        chunkPos        = 0;
        chunkEnd        = archetype.ChunkEnd();
        componentLen    = chunkEnd == 0 ? archetype.ChunkRest() : ChunkSize;
    Next:
        chunk1 = new Chunk<T1>(chunks1[chunkPos].components, copyT1, componentLen);
        chunkPos++;
        return true;  
    }
}