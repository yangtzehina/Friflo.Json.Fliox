﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Friflo.Fliox.Engine.ECS;

namespace Friflo.Fliox.Editor.UI.Explorer;

public class ExplorerTree
{
    internal readonly   GameEntityStore                 store;
    internal readonly   Dictionary<int, ExplorerItem>   items;
    
    public ExplorerTree (GameEntityStore store)
    {
        this.store                  = store;
        items                       = new Dictionary<int, ExplorerItem>();
        // store.ChildNodesChanged    += ChildNodesChangedHandler;
    }
    
    // todo init in constructor?
    public void Init() {
        store.ChildNodesChanged += ChildNodesChangedHandler;
    }
    
    private void ChildNodesChangedHandler(object sender, in ChildNodesChangedArgs args)
    {
        var parent              = items[args.parentId];
        Console.WriteLine($"event: {args}       parent: {parent}");
        var collectionChanged   = parent.CollectionChanged;
        if (collectionChanged == null) {
            return;
        }
        var action          = (NotifyCollectionChangedAction)args.action;
        object child        = items[args.childId];
        /* switch (args.action) {
            case ChildNodesChangedAction.Add:
                // todo
                break;
            case ChildNodesChangedAction.Remove:
                items.Remove(args.childId);
                break;
        } */
        var collectionArgs  = new NotifyCollectionChangedEventArgs(action, child, args.childIndex);
        // NOTE:
        // Passing parent as NotifyCollectionChangedEventHandler.sender enables the Avalonia UI event handlers called
        // below to check if the change event (Add / Remove) is caused by the containing Control or other code.  
        collectionChanged(parent, collectionArgs);
    }
    
    internal ExplorerItem CreateExplorerItems(GameEntityStore store)
    {
        var root = store.StoreRoot;
        return ExplorerItem.CreateExplorerItems(this, root);
    }
    
    internal static readonly GameEntityStore TestStore = CreateTestStore(); 
    
    private static GameEntityStore CreateTestStore()
    {
        var store   = new GameEntityStore();
        var root =    CreateEntity(store, 1, "root");
        root.AddChild(CreateEntity(store, 2, "child 2"));
        root.AddChild(CreateEntity(store, 3, "child 3"));
        root.AddChild(CreateEntity(store, 4, "child 4"));
        root.AddChild(CreateEntity(store, 5, "child 5"));
        root.AddChild(CreateEntity(store, 6, "child 6"));
        store.SetStoreRoot(root);
        return store;
    }
    
    private static GameEntity CreateEntity(GameEntityStore store, int id, string name)
    {
        var entity = store.CreateEntity(id);
        entity.AddComponent(new EntityName(name));
        return entity;
    }
}