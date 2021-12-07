using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Reflect;
using Unity.Reflect.Data;
using Unity.Reflect.IO;
using Unity.Reflect.Model;
using UnityEngine;
using UnityEngine.Reflect.Pipeline;

namespace ReflectViewerRuntimeTests
{
    public class BasicTestingPipeline : MonoBehaviour
    {
        public string ModelPath { get; private set; }

        public ReflectPipeline reflectBehaviour;
        public PipelineAsset pipelineAsset;

        void Start()
        {
            reflectBehaviour = gameObject.AddComponent<ReflectPipeline>();
            reflectBehaviour.pipelineAsset = pipelineAsset;
        }

        public void InitAndRefresh(string modelPath)
        {
            ModelPath = modelPath;
            reflectBehaviour.InitializeAndRefreshPipeline(new SampleSyncModelProvider(ModelPath));
        }
    }

    public class SampleSyncModelProvider : ISyncModelProvider
    {
        string m_DataFolder;

        public SampleSyncModelProvider(string modelPath)
        {

            m_DataFolder = Directory.EnumerateDirectories(Directory.GetParent(Application.dataPath).Parent.FullName, ".PerformanceTestProjects", SearchOption.AllDirectories).FirstOrDefault();

            if (m_DataFolder == null)
                Debug.LogError("Unable to find Samples data. Reflect Samples require local Reflect Model data in '.PerformanceTestProjects' in " + Directory.GetParent(Application.dataPath).Parent.FullName);
            else if (!string.IsNullOrEmpty(modelPath) && Directory.Exists(Path.Combine(m_DataFolder, modelPath)))
                m_DataFolder = Path.Combine(m_DataFolder, modelPath);
        }

        public async Task<IEnumerable<SyncManifest>> GetSyncManifestsAsync(CancellationToken token)
        {
            return Task.WhenAll(Directory.EnumerateFiles(m_DataFolder, "*.manifest", SearchOption.AllDirectories).ToList().Select(async x => await PlayerFile.LoadManifestAsync(x))).Result;
        }

        public async Task<ISyncModel> GetSyncModelAsync(StreamKey streamKey, string hash, CancellationToken token)
        {
            var fullPath = Path.Combine(m_DataFolder, hash + PlayerFile.PersistentKeyToExtension(streamKey.key));
            return await PlayerFile.LoadSyncModelAsync(fullPath, streamKey.key, new CancellationToken());
        }
    }
}
