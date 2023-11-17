﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable PossibleNullReferenceException
namespace Friflo.Fliox.Engine.ECS.Collections;

/// <summary>
/// Implements same interfaces as <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> to act as a replacement
/// for <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/> with generic type <see cref="ExplorerItem"/>.
/// </summary>
public sealed class ExplorerItem :
    IList<ExplorerItem>,
    IList,
    IReadOnlyList<ExplorerItem>,
    INotifyCollectionChanged
 // INotifyPropertyChanged                                                      not required. Implemented by ObservableCollection{T}
{
#region internal properties
    public              int         Id          => entity.Id;
    public              GameEntity  Entity      => entity;
    public              bool        IsRoot      => IsRootItem();
    public              bool        AllowDrag   => !IsRootItem();
    public              string      Name        { get => GetName(entity); set => SetName(entity, value); }
    
    public              bool        flag;       // todo remove
    
    public   override   string      ToString()  => entity.ToString();
    #endregion
    
#region internal fields
    internal readonly   GameEntity                          entity;
    internal readonly   ExplorerItemTree                    tree;
    internal            NotifyCollectionChangedEventHandler collectionChanged;
 // public  event       PropertyChangedEventHandler         PropertyChanged;    not required. Implemented by ObservableCollection{T}
    #endregion

#region constructor
    internal ExplorerItem (ExplorerItemTree tree, GameEntity entity) {
        this.tree   = tree      ?? throw new ArgumentNullException(nameof(tree));
        this.entity = entity    ?? throw new ArgumentNullException(nameof(entity));
    }
    #endregion

#region private methods
    private bool IsRootItem() {
        return tree.rootItem.entity == entity;
    }
    private static string GetName(GameEntity entity) {
        if (entity.HasName) {
            return entity.Name.value;
        }
        return "---";
    }
    
    private static void SetName(GameEntity entity, string value) {
        if (string.IsNullOrEmpty(value)) {
            entity.RemoveComponent<EntityName>();
            return;
        }
        entity.AddComponent(new EntityName(value));
    }
    
    private ExplorerItem GetChildByIndex(int index) {
        int childId = entity.GetChildNodeByIndex(index).Id;
        // Console.WriteLine($"GetChildByIndex {entity.Id} {index} - child {childId}");
        return tree.GetItemById(childId);
    }
    
    [ExcludeFromCodeCoverage]
    private void ClearChildEntities() {
        throw new NotImplementedException();
    }
    
    private void RemoveChildEntityAt(int index) {
        var child = entity.GetChildNodeByIndex(index).Entity;   // called by TreeDataGrid 
        entity.RemoveChild(child);  // todo add GameEntity.RemoveChild(int index)
    }
    
    // ReSharper disable twice UnusedParameter.Local
    [ExcludeFromCodeCoverage]
    private void ReplaceChildEntityAt(int index, ExplorerItem item) {
        throw new NotImplementedException();
    }
    
    private int GetChildIndex(ExplorerItem item) {
        return entity.GetChildIndex(item.entity.Id);
    }
    #endregion
    
// -------------------------------------- interface implementations --------------------------------------
#region INotifyCollectionChanged
    public event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add     => collectionChanged += value;
        remove  => collectionChanged -= value;
    }
    #endregion
    
#region IEnumerable<>
    public IEnumerator<ExplorerItem> GetEnumerator() {
        return new ExplorerItemEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return new ExplorerItemEnumerator(this);
    }
    #endregion

#region ICollection<>
    void ICollection<ExplorerItem>.Add(ExplorerItem item) {
        entity.AddChild(item.entity);                           // called by TreeDataGrid
    }

    [ExcludeFromCodeCoverage]
    void ICollection<ExplorerItem>.Clear() {
        ClearChildEntities();
    }

    bool ICollection<ExplorerItem>.Contains(ExplorerItem item) {
        return GetChildIndex(item) != - 1;
    }

    void ICollection<ExplorerItem>.CopyTo(ExplorerItem[] array, int arrayIndex) {
        var childIds = entity.ChildIds;
        for (int n = 0; n < childIds.Length; n++)
        {
            int id                  = childIds[n];
            array[n + arrayIndex]   = tree.GetItemById(id);            
        }
    }

    bool ICollection<ExplorerItem>.Remove(ExplorerItem item) {
        return entity.RemoveChild(item.entity);
    }

    int ICollection<ExplorerItem>.Count => entity.ChildCount;   // called by TreeDataGrid

    bool ICollection<ExplorerItem>.IsReadOnly => false;
    #endregion

#region IList<>
    int IList<ExplorerItem>.IndexOf(ExplorerItem item) {
        return GetChildIndex(item);
    }

    void IList<ExplorerItem>.Insert(int index, ExplorerItem item) {
        entity.InsertChild(index, item.entity);                 // called by TreeDataGrid (DRAG)
    }

    void IList<ExplorerItem>.RemoveAt(int index) {
        RemoveChildEntityAt(index);                             // called by TreeDataGrid (DRAG)
    }

    ExplorerItem IList<ExplorerItem>.this[int index] {
        get => GetChildByIndex(index);                          // called by TreeDataGrid
        [ExcludeFromCodeCoverage]
        set => ReplaceChildEntityAt(index, value);
    }

    #endregion
    
#region IReadOnlyCollection<>
    ExplorerItem    IReadOnlyList<ExplorerItem>.this[int index]   => GetChildByIndex(index);    // called by TreeDataGrid
    int             IReadOnlyCollection<ExplorerItem>.Count       => entity.ChildCount;         // called by TreeDataGrid
    #endregion
    
// ---------------------------------- crab interface implementations :) ----------------------------------
#region IList
    [ExcludeFromCodeCoverage]
    void IList.Clear()  {
        ClearChildEntities();
    }
    
    void IList.RemoveAt(int index) {
        RemoveChildEntityAt(index);
    }
    
    int IList.Add(object value) {
        var childEntity = ((ExplorerItem)value).entity;
        return entity.AddChild(childEntity);
    }

    object IList.this[int index] {
        get => GetChildByIndex(index);                          // called by TreeDataGrid
        [ExcludeFromCodeCoverage]
        set => ReplaceChildEntityAt(index, (ExplorerItem)value);
    }

    bool IList.Contains(object value) {
        return GetChildIndex((ExplorerItem)value) != -1;
    }

    int IList.IndexOf(object value) {
        return GetChildIndex((ExplorerItem)value);
    }

    void IList.Insert(int index, object item) {
        var childEntity = ((ExplorerItem)item).entity; 
        entity.InsertChild(index, childEntity);
    }

    void IList.Remove(object value) {
        int index = GetChildIndex((ExplorerItem)value);
        RemoveChildEntityAt(index);
    }
    
    bool    IList.IsFixedSize           => false;
    bool    IList.IsReadOnly            => false;
    #endregion
    
#region ICollection
    int     ICollection.Count           => entity.ChildCount;   // called by TreeDataGrid
    bool    ICollection.IsSynchronized  => false;
    object  ICollection.SyncRoot        => null!;
    
    void    ICollection.CopyTo(Array array, int index)
    {
        var childIds = entity.ChildIds;
        for (int n = 0; n < childIds.Length; n++)
        {
            int id      = childIds[n];
            var item    = tree.GetItemById(id);
            array.SetValue(item, n + index);
        }
    }
    #endregion
}