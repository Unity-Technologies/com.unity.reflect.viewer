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
        public void ClassWithUnityObject_WhenValueIsNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithUnityObjects();
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.ObjectValue, Is.EqualTo(src.ObjectValue));
        }
        
#if UNITY_EDITOR
        const string kTexture2DPath = "Assets/JsonSerializationTests-test-image.asset";

        [Test]
        public void ClassWithUnityObject_WhenValueIsTexture2DAsset_CanBeSerializedAndDeserialized()
        {
            var image = new Texture2D(1, 1);

            AssetDatabase.CreateAsset(image, kTexture2DPath);
            AssetDatabase.ImportAsset(kTexture2DPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            try
            {
                var src = new ClassWithUnityObjects
                {
                    ObjectValue = image
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
        public void UnityObject_WhenRootIsTexture2DAsset_CanBeSerializedAndDeserialized()
        {
            var image = new Texture2D(1, 1);

            AssetDatabase.CreateAsset(image, kTexture2DPath);
            AssetDatabase.ImportAsset(kTexture2DPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            try
            {
                var dst = SerializeAndDeserialize(image);
                Assert.That(dst, Is.EqualTo(image));
            }
            finally
            {
                AssetDatabase.DeleteAsset(kTexture2DPath);
            }
        }
#endif
    }
}