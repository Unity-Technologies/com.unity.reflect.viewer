#if URP_AVAILABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.Reflect.Viewer.UI
{
    public class UISelectionControllerURP: UISelectionController
    {
        MultiSelectionOutlineFeature m_MultiSelectionOutlineFeature;
        ScriptableRendererData[] m_RendererDatas;
        Texture2D m_CachedPalette;
        Coroutine m_WaitCoroutine;
        IDisposable m_QualityStateDataSelector;

        protected override void Awake()
        {
            base.Awake();

            UpdateRendererDatas((UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline);

            m_QualityStateDataSelector = UISelectorFactory.createSelector<int>(ApplicationSettingsContext.current, nameof(IApplicationSettingsDataProvider<QualityState>.qualityStateData) + "." + nameof(IQualitySettingsDataProvider.qualityLevel)
                , (qualityLevel) =>
                {
                    var pipeline = (UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(qualityLevel);

                    if (m_WaitCoroutine != null)
                    {
                        StopCoroutine(m_WaitCoroutine);
                    }
                    m_WaitCoroutine = StartCoroutine(WaitForRendererPipelineChange(pipeline));
                });
        }

        protected override void OnDestroy()
        {
            m_QualityStateDataSelector?.Dispose();
            base.OnDestroy();
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
            m_RendererDatas = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline));
        }

        protected override void ChangePalette(Texture2D texture)
        {
            m_CachedPalette = texture;

            if (m_RendererDatas == null)
                return;

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
            if (m_RendererDatas == null)
                return;

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
