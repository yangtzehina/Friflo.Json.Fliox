﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

[DebuggerTypeProxy(typeof(EntitiesDebugView))]
public readonly struct Entities : IReadOnlyList<Entity>
{
#region properties
    public              int             Count       => count;
    public              EntityStore     EntityStore => store;
    #endregion

#region interal fields
    internal readonly   int[]           ids;    //  8
    internal readonly   EntityStore     store;  //  8
    internal readonly   int             start;  //  4
    internal readonly   int             count;  //  4
    #endregion
    
#region general
    internal Entities(int[] ids, EntityStore store, int start, int count) {
        this.ids    = ids;
        this.store  = store;
        this.start  = start;
        this.count  = count;
    }
    
    public Entity this[int index] => new Entity(store, ids[start + index]);
    #endregion

    
#region IEnumerator
    public EntityEnumerator                 GetEnumerator() => new EntityEnumerator (this);
    
    // --- IEnumerable
    IEnumerator                 IEnumerable.GetEnumerator() => new EntityEnumerator (this);

    // --- IEnumerable<>
    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => new EntityEnumerator (this);
    #endregion
}


public struct EntityEnumerator : IEnumerator<Entity>
{
    private readonly    int[]       ids;        //  8
    private readonly    EntityStore store;      //  8
    private readonly    int         start;      //  4
    private readonly    int         last;       //  4
    private             int         index;      //  4
    
    internal EntityEnumerator(in Entities entities) {
        ids     = entities.ids;
        store   = entities.store;
        start   = entities.start - 1;
        last    = start + entities.count;
        index   = start;
    }
    
    // --- IEnumerator
    public          void         Reset()    => index = start;

    readonly object  IEnumerator.Current    => new Entity(store, ids[index]);

    public   Entity              Current    => new Entity(store, ids[index]);
    
    // --- IEnumerator
    public bool MoveNext()
    {
        if (index < last) {
            index++;
            return true;
        }
        return false;
    }
    
    public readonly void Dispose() { }
}

internal sealed class EntitiesDebugView
{
    [Browse(RootHidden)]
    internal            Entity[]    Entities => GetEntities();
    
    private readonly    Entities    entities;
    
    internal EntitiesDebugView(Entities entities) {
        this.entities = entities;
    }
    
    private Entity[] GetEntities()
    {
        var store   = entities.store;
        var count   = entities.count;
        var ids     = entities.ids;
        var result  = new Entity[count];
        for (int n = 0; n < count; n++) {
            result[n] = new Entity(store, ids[n]);
        }
        return result;
    }
} 