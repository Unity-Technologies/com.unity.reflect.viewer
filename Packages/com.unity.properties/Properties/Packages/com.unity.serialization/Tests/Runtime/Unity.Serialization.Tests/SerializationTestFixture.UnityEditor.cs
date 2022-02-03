#if UNITY_EDITOR
using NUnit.Framework;
using Unity.Properties;
using UnityEngine;
using UnityEditor;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [GeneratePropertyBag]
        internal class ClassWithGlobalObjectId
        {
            public GlobalObjectId GlobalObjectId;
            public GUID Guid;
        }
        
        [Test]
        public void ClassWithGlobalObjectId_CanBeSerializedAndDeserialized()
        {
            var image = new Texture2D(1, 1);

            AssetDatabase.CreateAsset(image, kTexture2DPath);
            AssetDatabase.ImportAsset(kTexture2DPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            try
            {
                var src = new ClassWithGlobalObjectId
                {
                    GlobalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(image),
                    Guid = GUID.Generate()
                };
                
                var dst = SerializeAndDeserialize(src);
            
                Assert.That(dst, Is.Not.SameAs(src));
                Assert.That(dst.GlobalObjectId, Is.EqualTo(src.GlobalObjectId));
                Assert.That(dst.Guid, Is.EqualTo(src.Guid));
            }
            finally
            {
                AssetDatabase.DeleteAsset(kTexture2DPath);
            }
        }
    }
}
#endif