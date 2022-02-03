using System;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Utils;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class DisplayController : UIBehaviour
    {
        const int k_Tolerance = 2;
        CanvasScaler m_CanvasScaler;
        Vector2 m_UnscaledScreenSize;
        IUISelector m_scaleFactorSelector;


#if UNITY_STANDALONE_WIN
        bool m_HasCheckedResolutionIssue;
#endif

        protected override void Awake()
        {
            m_UnscaledScreenSize = Vector2.zero;

            m_scaleFactorSelector = UISelectorFactory.createSelector<float>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.scaleFactor), OnScaleFactorChanged);
            m_CanvasScaler = GetComponent<CanvasScaler>();
        }

        void OnScaleFactorChanged(float newData)
        {
            if (m_CanvasScaler != null)
            {
                m_CanvasScaler.scaleFactor = newData;
                m_CanvasScaler.enabled = Math.Abs(newData - 1f) > Mathf.Epsilon;
            }
        }

        protected override void OnDestroy()
        {
            m_scaleFactorSelector?.Dispose();
            base.OnDestroy();
        }

        protected override void Start()
        {
            base.Start();
            OnRectTransformDimensionsChange();
        }


        protected override void OnRectTransformDimensionsChange()
        {
            if (UIStateManager.current != null && ((Math.Abs(Screen.width - m_UnscaledScreenSize.x) > k_Tolerance) ||
                (Math.Abs(Screen.height - m_UnscaledScreenSize.y) > k_Tolerance)))
            {

#if UNITY_STANDALONE_WIN
                if (CheckAutoResizeTooSmall())
                {
                    return;
                }
#endif

                m_UnscaledScreenSize = new Vector2(Screen.width, Screen.height);

                var screenDpi = UIUtils.GetScreenDpi();
                var deviceType = UIUtils.GetDeviceType(m_UnscaledScreenSize.x, m_UnscaledScreenSize.y, screenDpi);
                var targetDpi = UIUtils.GetTargetDpi(screenDpi, deviceType);

                var scaleFactor = UIUtils.GetScaleFactor(m_UnscaledScreenSize.x, m_UnscaledScreenSize.y, screenDpi, deviceType);
                var display = new SetDisplayAction.SetDisplayData();
                display.screenSize = m_UnscaledScreenSize;
                display.scaledScreenSize = new Vector2(m_UnscaledScreenSize.x / scaleFactor, m_UnscaledScreenSize.y / scaleFactor);
                display.screenSizeQualifier = UIUtils.QualifyScreenSize(display.scaledScreenSize);
                display.targetDpi = targetDpi;
                display.dpi = screenDpi;
                display.scaleFactor = scaleFactor;
                display.displayType = deviceType;
                Dispatcher.Dispatch(SetDisplayAction.From(display));
            }
        }

#if UNITY_STANDALONE_WIN
        // When we launch the application from Sub Monitor Display, and the Display Scale is not 100%,
        // Windows Standalone application window size will be changed to very small.(ex.240x153)
        // This behaviour occurs only one time so we need this code to prevent the behaviour.
        bool CheckAutoResizeTooSmall()
        {
            if (!m_HasCheckedResolutionIssue && m_UnscaledScreenSize != Vector2.zero)
            {
                m_HasCheckedResolutionIssue = true;
                if (Screen.height < 300)
                {
                    Screen.SetResolution((int)m_UnscaledScreenSize.x, (int)m_UnscaledScreenSize.y, FullScreenMode.Windowed);
                    return true;
                }
            }
            return false;
        }
#endif
    }
}
