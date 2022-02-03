using System;
using NUnit.Framework;

namespace Unity.Properties.Editor.Tests
{
    struct Difficulty
    {
        public struct Easy
        {
            public struct Medium
            {
                public struct Hard {}
                
                public struct Hard<T> {}
            }
            
            public struct Medium<T1>
            {
                public struct Hard {}
                
                public struct Hard<T2> {}
            }

            public struct Medium<T1, T2>
            {
                public struct Hard<T3, T4> {}
            }
        }
        
        public struct Easy<T1>
        {
            public struct Medium
            {
                public struct Hard {}
                
                public struct Hard<T2> {}
            }
            
            public struct Medium<T2>
            {
                public struct Hard {}
                
                public struct Hard<T3> {}
            }
        }
        
        public class CustomNames<TFirst, TSecond, TRandom, TType>{}
        
        public class NestedCustomNames<T1, T2> : CustomNames<int, T1, float, T2>{}
        
        public interface IBaseInterface
        {
        }
        
        public interface IDerivedInterface : IBaseInterface
        {
        }
    }

    [TestFixture]
    class TypeUtilityTests
    {
        class RootType{}
        class ChildrenType : RootType{}
        class ChildrenType<T> : ChildrenType{}
        
        class RootType<T>{}
        class OtherChildrenType : RootType<int>{}
        class OtherChildrenType<T> : RootType<T>{}
        class OtherChildrenType<T1, T2> : RootType<T2>{}
        class DeepNested<T1, T2, T3> : OtherChildrenType<T3, T1>{}
        
        [TestCase(typeof(Difficulty), "Difficulty")]
        [TestCase(typeof(Difficulty.Easy), "Difficulty.Easy")]
        [TestCase(typeof(Difficulty.Easy<int>), "Difficulty.Easy<int>")]
        [TestCase(typeof(Difficulty.Easy<Difficulty.Easy>), "Difficulty.Easy<Difficulty.Easy>")]
        [TestCase(typeof(Difficulty.Easy<Difficulty.Easy<int>>), "Difficulty.Easy<Difficulty.Easy<int>>")]
        [TestCase(typeof(Difficulty.Easy<int>.Medium), "Difficulty.Easy<int>.Medium")]
        [TestCase(typeof(Difficulty.Easy<int>.Medium<float>), "Difficulty.Easy<int>.Medium<float>")]
        [TestCase(typeof(Difficulty.Easy<int>.Medium<float>.Hard), "Difficulty.Easy<int>.Medium<float>.Hard")]
        [TestCase(typeof(Difficulty.Easy<int>.Medium<float>.Hard<int>), "Difficulty.Easy<int>.Medium<float>.Hard<int>")]
        [TestCase(typeof(Difficulty.Easy.Medium), "Difficulty.Easy.Medium")]
        [TestCase(typeof(Difficulty.Easy.Medium.Hard), "Difficulty.Easy.Medium.Hard")]
        [TestCase(typeof(Difficulty.Easy.Medium.Hard<int>), "Difficulty.Easy.Medium.Hard<int>")]
        [TestCase(typeof(Difficulty.Easy.Medium<float>.Hard<int>), "Difficulty.Easy.Medium<float>.Hard<int>")]
        [TestCase(typeof(Difficulty.Easy<Difficulty.Easy.Medium<float>.Hard<int>>.Medium<Difficulty.Easy.Medium.Hard<int>>.Hard<Difficulty.Easy<Difficulty.Easy<int>>>), "Difficulty.Easy<Difficulty.Easy.Medium<float>.Hard<int>>.Medium<Difficulty.Easy.Medium.Hard<int>>.Hard<Difficulty.Easy<Difficulty.Easy<int>>>")]
        [TestCase(typeof(Difficulty.Easy.Medium<float, char>.Hard<char, float>), "Difficulty.Easy.Medium<float, char>.Hard<char, float>")]
        [TestCase(typeof(Difficulty.Easy<>), "Difficulty.Easy<T1>")]
        [TestCase(typeof(Difficulty.Easy.Medium<>), "Difficulty.Easy.Medium<T1>")]
        [TestCase(typeof(Difficulty.Easy.Medium.Hard<>), "Difficulty.Easy.Medium.Hard<T>")]
        [TestCase(typeof(Difficulty.Easy.Medium<>.Hard<>), "Difficulty.Easy.Medium<T1>.Hard<T2>")]
        [TestCase(typeof(Difficulty.Easy<>.Medium<>.Hard<>), "Difficulty.Easy<T1>.Medium<T2>.Hard<T3>")]
        [TestCase(typeof(Difficulty.CustomNames<,,,>), "Difficulty.CustomNames<TFirst, TSecond, TRandom, TType>")]
        [TestCase(typeof(Difficulty.NestedCustomNames<string, char>), "Difficulty.NestedCustomNames<string, char>")]
        [TestCase(typeof(Difficulty.NestedCustomNames<,>), "Difficulty.NestedCustomNames<T1, T2>")]
        public void ResolvingATypeName_GivenAnyType_ReturnsExpectedName(Type type, string expected)
        {
            Assert.That(TypeUtility.GetTypeDisplayName(type), Is.EqualTo(expected));
            // Purposefully calling it twice for test stability: This is not a cheap operation and the result may be cached.
            Assert.That(TypeUtility.GetTypeDisplayName(type), Is.EqualTo(expected));
        }

        [TestCase(typeof(Difficulty), typeof(Difficulty))]
        [TestCase(typeof(RootType), typeof(RootType))]
        [TestCase(typeof(ChildrenType), typeof(RootType))]
        [TestCase(typeof(ChildrenType<int>), typeof(RootType))]
        [TestCase(typeof(ChildrenType<>), typeof(RootType))]
        
        [TestCase(typeof(RootType<int>), typeof(RootType<int>))]
        [TestCase(typeof(OtherChildrenType), typeof(RootType<int>))]
        [TestCase(typeof(OtherChildrenType<int>), typeof(RootType<int>))]
        [TestCase(typeof(DeepNested<char, float, int>), typeof(RootType<char>))]
        public void GettingTheBaseType_GivenAnyType_ReturnsExpectedType(Type type, Type root)
        {
            Assert.That(TypeUtility.GetRootType(type), Is.EqualTo(root));
        }

        [TestCase(typeof(Difficulty.IBaseInterface))]
        [TestCase(typeof(Difficulty.IDerivedInterface))]
        public void GettingTheBaseType_GivenAnInterface_ReturnsNull(Type type)
        {
            Assert.That(TypeUtility.GetRootType(type), Is.EqualTo(null));
        }
    }
}