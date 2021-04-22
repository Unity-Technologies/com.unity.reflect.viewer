#if URP_AVAILABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.Reflect.Viewer.UI
{
    public class UISelectionControllerURP : UISelectionController
    {
        MultiSelectionOutlineFeature m_MultiSelectionOutlineFeature;
        ScriptableRendererData[] m_RendererDatas;
        int m_CachedQuality = -1;
        Texture2D m_CachedPalette;
        Coroutine m_WaitCoroutine;

        protected override void Awake()
        {
            base.Awake();

            UpdateRendererDatas((UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline);

            UIStateManager.applicationStateChanged += OnQualityChanged;
        }

        void OnQualityChanged(ApplicationStateData data)
        {
            if (data.qualityStateData.qualityLevel == m_CachedQuality)
                return;

            var pipeline = (UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(data.qualityStateData.qualityLevel);

            if (m_WaitCoroutine != null)
            {
                StopCoroutine(m_WaitCoroutine);
            }
            m_WaitCoroutine = StartCoroutine(WaitForRendererPipelineChange(pipeline));

            m_CachedQuality = data.qualityStateData.qualityLevel;
        }

        IEnumerator WaitForRendererPipelineChange(UniversalRenderPipelineAsset pipeline)
        {
            while (pipeline != (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline)
            {
                yield return null;
            }

            UpdateRendererDatas(pipeline);
            ChangePalette(m_CachedPalette);
            UpdateMultiSelection();

            m_WaitCoroutine = null;
        }

        void UpdateRendererDatas(UniversalRenderPipelineAsset pipeline)
        {
            FieldInfo propertyInfo = pipeline.GetType(  ).GetField( "m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic );
            m_RendererDatas = ((ScriptableRendererData[]) propertyInfo?.GetValue( pipeline ));
        }

        protected override void ChangePalette(Texture2D texture)
        {
            m_CachedPalette = texture;

            foreach (var rendererData in m_RendererDatas)
            {
                var multiselectionFeature = rendererData.rendererFeatures.OfType<MultiSelectionOutlineFeature>().FirstOrDefault();
                if (multiselectionFeature != null)
                {
                    multiselectionFeature.ChangePalette(texture);
                }
            }
        }

        protected override void UpdateMultiSelection()
        {
            foreach (var rendererData in m_RendererDatas)
            {
                var multiselectionFeature = rendererData.rendererFeatures.OfType<MultiSelectionOutlineFeature>().FirstOrDefault();
                if (multiselectionFeature != null)
                {
                    multiselectionFeature.datas.Clear();

                    foreach (var data in m_SelectedDatas)
                    {
                        if (data.selectedObject != null)
                        {
                            var renderers = new List<Renderer>();
                            var renderer = data.selectedObject.GetComponent<Renderer>();
                            if (renderer == null)
                            {
                                renderers = data.selectedObject.GetComponentsInChildren<Renderer>().ToList();
                            }
                            else
                            {
                                renderers.Add(renderer);
                            }

                            multiselectionFeature.datas.Add(new MultiSelectionOutlineFeature.SelectionOutlineData()
                            {
                                colorId = data.colorId,
                                renderers = renderers
                            });
                        }
                    }
                }
            }
        }
    }
}
#endif
