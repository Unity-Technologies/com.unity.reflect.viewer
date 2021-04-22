using SharpFlux;
using System;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ARScaleRadialUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        DialControl m_ScaleDialControl;
        [SerializeField]
        Button m_MainButton;
        [SerializeField]
        Button m_ResetButton;
        [SerializeField]
        TextMeshProUGUI m_ARScaleText;
#pragma warning restore CS0649

        ArchitectureScale m_DefaultScale = ArchitectureScale.OneToOneHundred;
        static int m_NumScales = Enum.GetNames(typeof(ArchitectureScale)).Length;
        ARLabelConverter m_LabelConverter = new ARLabelConverter();
        public static ToolbarType m_previousToolbar; // Note, will only be either ARSidebar or ARInstructionSidebar and set in those sidebar controllers
        ArchitectureScale m_CurrentScale;
        bool? m_ShowScaleReference;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            m_ScaleDialControl.onSelectedValueChanged.AddListener(OnScaleDialValueChanged);
            m_ResetButton.onClick.AddListener(OnResetButtonClicked);
            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_ScaleDialControl.labelConverter = m_LabelConverter;
            m_ScaleDialControl.selectedValue = GetFloatFromScale(m_DefaultScale);
            m_ScaleDialControl.maximumValue = m_NumScales - 1; // Note, internal radial values will be ArchitectureScale enum indices
            m_ARScaleText.text = FormatScaleText(UIStateManager.current.stateData.modelScale);
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_CurrentScale != data.modelScale)
            {
                m_ScaleDialControl.selectedValue = GetFloatFromScale(data.modelScale);
                m_ARScaleText.text = FormatScaleText(data.modelScale);
                m_CurrentScale = data.modelScale;
            }

            if (m_ShowScaleReference != data.navigationState.showScaleReference)
            {
                m_ARScaleText.gameObject.SetActive(data.navigationState.showScaleReference);
                m_ShowScaleReference = data.navigationState.showScaleReference;
            }
        }

        void OnScaleDialValueChanged(float value)
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, GetScaleFromFloat(value)));
        }

        void OnResetButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, m_DefaultScale));
        }

        void OnMainButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, m_previousToolbar));
            // TODO AR scale message (text)
        }

        public static string FormatScaleText(ArchitectureScale scale)
        {
            return $"1 : {(int)scale}";
        }

        // Get ArchitectureScale value from Dial's float value, and vice versa
        public static ArchitectureScale GetScaleFromFloat(float value)
        {
            var index = (int)Mathf.Round(value);
            return (ArchitectureScale)Enum.GetValues(typeof(ArchitectureScale)).GetValue(index);
        }
        public static float GetFloatFromScale(ArchitectureScale scale)
        {
            var index = Array.IndexOf(Enum.GetValues(typeof(ArchitectureScale)), scale);
            return index;
        }

        class ARLabelConverter : ILabelConverter
        {
            public string ConvertSelectedValLabel(float value, bool isInt)
            {
                return $"1 : {(int)GetScaleFromFloat(value)}";
            }
            public string ConvertTickLabels(float value)
            {
                return $"1 : {(int)GetScaleFromFloat(value)}";
            }
        }
    }
}
