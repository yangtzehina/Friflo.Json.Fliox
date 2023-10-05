using System.Collections.Generic;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_ComponentReader
{
    private static readonly JsonValue structComponents =
        new JsonValue("{ \"pos\": { \"x\": 1, \"y\": 2, \"z\": 3 }, \"scl3\": { \"x\": 4, \"y\": 5, \"z\": 6 } }");
    
    [Test]
    public static void Test_ReadStructComponents()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        
        var rootNode    = new DataNode { pid = 10, components = structComponents, children = new List<long> { 11 } };
        var childNode   = new DataNode { pid = 11 };
        
        var root        = store.CreateFromDataNode(rootNode);
        var child       = store.CreateFromDataNode(childNode);
        AssertRootEntity(root);
        var type = store.GetArchetype(Signature.Get<Position, Scale3>());
        AreEqual(1,     type.EntityCount);
        
        // --- read same DataNode again
        root.Position   = default;
        root.Scale3     = default;
        root            = store.CreateFromDataNode(rootNode);
        AssertRootEntity(root);
        AreEqual(1,     type.EntityCount);

        AreEqual(11,    child.Id);
    }
    
    private static void AssertRootEntity(GameEntity root) {
        AreEqual(1,     root.ChildCount);
        AreEqual(11,    root.ChildNodes.Ids[0]);
        AreEqual(2,     root.ComponentCount);
        AreEqual(1f,    root.Position.x);
        AreEqual(2f,    root.Position.y);
        AreEqual(3f,    root.Position.z);
        AreEqual(4f,    root.Scale3.x);
        AreEqual(5f,    root.Scale3.y);
        AreEqual(6f,    root.Scale3.z);
    }
    
    [Test]
    public static void Test_ReadStructComponents_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        
        var rootNode    = new DataNode { pid = 10, components = structComponents, children = new List<long> { 11 } };
        
        const int count = 10; // 1_000_000 ~ 2.639 ms (bottleneck parsing JSON to structs)
        for (int n = 0; n < count; n++)
        {
            var root = store.CreateFromDataNode(rootNode);
            root.DeleteEntity();
        }
    }
    
    private static readonly JsonValue classComponents = new JsonValue("{ \"testRef1\": { \"val1\": 2 } }");
    
    [Test]
    public static void Test_ReadClassComponents()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        
        var rootNode    = new DataNode { pid = 10, components = classComponents, children = new List<long> { 11 } };

        var root        = store.CreateFromDataNode(rootNode);
        AreEqual(1,     root.ClassComponents.Length);
        var comp1       = root.GetClassComponent<TestRefComponent1>();
        AreEqual(2,     comp1.val1);
        comp1.val1      = -1;
        
        // --- read same DataNode again
        store.CreateFromDataNode(rootNode);
        var comp2       = root.GetClassComponent<TestRefComponent1>();
        AreEqual(2,     comp2.val1);
        AreSame(comp1, comp2);
    }
    
    [Test]
    public static void Test_ReadClassComponents_Perf()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        
        var rootNode    = new DataNode { pid = 10, components = classComponents, children = new List<long> { 11 } };

        const int count = 10; // 5_000_000 ~ 3.528 ms
        for (int n = 0; n < count; n++) {
            store.CreateFromDataNode(rootNode);
        }
    }
    
    [Test]
    public static void Test_RegisterComponents()
    {
        var types   = EntityStore.GetComponentTypes();
        var structs = types.Structs;
        var classes = types.Classes;
        
        AreEqual("components - struct: 6 class: 3", types.ToString());
        AreEqual(7, structs.Length);
        AreEqual(4, classes.Length);
        
        IsNull(structs[0]);
        for (int n = 1; n < structs.Length; n++) {
            var type = structs[n];
            AreEqual(n, type.index);
            IsTrue  (type.isStructType);
            NotNull (type.componentKey);
            var typeHandle = type.type.TypeHandle.Value.ToInt64();
            AreEqual(typeHandle, type.structHash);

        }
        IsNull(classes[0]);
        for (int n = 1; n < classes.Length; n++) {
            var type = classes[n];
            AreEqual(n, type.index);
            IsFalse (type.isStructType);
            NotNull (type.componentKey);
            AreEqual(0, type.structHash);
        }
        
        var posType = types.GetComponentTypeByKey("pos");
        AreEqual(typeof(Position), posType.type);
        
        var testType = types.GetComponentTypeByKey("test");
        AreEqual(typeof(TestComponent), testType.type);
        
        var myComponentType = types.GetStructComponentType<MyComponent1>();
        AreEqual("my1", myComponentType.componentKey);
        
        var TestComponentType = types.GetClassComponentType<TestComponent>();
        AreEqual("test", TestComponentType.componentKey);
        
        AreEqual(typeof(Position),  types.ComponentTypeByKey["pos"].type);
        AreEqual("test",            types.ComponentTypeByType[typeof(TestComponent)].componentKey);
    }
}
