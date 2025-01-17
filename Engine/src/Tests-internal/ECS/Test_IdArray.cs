﻿using System;
using System.Diagnostics;
using Friflo.Engine.ECS.Collections;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

#pragma warning disable CA1861

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable InconsistentNaming
namespace Internal.ECS
{

    public class Test_IdArray
    {
        [Test]
        public void Test_IdArray_Add()
        {
            var heap    = new IdArrayHeap();
            
            var array   = new IdArray();
            AreEqual("count: 0", array.ToString());
            AreEqual(0, array.Count);
            AreEqual("{ }", array.GetIdSpan(heap).Debug());

            array.AddId(100, heap);
            AreEqual(1, array.Count);
            AreEqual("count: 1  id: 100", array.ToString());
            var span = array.GetIdSpan(heap);
            AreEqual("{ 100 }", span.Debug());
            AreEqual(0, heap.Count);
            
            array.AddId(101, heap);
            AreEqual(2, array.Count);
            AreEqual("count: 2  index: 1  start: 0", array.ToString());
            AreEqual("{ 100, 101 }", array.GetIdSpan(heap).Debug());
            AreEqual(1, heap.Count);

            array.AddId(102, heap);
            AreEqual(3, array.Count);
            AreEqual("{ 100, 101, 102 }", array.GetIdSpan(heap).Debug());
            AreEqual(1, heap.Count);
            
            array.AddId(103, heap);
            AreEqual(4, array.Count);
            AreEqual("{ 100, 101, 102, 103 }", array.GetIdSpan(heap).Debug());
            AreEqual(1, heap.Count);
            
            array.AddId(104, heap);
            AreEqual(5, array.Count);
            AreEqual("{ 100, 101, 102, 103, 104 }", array.GetIdSpan(heap).Debug());
            AreEqual(1, heap.Count);
            AreEqual("count: 1", heap.ToString());
            
            AreEqual("arraySize: 2 count: 0", heap.GetPool(1).ToString());
            AreEqual("arraySize: 4 count: 0", heap.GetPool(2).ToString());
            AreEqual("arraySize: 8 count: 1", heap.GetPool(3).ToString());
        }
        
        [Test]
        public void Test_IdArray_Remove()
        {
            var heap    = new IdArrayHeap();
            {
                var array   = new IdArray();
                array.AddId(100, heap);
                array.RemoveAt(0, heap);
                AreEqual(0, array.Count);
            } {
                var array   = new IdArray();
                array.AddId(200, heap);
                array.AddId(201, heap);
                array.RemoveAt(0, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual("{ 201 }", ids.Debug());
                AreEqual(0, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(300, heap);
                array.AddId(301, heap);
                array.RemoveAt(1, heap);
                AreEqual(1, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual("{ 300 }", ids.Debug());
                AreEqual(0, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(400, heap);
                array.AddId(401, heap);
                array.AddId(402, heap);
                array.RemoveAt(0, heap);
                AreEqual(2, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual("{ 401, 402 }", ids.Debug());
                AreEqual(1, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(500, heap);
                array.AddId(501, heap);
                array.AddId(502, heap);
                array.RemoveAt(2, heap);
                AreEqual(2, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual("{ 500, 501 }", ids.Debug());
                AreEqual(2, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(600, heap);
                array.AddId(601, heap);
                array.AddId(602, heap);
                array.AddId(603, heap);
                array.RemoveAt(0, heap);
                AreEqual(3, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual("{ 603, 601, 602 }", ids.Debug());
                AreEqual(3, heap.Count);
            } {
                var array   = new IdArray();
                array.AddId(700, heap);
                array.AddId(701, heap);
                array.AddId(702, heap);
                array.AddId(703, heap);
                array.RemoveAt(3, heap);
                AreEqual(3, array.Count);
                var ids     = array.GetIdSpan(heap);
                AreEqual("{ 700, 701, 702 }", ids.Debug());
                AreEqual(4, heap.Count);
            }
        }
        
        [Test]
        public void Test_IdArray_clear_freeList()
        {
            var heap    = new IdArrayHeap();
            var arrays  = new IdArray[100];
            int id      = 0;
            for (int n = 0; n < 100; n++) {
                ref var array = ref arrays[n]; 
                for (int i = 0; i < 10; i++) {
                    array.AddId(id++, heap);
                }
            }
            id = 0;
            for (int n = 0; n < 100; n++) {
                var span = arrays[n].GetIdSpan(heap);
                for (int i = 0; i < 10; i++) {
                    Mem.AreEqual(id++, span[i]);
                }
            }
            var pool16 = heap.GetPool(4);
            for (int n = 0; n < 100; n++) {
                Mem.AreEqual(n, pool16.FreeCount);
                ref var array = ref arrays[n]; 
                for (int i = 0; i < 10; i++) {
                    array.RemoveAt(array.Count - 1, heap);
                }
            }
            AreEqual(0, pool16.FreeCount);
        }
        
        [Test]
        public void Test_IdArray_exceptions()
        {
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            Throws<IndexOutOfRangeException>(() => {
                array.RemoveAt(0, heap);    
            });
            Throws<IndexOutOfRangeException>(() => {
                array.RemoveAt(-1, heap);    
            });
        }
        
        [Test]
        public void Test_IdArray_Add_Remove_One_Perf()
        {
            int repeat  = 100; // 100_000_000;
            //  #PC: Add_Remove_One: repeat: 100000000 duration: 732 ms
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < repeat; i++) {
                array.AddId(42, heap);
                array.RemoveAt(0, heap);
            }
            Console.WriteLine($"Add_Remove_One: repeat: {repeat} duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(0, heap.Count);
        }
        
        [Test]
        public void Test_IdArray_Add_Remove_Many_Perf()
        {
            int count   = 100; // 1_000_000
            int repeat  = 100;
            //  #PC: Add_Remove_Many_Perf - count: 1000000 repeat: 100 duration: 1472 ms
            var heap    = new IdArrayHeap();
            var array   = new IdArray();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < repeat; i++) {
                for (int n = 0; n < count; n++) {
                    array.AddId(n, heap);
                }
                for (int n = 0; n < count; n++) {
                    array.RemoveAt(array.Count - 1, heap);
                }
            }
            Console.WriteLine($"Add_Remove_Many_Perf - count: {count} repeat: {repeat} duration: {sw.ElapsedMilliseconds} ms");
            AreEqual(0, heap.Count);
        }
        
        [Test]
        public unsafe void Test_IdArray_size_of()
        {
            AreEqual(8, sizeof(IdArray));
        }
        
        [Test]
        public void Test_IdArray_LeadingZeroCount()
        {
            for (uint n = 0; n < 1000; n++) {
                AreEqual(System.Numerics.BitOperations.LeadingZeroCount(n), IdArrayHeap.LeadingZeroCount(n));    
            }
            for (uint n = int.MaxValue - 1000; n < int.MaxValue; n++) {
                AreEqual(System.Numerics.BitOperations.LeadingZeroCount(n), IdArrayHeap.LeadingZeroCount(n));    
            }
        }
        

    }
}