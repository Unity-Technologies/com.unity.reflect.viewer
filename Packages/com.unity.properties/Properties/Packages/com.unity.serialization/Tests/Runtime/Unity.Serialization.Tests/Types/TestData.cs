using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.Serialization.Tests
{
    [Flags]
    public enum EnumInt32Flags : int
    {
        None =   0,
        Value1 = 1,
        Value2 = 2,
        Value3 = 4,
        Value4 = 8
    }

    public enum UnorderedEnumInt32 : int
    {
        None = 0,
        Value1 = 1,
        Value4 = 4,
        Value2 = 2,
        Value3 = 3
    }

    public enum EnumUInt8 : byte
    {
        None = 0,
        Value1 = 1,
        Value2 = 2,
    }

    [GeneratePropertyBag]
    struct ClassWithNoFields
    {
    }

    [GeneratePropertyBag]
    struct StructWithNoFields
    {
    }

    [GeneratePropertyBag]
    class ClassWithPrimitives
    {
        public bool BoolValue;
        public sbyte Int8Value;
        public short Int16Value;
        public int Int32Value;
        public long Int64Value;
        public byte UInt8Value;
        public ushort UInt16Value;
        public uint UInt32Value;
        public ulong UInt64Value;
        public float Float32Value;
        public double Float64Value;
        public char CharValue;
        public string StringValue;
        public EnumInt32Flags EnumInt32Flags;
        public UnorderedEnumInt32 EnumInt32Unordered;
        public EnumUInt8 EnumUInt8;
    }
    
    [GeneratePropertyBag]
    struct StructWithPrimitives
    {
        public bool BoolValue;
        public sbyte Int8Value;
        public short Int16Value;
        public int Int32Value;
        public long Int64Value;
        public byte UInt8Value;
        public ushort UInt16Value;
        public uint UInt32Value;
        public ulong UInt64Value;
        public float Float32Value;
        public double Float64Value;
        public char CharValue;
        public string StringValue;
        public EnumInt32Flags EnumInt32Flags;
        public UnorderedEnumInt32 EnumInt32Unordered;
        public EnumUInt8 EnumUInt8;
    }

    [GeneratePropertyBag]
    class ClassWithNestedClass
    {
        public ClassWithPrimitives Container;
    }

    [GeneratePropertyBag]
    class ClassWithNestedStruct
    {
        public StructWithPrimitives Container;
    }

    [GeneratePropertyBag]
    struct StructWithNestedClass
    {
        public ClassWithPrimitives Container;
    }
    
    [GeneratePropertyBag]
    struct StructWithNestedStruct
    {
        public StructWithPrimitives Container;
    }

    [GeneratePropertyBag]
    class ClassWithNestedClassRecursive
    {
        public ClassWithNestedClassRecursive Container;
    }

    [GeneratePropertyBag]
    class ClassWithArrays
    {
        public int[] Int32Array;
        public ClassWithPrimitives[] ClassContainerArray;
        public StructWithPrimitives[] StructContainerArray;
    }

    [GeneratePropertyBag]
    class ClassWithLists
    {
        public List<int> Int32List;
        public List<ClassWithPrimitives> ClassContainerList;
        public List<StructWithPrimitives> StructContainerList;
        public List<List<int>> Int32ListList;
    }

    [GeneratePropertyBag]
    class ClassWithDictionaries
    {
        public Dictionary<string, int> DictionaryStringInt32;
    }
    
    [GeneratePropertyBag]
    class ClassWithPropertyPath
    {
        public PropertyPath Path = new PropertyPath("Path.To.Array[2].Element");
    }

    interface IContainerInterface
    {
        
    }

    abstract class ClassAbstract : IContainerInterface
    {
        public int AbstractInt32Value;
    }

    [GeneratePropertyBag]
    class ClassDerivedA : ClassAbstract
    {
        public int DerivedAInt32Value;
    }

    [GeneratePropertyBag]
    class ClassDerivedB : ClassAbstract
    {
        public int DerivedBInt32Value;
    }

    [GeneratePropertyBag]
    class ClassDerivedA1 : ClassDerivedA
    {
        public int DerivedA1Int32Value;
    }

    [GeneratePropertyBag]
    class ClassWithPolymorphicFields
    {
        public object ObjectValue;
        public IContainerInterface InterfaceValue;
        public ClassAbstract AbstractValue;
    }

    [GeneratePropertyBag]
    class ClassWithNullablePrimitives
    {
        public int? NullableInt32;
        public EnumUInt8? NullableEnumUInt8;
    }

    [GeneratePropertyBag]
    class ClassWithNullableContainers
    {
        public StructWithPrimitives? NullableStructWithPrimitives;
    }

    [GeneratePropertyBag]
    class ScriptableObjectWithPrimitives : ScriptableObject
    {
        public int Int32Value;
    }
    
    [GeneratePropertyBag]
    class ClassWithUnityObjects
    {
        public UnityEngine.Object ObjectValue;
        public UnityEngine.Texture2D Texture2DValue;
        public ScriptableObjectWithPrimitives ScriptableObjectValue;
    }

    [GeneratePropertyBag]
    class ClassWithLazyLoadReferences
    {
        public LazyLoadReference<UnityEngine.Object> ObjectValue;
        public LazyLoadReference<UnityEngine.Texture2D> Texture2DValue;
        public LazyLoadReference<ScriptableObjectWithPrimitives> ScriptableObjectValue;
    }

    [GeneratePropertyBag]
    class ClassWithNonSerializeFields
    {
        public int A;
        public int B;
        [NonSerialized] public int C;
    }

    [GeneratePropertyBag]
    class ClassWithMultidimensionalArray
    {
        public int[,] MultidimensionalArrayInt32;
    }
}