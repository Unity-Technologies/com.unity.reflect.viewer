using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
#pragma warning disable 649

namespace Unity.Properties.Reflection.Editor.Tests
{
    [TestFixture]
    class PropertyBagReflectionUtilityTests
    {
        static class NonAsyncNonRecursive
        {
            public interface IData
            {
            }

            public abstract class Data : IData
            {
            }

            public class GenericType<T1, T2>
            {
                public T1 t1;
                public T2 t2;
            }

            public struct Param1
            {
                public string Value;
            }

            public struct Param2
            {
                public string Value;
            }

            public struct Vec2
            {
                public float x;
                public float y;
            }

            public struct Vec3
            {
                public float x;
                public float y;
                public float z;
            }

            public struct InnerContainer
            {
                public Vec2 vec2;
            }

            public struct InnerContainer2
            {
                public Vec3 vec3;
            }

            public struct OuterContainer
            {
                public InnerContainer Inner;
                public InnerContainer2 Inner2;
                public InnerContainer SecondInner;
                public IData IData;
                public Data Data;
                public GenericType<Param1, Param2> GenericType;
            }
        }

        static class NonAsyncAndRecursive
        {
            public interface IData
            {
            }

            public abstract class Data : IData
            {
            }

            public class GenericType<T1, T2>
            {
                public T1 t1;
                public T2 t2;
            }

            public struct Param1
            {
                public string Value;
            }

            public struct Param2
            {
                public string Value;
            }

            public struct Vec2
            {
                public float x;
                public float y;
            }

            public struct Vec3
            {
                public float x;
                public float y;
                public float z;
            }

            public struct InnerContainer
            {
                public Vec2 vec2;
            }

            public struct InnerContainer2
            {
                public Vec3 vec3;
            }

            public struct OuterContainer
            {
                public InnerContainer Inner;
                public InnerContainer2 Inner2;
                public InnerContainer SecondInner;
                public IData IData;
                public Data Data;
                public GenericType<Param1, Param2> GenericType;
            }
        }
        
        static class AsyncNonRecursive
        {
            public interface IData
            {
            }

            public abstract class Data : IData
            {
            }

            public class GenericType<T1, T2>
            {
                public T1 t1;
                public T2 t2;
            }

            public struct Param1
            {
                public string Value;
            }

            public struct Param2
            {
                public string Value;
            }

            public struct Vec2
            {
                public float x;
                public float y;
            }

            public struct Vec3
            {
                public float x;
                public float y;
                public float z;
            }

            public struct InnerContainer
            {
                public Vec2 vec2;
            }

            public struct InnerContainer2
            {
                public Vec3 vec3;
            }

            public struct OuterContainer
            {
                public InnerContainer Inner;
                public InnerContainer2 Inner2;
                public InnerContainer SecondInner;
                public IData IData;
                public Data Data;
                public GenericType<Param1, Param2> GenericType;
            }
        }

        static class AsyncAndRecursive
        {
            public interface IData
            {
            }

            public abstract class Data : IData
            {
            }

            public class GenericType<T1, T2>
            {
                public T1 t1;
                public T2 t2;
            }

            public struct Param1
            {
                public string Value;
            }

            public struct Param2
            {
                public string Value;
            }

            public struct Vec2
            {
                public float x;
                public float y;
            }

            public struct Vec3
            {
                public float x;
                public float y;
                public float z;
            }

            public struct InnerContainer
            {
                public Vec2 vec2;
            }

            public struct InnerContainer2
            {
                public Vec3 vec3;
            }

            public struct OuterContainer
            {
                public InnerContainer Inner;
                public InnerContainer2 Inner2;
                public InnerContainer SecondInner;
                public IData IData;
                public Data Data;
                public GenericType<Param1, Param2> GenericType;
            }
        }

        [UnityTest]
        public IEnumerator PropertyBagPreparation_WhenRecursiveIsUnset_PrepareASingleBag()
        {
            if (Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.OuterContainer>())
                yield break;
            
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.OuterContainer>(), Is.False); 
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.GenericType<NonAsyncNonRecursive.Param1, NonAsyncNonRecursive.Param2>>(), Is.False);

            var options = new PropertyBagUtility.PropertyBagPreparationOptions {Recursive = false, Async = false};
            PropertyBagUtility.RequestPropertyBagGeneration<NonAsyncNonRecursive.OuterContainer>(options);

            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.OuterContainer>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncNonRecursive.GenericType<NonAsyncNonRecursive.Param1, NonAsyncNonRecursive.Param2>>(), Is.False);
        }

        [UnityTest]
        public IEnumerator PropertyBagPreparation_WhenRecursiveIsSet_PrepareAllBags()
        {
            if (Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.OuterContainer>())
                yield break;
            
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.OuterContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.GenericType<NonAsyncAndRecursive.Param1, NonAsyncAndRecursive.Param2>>(), Is.False);

            var options = new PropertyBagUtility.PropertyBagPreparationOptions {Recursive = true};
            PropertyBagUtility.RequestPropertyBagGeneration<NonAsyncAndRecursive.OuterContainer>(options);

            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.OuterContainer>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.InnerContainer>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.InnerContainer2>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.Vec2>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.Vec3>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<NonAsyncAndRecursive.GenericType<NonAsyncAndRecursive.Param1, NonAsyncAndRecursive.Param2>>(), Is.True);
        }

        [UnityTest]
        public IEnumerator PropertyBagAsyncPreparation_WhenRecursiveIsUnset_PrepareASingleBag()
        {
            if (Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.OuterContainer>())
                yield break;

            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.OuterContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.GenericType<AsyncNonRecursive.Param1, AsyncNonRecursive.Param2>>(), Is.False);

            var options = new PropertyBagUtility.PropertyBagPreparationOptions {Recursive = false, Async = true};
            PropertyBagUtility.RequestPropertyBagGeneration<AsyncNonRecursive.OuterContainer>(options);
            
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.OuterContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.GenericType<AsyncNonRecursive.Param1, AsyncNonRecursive.Param2>>(), Is.False);

            for (var i = 0; i < 25; ++i)
                yield return null;

            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.OuterContainer>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncNonRecursive.GenericType<AsyncNonRecursive.Param1, AsyncNonRecursive.Param2>>(), Is.False);
        }

        [UnityTest]
        public IEnumerator PropertyBagAsyncPreparation_WhenRecursiveIsSet_PrepareAllBags()
        {
            if (Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.OuterContainer>())
                yield break;

            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.OuterContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.GenericType<AsyncAndRecursive.Param1, AsyncAndRecursive.Param2>>(), Is.False);

            var options = new PropertyBagUtility.PropertyBagPreparationOptions {Recursive = true, Async = true};
            PropertyBagUtility.RequestPropertyBagGeneration<AsyncAndRecursive.OuterContainer>(options);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.OuterContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.InnerContainer>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.InnerContainer2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Vec2>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Vec3>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.GenericType<AsyncAndRecursive.Param1, AsyncAndRecursive.Param2>>(), Is.False);

            for (var i = 0; i < 25; ++i)
                yield return null;
            
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.OuterContainer>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.InnerContainer>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.InnerContainer2>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Vec2>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Vec3>(), Is.True);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.IData>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.Data>(), Is.False);
            Assert.That(Properties.Internal.PropertyBagStore.Exists<AsyncAndRecursive.GenericType<AsyncAndRecursive.Param1, AsyncAndRecursive.Param2>>(), Is.True);
        }
    }
}