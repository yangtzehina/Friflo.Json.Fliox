﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public readonly struct  EntitiesChangedArgs
{
    /// <remarks>
    /// Use <see cref="EntityStore.GetEntityById"/> to get the <see cref="Entity"/>. E.g.<br/>
    /// <code>      var entity = store.GetEntityById(args.entityId);       </code>
    /// </remarks>
    public              IReadOnlySet<int>   EntityIds   => entityIds;
    
    private readonly    HashSet<int>        entityIds;  //  8
    
    public  override    string              ToString() => $"entities changed. Count: {entityIds.Count}";

    public EntitiesChangedArgs(HashSet<int> entityIds)
    {
        this.entityIds = entityIds;
    }
}