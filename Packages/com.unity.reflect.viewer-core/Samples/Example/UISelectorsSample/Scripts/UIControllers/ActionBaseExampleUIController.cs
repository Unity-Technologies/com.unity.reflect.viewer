using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Viewer.Example.ActionsBase
{
    public class ActionBaseExampleUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Toggle m_ActionBaseToggle;

        [SerializeField]
        Toggle m_ActionBaseOverrideToggle;

        [SerializeField]
        Toggle m_ActionTStateToggle;

        [SerializeField]
        Toggle m_ActionTStateTDataToggle;

        [SerializeField]
        Text m_ExampleTextSettingValue;

        [SerializeField]
        Button m_ExampleFlagInvertorButton;
#pragma warning restore 649

        IUISelector<bool> m_FlagSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_FlagSelector = UISelectorFactory.createSelector<bool>(ExampleContext.current, nameof(IStateFlagData.stateFlag), (flag) =>
            {
                m_ActionBaseToggle.isOn = flag;
                m_ActionBaseOverrideToggle.isOn = flag;
                m_ActionTStateToggle.isOn = flag;
                m_ActionTStateTDataToggle.isOn = flag;
            }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ExampleContext.current, nameof(IStateTextData.stateText), (text) =>
            {
                m_ExampleTextSettingValue.text = text;
            }));
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());

            m_ActionBaseToggle.onValueChanged.RemoveListener(OnBaseActionToggleChanged);
            m_ActionBaseOverrideToggle.onValueChanged.RemoveListener(OnCustomActionToggleChanged);
            m_ActionTStateToggle.onValueChanged.RemoveListener(OnActionTStateToggleChanged);
            m_ActionTStateTDataToggle.onValueChanged.RemoveListener(OnActionTStateTDataToggleChanged);

            m_ExampleFlagInvertorButton.onClick.RemoveListener(OnButtonClicked);
        }

        void Start()
        {
            m_ActionBaseToggle.onValueChanged.AddListener(OnBaseActionToggleChanged);
            m_ActionBaseOverrideToggle.onValueChanged.AddListener(OnCustomActionToggleChanged);
            m_ActionTStateToggle.onValueChanged.AddListener(OnActionTStateToggleChanged);
            m_ActionTStateTDataToggle.onValueChanged.AddListener(OnActionTStateTDataToggleChanged);

            m_ExampleFlagInvertorButton.onClick.AddListener(OnButtonClicked);
        }

        void OnButtonClicked()
        {
            bool value = m_FlagSelector.GetValue();
            Dispatcher.Dispatch(ActionBase.From<CopyAllPropertiesAction>(new { stateFlag = !value, stateText = $"oldValue = {value}" }));
        }

        void OnBaseActionToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<CopyAllPropertiesAction>(new { stateFlag = on, stateText = "CopyAllPropertiesAction dispatched" }));
            }
        }

        void OnCustomActionToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<CustomAction>(on));
                SetTextProperty("CustomAction dispatched");
            }
        }

        void OnActionTStateToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<Action_StateDataT>(on));
                SetTextProperty("Action_StateDataT dispatched");
            }
        }

        void OnActionTStateTDataToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                var data = new StateData();
                data.stateFlag = on;
                Dispatcher.Dispatch(ActionBase.From<Action_ActionDataT_StateDataT>(data));
                SetTextProperty("Action_StateDataT_ActionDataT dispatched");
            }
        }

        struct StateData : IStateFlagData
        {
            public bool stateFlag { get; set; }
        }

        void SetTextProperty(string text)
        {
            Dispatcher.Dispatch(ActionBase.From<SetTextDataPropertyAction>(text));
        }
    }
}
