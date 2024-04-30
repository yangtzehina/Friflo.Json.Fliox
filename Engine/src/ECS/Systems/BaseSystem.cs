﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Friflo.Json.Fliox;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;
// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertToAutoProperty
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    public abstract class BaseSystem
    {
    #region properties
        [Browse(Never)]         public virtual  string          Name        => systemName;
        [Browse(Never)]         public          SystemRoot      SystemRoot  => systemRoot;
        [Browse(Never)]         public          SystemGroup     ParentGroup => parentGroup;
        [Browse(Never)][Ignore] public          bool            Enabled     { get => enabled; set => enabled = value; }
        [Browse(Never)]         public          int             Id          => id;
        #endregion
            
    #region fields
        [Ignore]    [Browse(Never)] public              Tick        Tick;
        [Serialize] [Browse(Never)] internal            int         id;
        [Serialize] [Browse(Never)] private             bool        enabled = true;
                    [Browse(Never)] private readonly    string      systemName;
                    [Browse(Never)] internal            SystemGroup parentGroup;
                    [Browse(Never)] private             SystemRoot  systemRoot;
         #endregion
         
    #region constructors
        protected BaseSystem() {
            systemName = GetType().Name;
        }
        #endregion
        
    #region system events
        public event Action<SystemChanged>  OnSystemChanged;

        public void CastSystemUpdate(string field, object value)
        {
            var system  = this;
            var change  = new SystemChanged(SystemChangedAction.Update, system, field, value);
            var root    = system.SystemRoot;
            if (root != system) {
                system.OnSystemChanged?.Invoke(change);
            }
            root?.OnSystemChanged?.Invoke(change);
        }
        
        internal static void CastSystemChanged(BaseSystem system, SystemChangedAction action)
        {
            var change  = new SystemChanged(action, system, null, null);
            var root    = system.SystemRoot;
            var parent  = system.parentGroup;
            if (root != parent) {
                parent.OnSystemChanged?.Invoke(change);
            }
            root?.OnSystemChanged?.Invoke(change);
        }
        
        private static void CastSystemMoved(BaseSystem system, SystemGroup oldParent)
        {
            var change  = new SystemChanged(SystemChangedAction.Move, system, null, oldParent);
            var root    = system.SystemRoot;
            var parent  = system.parentGroup;
            if (parent == oldParent) {
                parent.OnSystemChanged?.Invoke(change);
            } else {
                oldParent.OnSystemChanged?.Invoke(change);
                parent.   OnSystemChanged?.Invoke(change);
            }
            if (root != parent && root != oldParent) {
                root?.OnSystemChanged?.Invoke(change);
            }
        }
        
        internal static void CastSystemRemoved(BaseSystem system, SystemRoot oldRoot, SystemGroup oldParent)
        {
            var change  = new SystemChanged(SystemChangedAction.Remove, system, null, oldParent);
            if (oldRoot != oldParent) {
                oldParent.OnSystemChanged?.Invoke(change);
            }
            oldRoot?.OnSystemChanged?.Invoke(change);
        }
        #endregion

    #region virtual - store: add / remove
        internal           virtual  void RemoveStoreInternal(EntityStore store) { }
        internal           virtual  void AddStoreInternal   (EntityStore store) { }
        #endregion
        
    #region virtual - system: update
        public             virtual void Update(Tick tick) { }
        
        /// <summary>
        /// Called for every system in <see cref="SystemGroup.ChildSystems"/> before group <see cref="SystemGroup.Update"/>.
        /// </summary>
        protected internal virtual  void OnUpdateGroupBegin () { }
        
        /// <summary>
        /// Called for every system in <see cref="SystemGroup.ChildSystems"/> after group <see cref="SystemGroup.Update"/>.
        /// </summary>
        protected internal virtual  void OnUpdateGroupEnd   () { }
        #endregion
        
    #region system: move
        public int MoveSystemTo(SystemGroup targetGroup, int index)
        {
            if (targetGroup == null)                                    throw new ArgumentNullException(nameof(targetGroup));
            if (parentGroup == null)                                    throw new InvalidOperationException($"System '{Name}' has no parent");
            if (index < -1 || index > targetGroup.childSystems.Count)   throw new ArgumentException($"invalid index: {index}");
            if (parentGroup == targetGroup) {
                // case:    Change system position within its parent  
                var oldIndex = targetGroup.childSystems.Remove(this);
                if (index == -1) {
                    targetGroup.childSystems.Add(this);
                    index = targetGroup.childSystems.Count - 1;
                } else {
                    if (index > oldIndex) index--;
                    targetGroup.childSystems.InsertAt(index, this);
                }
                // Send event. See: SEND_EVENT notes
                CastSystemMoved(this, targetGroup); 
                return index;
            }
            if (systemRoot != targetGroup.systemRoot) {
                var expect = systemRoot?.Name;
                var msg = $"Expect {nameof(targetGroup)} == {nameof(SystemRoot)}. Expected: '{expect}' was: '{targetGroup.Name}'";
                throw new InvalidOperationException(msg);
            }
            // case:    System moved to another parent group  
            var oldParent = parentGroup;
            oldParent.childSystems.Remove(this);
            if (index == -1) {
                targetGroup.childSystems.Add(this);
                index = targetGroup.childSystems.Count - 1;
            } else {
                targetGroup.childSystems.InsertAt(index, this);
            }
            parentGroup = targetGroup;
            // Send event. See: SEND_EVENT notes
            CastSystemMoved(this, oldParent);
            return index;
        }
        #endregion
        
    #region set parent and root
        internal void SetRoot(SystemRoot root)
        {
            systemRoot = root;
        }
        
        internal void SetParentAndRoot(SystemGroup group)
        {
            parentGroup     = group;
            var newRoot     = group.systemRoot;
            var currentRoot = systemRoot;
            if (currentRoot == newRoot) {
                return;
            }
            if (newRoot == null) {
                return;
            }
            var rootSystems = GetSubSystems(ref newRoot.systemBuffer);
            // add stores of SystemRoot to all root Systems
            foreach (var system in rootSystems) {
                system.systemRoot = newRoot;
                newRoot.AddSystemToRoot(system);
                foreach (var store in newRoot.stores) {
                    system.AddStoreInternal(store);
                }
            }
        }
        
        internal void ClearParentAndRoot()
        { 
            parentGroup         = null;
            var currentRoot     = systemRoot;
            if (currentRoot == null) {
                return;
            }
            var rootSystems = GetSubSystems(ref currentRoot.systemBuffer);
            // remove stores of SystemRoot from all root Systems
            foreach (var system in rootSystems) {
                system.systemRoot = null;
                currentRoot.RemoveSystemFromRoot(system);
                foreach (var store in currentRoot.stores) {
                    system.RemoveStoreInternal(store);
                }
            }
        }
        #endregion
        
    #region get systems of systems sub tree
        internal Array<BaseSystem> GetSubSystems(ref Array<BaseSystem> systemBuffer)
        {
            systemBuffer.Clear();
            AddSubSystems(ref systemBuffer, this);
            return systemBuffer;
        }
        
        private static void AddSubSystems(ref Array<BaseSystem> array, BaseSystem system)
        {
            array.Add(system);
            if (system is SystemGroup systemGroup) {
                foreach (var child in systemGroup.childSystems) {
                    AddSubSystems(ref array, child);
                }
            }
        }
        #endregion
    }
    
    internal sealed class View
    {
        public  Tick                Tick            => system.Tick;
        public  int                 Id              => system.Id;
        public  bool                Enabled         => system.Enabled;
        public  string              Name            => system.Name;
        public  SystemRoot          SystemRoot      => system.SystemRoot;
        public  SystemGroup         ParentGroup     => system.ParentGroup;

        public override string      ToString()      => $"Enabled: {Enabled}  Id: {Id}";

        [Browse(Never)] private readonly BaseSystem   system;
        
        internal View(BaseSystem system) {
            this.system = system;
        }
    }
}