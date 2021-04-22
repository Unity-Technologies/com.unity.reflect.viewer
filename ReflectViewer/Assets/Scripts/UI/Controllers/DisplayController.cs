using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Utils;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class DisplayController : UIBehaviour
    {
        const int k_Tolerance = 10;
        CanvasScaler m_CanvasScaler;
        Vector2 m_UnscaledScreenSize;
        float m_CurrentScaleFactor = 1;
        public Action<DisplayData> OnDisplayChanged;

        protected override void Awake()
        {
            m_CanvasScaler = GetComponent<CanvasScaler>();
            m_UnscaledScreenSize = Vector2.zero;
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        protected override void Start()
        {
            base.Start();
            OnRectTransformDimensionsChange();
        }

        void OnStateDataChanged(UIStateData uiData)
        {
            if (uiData.display.scaleFactor != m_CurrentScaleFactor)
            {
                m_CurrentScaleFactor = uiData.display.scaleFactor;
                m_CanvasScaler.scaleFactor = m_CurrentScaleFactor;
                m_CanvasScaler.enabled = m_CurrentScaleFactor != 1f;
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (UIStateManager.current != null && ((Math.Abs(Screen.width - m_UnscaledScreenSize.x) > k_Tolerance) ||
                (Math.Abs(Screen.height - m_UnscaledScreenSize.y) > k_Tolerance)))
            {
                m_UnscaledScreenSize = new Vector2(Screen.width, Screen.height);

                var screenDpi = UIUtils.GetScreenDpi();
                var deviceType = UIUtils.GetDeviceType(m_UnscaledScreenSize.x, m_UnscaledScreenSize.y, screenDpi);
                var targetDpi = UIUtils.GetTargetDpi(screenDpi, deviceType);

                var scaleFactor = UIUtils.GetScaleFactor(m_UnscaledScreenSize.x, m_UnscaledScreenSize.y, screenDpi, deviceType);
                var display = UIStateManager.current.stateData.display;
                display.screenSize = m_UnscaledScreenSize;
                display.scaledScreenSize = new Vector2(m_UnscaledScreenSize.x / scaleFactor, m_UnscaledScreenSize.y / scaleFactor);
                display.screenSizeQualifier = UIUtils.QualifyScreenSize(display.scaledScreenSize);
                display.targetDpi = targetDpi;
                display.dpi = screenDpi;
                display.scaleFactor = scaleFactor;
                display.displayType = deviceType;
                OnDisplayChanged?.Invoke(display);
            }
        }
    }
}
