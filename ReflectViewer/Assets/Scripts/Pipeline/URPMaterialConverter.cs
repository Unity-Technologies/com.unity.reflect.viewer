using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Reflect;
using Unity.Reflect.Data;
using Unity.Reflect.Model;

namespace UnityEngine.Reflect.Pipeline
{
    [Serializable]
    class ReflectURPMaterialConverter : IReflectMaterialConverter
    {
#pragma warning disable CS0649
        [SerializeField]
        Material m_DefaultMaterial;

        [SerializeField]
        Shader m_URPOpaqueShader;

        [SerializeField]
        Shader m_URPTransparentShader;

        [SerializeField]
        Shader m_URPDoubleOpaqueShader;

        [SerializeField]
        Shader m_URPDoubleTransparentShader;
#pragma warning restore CS0649

#if UNITY_EDITOR
        public Material defaultEditorMaterial => m_DefaultMaterial;
#endif
        public string name => "Reflect Universal RP";

        public bool IsAvailable => true;

        public Shader GetShader(SyncMaterial syncMaterial)
        {
            var transparent = StandardShaderHelper.IsTransparent(syncMaterial);
            if (syncMaterial.IsDoubleSided)
                return transparent ? m_URPDoubleTransparentShader : m_URPDoubleOpaqueShader;

            return transparent ? m_URPTransparentShader : m_URPOpaqueShader;
        }

        public void SetMaterialProperties(SyncedData<SyncMaterial> syncMaterial, Material material, ITextureCache textureCache)
        {
            StandardShaderHelper.ComputeMaterial(syncMaterial, material, textureCache);
        }

        public Material defaultMaterial => m_DefaultMaterial;
    }

    [Serializable]
    public class URPMaterialConverterNode : ReflectNode<URPMaterialConverter>, IMaterialCache
    {
#pragma warning disable CS0649
        [Header("Materials")]
        [SerializeField]
        ReflectURPMaterialConverter m_ReflectUniversalRp;
#pragma warning restore CS0649

        public TextureCacheParam textureCacheParam = new TextureCacheParam();

        public SyncMaterialInput input = new SyncMaterialInput();
        public MaterialOutput output = new MaterialOutput();

        protected override URPMaterialConverter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var p = new URPMaterialConverter(hook.services.eventHub, hook.services.memoryTracker, textureCacheParam.value, output, m_ReflectUniversalRp);

            input.streamEvent = p.OnStreamEvent;

            return p;
        }

        public Material GetMaterial(StreamKey key)
        {
            return processor.GetFromCache(key);
        }
    }

    public class URPMaterialConverter : MaterialConverter
    {
        public URPMaterialConverter(EventHub hub, MemoryTracker memTracker, ITextureCache textureCache, IOutput<SyncedData<Material>> output, IReflectMaterialConverter converter)
            : base(hub, memTracker, textureCache, output)
        {
            ReflectMaterialManager.RegisterConverter(converter);
        }
    }
}
