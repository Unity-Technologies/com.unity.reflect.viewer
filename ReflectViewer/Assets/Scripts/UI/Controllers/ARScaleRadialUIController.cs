using System;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

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

        SetModelScaleAction.ArchitectureScale m_DefaultScale = SetModelScaleAction.ArchitectureScale.OneToOneHundred;
        static int m_NumScales = Enum.GetNames(typeof(SetModelScaleAction.ArchitectureScale)).Length;
        ARLabelConverter m_LabelConverter = new ARLabelConverter();
        public static SetActiveToolBarAction.ToolbarType m_previousToolbar; // Note, will only be either ARSidebar or ARInstructionSidebar and set in those sidebar controllers
        IUISelector<SetModelScaleAction.ArchitectureScale> m_ModelScaleSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_ModelScaleSelector = UISelectorFactory.createSelector<SetModelScaleAction.ArchitectureScale>(ARPlacementContext.current, nameof(IARPlacementDataProvider.modelScale), OnModelScaleChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.showScaleReference),
                data =>
                {
                    m_ARScaleText.gameObject.SetActive(data);
                }));
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Start()
        {
            m_ScaleDialControl.onSelectedValueChanged.AddListener(OnScaleDialValueChanged);
            m_ResetButton.onClick.AddListener(OnResetButtonClicked);
            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_ScaleDialControl.labelConverter = m_LabelConverter;
            m_ScaleDialControl.selectedValue = GetFloatFromScale(m_DefaultScale);
            m_ScaleDialControl.maximumValue = m_NumScales - 1; // Note, internal radial values will be ArchitectureScale enum indices
            m_ARScaleText.text = FormatScaleText(m_ModelScaleSelector.GetValue());
        }

        void OnModelScaleChanged(SetModelScaleAction.ArchitectureScale newData)
        {
            m_ScaleDialControl.selectedValue = GetFloatFromScale(newData);
            m_ARScaleText.text = FormatScaleText(newData);
        }

        void OnScaleDialValueChanged(float value)
        {
            Dispatcher.Dispatch(SetModelScaleAction.From(GetScaleFromFloat(value)));
        }

        void OnResetButtonClicked()
        {
            Dispatcher.Dispatch(SetModelScaleAction.From(m_DefaultScale));
        }

        void OnMainButtonClicked()
        {
            Dispatcher.Dispatch(SetActiveToolBarAction.From(m_previousToolbar));
            // TODO AR scale message (text)
        }

        public static string FormatScaleText(SetModelScaleAction.ArchitectureScale scale)
        {
            return $"1 : {(int)scale}";
        }

        // Get ArchitectureScale value from Dial's float value, and vice versa
        public static SetModelScaleAction.ArchitectureScale GetScaleFromFloat(float value)
        {
            var index = (int)Mathf.Round(value);
            return (SetModelScaleAction.ArchitectureScale)Enum.GetValues(typeof(SetModelScaleAction.ArchitectureScale)).GetValue(index);
        }
        public static float GetFloatFromScale(SetModelScaleAction.ArchitectureScale scale)
        {
            var index = Array.IndexOf(Enum.GetValues(typeof(SetModelScaleAction.ArchitectureScale)), scale);
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
