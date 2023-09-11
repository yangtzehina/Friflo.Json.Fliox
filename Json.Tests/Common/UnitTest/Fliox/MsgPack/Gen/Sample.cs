﻿// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo


// ReSharper disable once CheckNamespace

using System;
using Friflo.Json.Fliox.MsgPack;
using Friflo.Json.Tests.Common.UnitTest.Fliox.MsgPack;

namespace Gen.Friflo.Json.Tests.Common.UnitTest.Fliox.MsgPack {

    static class Gen_Sample
    {
        private const   int       _val          = 0;
        private const   ulong     _abcdefgh     = 0x6867_6665_6463_6261;
        private const   long      _x            = 0x0000_0000_0000_0078;
        private const   long      _child        = 0x0000_0064_6c69_6863;
        private static  byte[]    _x2           = new byte[] { (byte)'x' };

        public static void ReadMsg (this ref MsgReader reader, ref Sample obj)
        {
            if (!reader.ReadObject(out int len)) {
                obj = null;
                return;
            }
            if (obj == null) {
                obj = new Sample();
            } else {
                obj.x = 0;
            }
            while (len-- > 0) {
                switch (reader.ReadKey()) {
                    // case _val:      obj.val = reader.ReadInt32 (); break;
                    case _x:        obj.x = reader.ReadInt32 ();                    continue;
                    case _child:    reader.ReadMsg(ref obj.child) ;   continue;
                }
                reader.SkipTree();
            }
        }
        
        public static void ReadMsgLongMembers (ref MsgReader reader, ref Sample obj)
        {
            if (!reader.ReadObject(out int len)) {
                obj = null;
                return;
            }
            if (obj == null) {
                obj = new Sample();
            } else {
                obj.x = 0;
            }
            while (len-- > 0) {
                switch (reader.ReadKey()) {
                    case _x:        if (reader.KeyName.SequenceEqual(_x2) ) { obj.x = reader.ReadInt32 (); continue; } break;
                    case _child:    reader.ReadMsg(ref obj.child);           continue;
                }
                reader.SkipTree();
            }
        }

        public static void WriteMsg(this ref MsgWriter writer, ref Sample obj)
        {
            if (obj == null) {
                writer.WriteNull();
                return;
            }
            int map = writer.WriteMapFix();
            int count = 1;
            // writer.WriteInt32   (_val, obj.val);
            // writer.WriteInt32   (_x2, obj.x);
            
            writer.WriteKeyInt32   (1, _x, obj.x);
            // writer.WriteMapByte   (1, _x, (byte)obj.x);
            
            if (writer.AddKey(obj.child != null)) {
                writer.WriteKey (5, _child, ref count); 
                Gen_Child.WriteMsg (ref writer, ref obj.child);
            }
            writer.WriteMapFixCount(map, count); // write element count
        }
    }
}