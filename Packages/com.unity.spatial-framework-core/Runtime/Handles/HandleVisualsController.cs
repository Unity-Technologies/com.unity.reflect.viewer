using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using Unity.SpatialFramework.Utils;
using Unity.Tweening;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Controls the visual appearance of standard handles based on their interaction state.
    /// The handle grows in scale when hovered, and has a lighter color. When dragged it changes to a specified color.
    /// </summary>
    public class HandleVisualsController : MonoBehaviour
    {
        class HandleStateData
        {
            internal Renderer targetRenderer;
            internal Color idleColor;

            /// <summary>
            /// Gets the color to use while hovering the handle. This will be 50% blend between the idle color and white
            /// </summary>
            /// <returns>The hover color</returns>
            internal Color GetHoverColor()
            {
                return Color.Lerp(idleColor, Color.white, 0.5f);
            }
        }

#pragma warning disable 649
        [SerializeField, Tooltip("All the handles that will be controlled.")]
        List<BaseHandle> m_Handles;

        [SerializeField, Tooltip("The color to use when the handle is being dragged. Default is yellow FFD600")]
        Color m_DragColor = new Color(1f, 0.8392157f, 0f); //FFD600

        [SerializeField, Tooltip("The amount to scale the handle when it is being hovered.")]
        float m_HoverScaleFactor = 1.25f;

        [SerializeField, Tooltip("The duration for the hover scaling in seconds.")]
        float m_HoverScaleTransitionDuration = 0.1f;
#pragma warning restore 649

        Dictionary<BaseHandle, HandleStateData> m_HandleData = new Dictionary<BaseHandle, HandleStateData>();
        bool m_Dragging;
        Coroutine m_ScaleCoroutine;

        static MaterialPropertyBlock s_MatPropBlock;
        static int s_ColorNameID;

        /// <summary>
        /// The color of the handle when it is being dragged
        /// </summary>
        public Color dragColor
        {
            get => m_DragColor;
            set => m_DragColor = value;
        }

        void Awake()
        {
            m_HandleData.Clear();
            if (s_MatPropBlock == null)
                InitStaticMaterialValues();

            foreach (var handle in m_Handles)
            {
                handle.hoverStarted += HoverBegin;
                handle.hoverEnded += HoverEnd;
                handle.dragStarted += DragBegin;
                handle.dragEnded += DragEnd;

                var handleStateData = new HandleStateData { targetRenderer = handle.GetComponent<Renderer>() };
                handleStateData.idleColor = handleStateData.targetRenderer.sharedMaterial.color;

                m_HandleData.Add(handle, handleStateData);
            }
        }
        static void InitStaticMaterialValues()
        {
            s_MatPropBlock = new MaterialPropertyBlock();
            s_ColorNameID = Shader.PropertyToID("_Color");
        }

        void HoverBegin(BaseHandle baseHandle, HandleEventData handleEventData)
        {
            if (baseHandle.hasDragSource)
                return;

            var handleStateData = m_HandleData[baseHandle];
            SetColor(handleStateData.targetRenderer, handleStateData.GetHoverColor());
            SetScale(baseHandle);
        }

        void HoverEnd(BaseHandle baseHandle, HandleEventData handleEventData)
        {
            if (baseHandle.hasDragSource)
                return;

            var handleStateData = m_HandleData[baseHandle];
            SetColor(handleStateData.targetRenderer, handleStateData.idleColor);
            SetScale(baseHandle);
        }

        void DragBegin(BaseHandle baseHandle, HandleEventData handleEventData)
        {
            var handleStateData = m_HandleData[baseHandle];
            SetColor(handleStateData.targetRenderer, dragColor);
            SetScale(baseHandle);

            m_Dragging = true;

            // Hide other handles and disable interaction
            foreach (var handle in m_Handles)
            {
                if (baseHandle != handle)
                {
                    var otherHandleData = m_HandleData[handle];
                    handle.enabled = false; // Disable handle before setting the color, because this could cancel a hover, which sets the color.
                    SetColor(otherHandleData.targetRenderer, Color.clear);
                }
            }
        }

        void DragEnd(BaseHandle baseHandle, HandleEventData handleEventData)
        {
            SetScale(baseHandle);
            m_Dragging = false;

            // Unhide other handles
            foreach (var handle in m_Handles)
            {
                var handleStateData = m_HandleData[handle];
                SetColor(handleStateData.targetRenderer, baseHandle.isHovered ? handleStateData.GetHoverColor() : handleStateData.idleColor);
                handle.enabled = true;
            }
        }

        void SetScale(BaseHandle handle)
        {
            var targetScale = 1f;
            if (handle.hasHoverSource || handle.hasDragSource)
            {
                targetScale = m_HoverScaleFactor;
            }

            this.RestartCoroutine(ref m_ScaleCoroutine, handle.transform.TweenScale(targetScale * Vector3.one, m_HoverScaleTransitionDuration));
        }

        static void SetColor(Renderer targetRenderer, Color color)
        {
            s_MatPropBlock.SetColor(s_ColorNameID, color);
            targetRenderer.SetPropertyBlock(s_MatPropBlock);
        }

        void Update()
        {
            // When not dragging, handles that are not ray selectable and not hovered are semi-transparent
            if (m_Dragging)
                return;

            foreach (var handle in m_Handles)
            {
                if (handle.isHovered)
                    continue;

                var handleStateData = m_HandleData[handle];
                var color = handleStateData.idleColor;
                color.a = (handle.selectionFlags & SelectionFlags.Ray) == 0 ? 0.2f : 1f;

                SetColor(handleStateData.targetRenderer, color);
            }
        }
    }
}
