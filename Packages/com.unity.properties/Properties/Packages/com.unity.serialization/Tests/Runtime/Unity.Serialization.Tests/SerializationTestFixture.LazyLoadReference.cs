using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithLazyLoadReference_WhenValueIsNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithLazyLoadReferences();
            var dst = SerializeAndDeserialize(src);

            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.ObjectValue, Is.EqualTo(src.ObjectValue));
        }

#if UNITY_EDITOR
        [Test]
        public void ClassWithLazyLoadReference_WhenValueIsTexture2DAsset_CanBeSerializedAndDeserialized()
        {
            var image = new Texture2D(1, 1);

            AssetDatabase.CreateAsset(image, kTexture2DPath);
            AssetDatabase.ImportAsset(kTexture2DPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            try
            {
                var src = new ClassWithLazyLoadReferences
                {
                    ObjectValue = new LazyLoadReference<Object> { asset = image }
                };

                var dst = SerializeAndDeserialize(src);

                Assert.That(dst, Is.Not.SameAs(src));
                Assert.That(dst.ObjectValue, Is.EqualTo(src.ObjectValue));
            }
            finally
            {
                AssetDatabase.DeleteAsset(kTexture2DPath);
            }
        }

        [Test]
        public void LazyLoadReference_WhenRootIsTexture2DAsset_CanBeSerializedAndDeserialized()
        {
            var image = new Texture2D(1, 1);

            AssetDatabase.CreateAsset(image, kTexture2DPath);
            AssetDatabase.ImportAsset(kTexture2DPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var lazyLoadReference = new LazyLoadReference<Texture2D> { asset = image };
            try
            {
                var dst = SerializeAndDeserialize(lazyLoadReference);
                Assert.That(dst, Is.EqualTo(lazyLoadReference));
            }
            finally
            {
                AssetDatabase.DeleteAsset(kTexture2DPath);
            }
        }
#endif
    }
}