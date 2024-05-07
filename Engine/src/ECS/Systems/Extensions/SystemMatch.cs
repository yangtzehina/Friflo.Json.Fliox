﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    public struct SystemMatch
    {
        public      BaseSystem  System      => system;
        public      int         Depth       => depth;
        public      int         Count       => count;
        
        internal    int         parent;
        internal    int         count;
        internal    BaseSystem  system;
        internal    int         depth;
            
        public override string      ToString() => GetString(); 
        
        private string GetString() {
            if (system is SystemGroup) {
                return $"{System.Name} [{Count}] - Depth: {Depth}";
            }
            return $"{System.Name} - Depth: {Depth}";
        }
    }
}