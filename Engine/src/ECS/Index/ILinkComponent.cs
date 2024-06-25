﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Is used to define a component type having a link to another <see cref="Entity"/>.
/// </summary>
/// <remarks>
/// This component type enables:
/// <list type="bullet">
///   <item>
///     Return all entities having a <see cref="ILinkComponent"/> to a specific entity.<br/>
///     See <see cref="IndexExtensions.GetLinkingEntities{TComponent}"/>
///   </item>
///   <item>
///     Return all entities linked by a specific <see cref="ILinkComponent"/> type.<br/>
///     See <see cref="EntityStore.GetLinkedEntities{TComponent}"/>
///   </item>
///   <item>
///     Filter entities in a query having a <see cref="ILinkComponent"/> to a specific entity.<br/>
///     See <see cref="ArchetypeQuery.HasValue{TComponent,TValue}"/>.
///   </item>
/// </list>
/// </remarks>
public interface ILinkComponent : IIndexedComponent<Entity> { }