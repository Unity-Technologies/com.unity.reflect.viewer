using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Viewer.Example
{
    public class ExampleUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Toggle m_BaseActionToggle;

        [SerializeField]
        Toggle m_BaseActionOverrideToggle;

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
                m_BaseActionToggle.isOn = flag;
                m_BaseActionOverrideToggle.isOn = flag;
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

            m_BaseActionToggle.onValueChanged.RemoveListener(OnBaseActionToggleChanged);
            m_BaseActionOverrideToggle.onValueChanged.RemoveListener(OnBaseActionToggleOverrideChanged);
            m_ActionTStateToggle.onValueChanged.RemoveListener(OnActionTStateToggleChanged);
            m_ActionTStateTDataToggle.onValueChanged.RemoveListener(OnTStateTDataActionToggleChanged);

            m_ExampleFlagInvertorButton.onClick.RemoveListener(OnButtonClicked);
        }

        void Start()
        {
            m_BaseActionToggle.onValueChanged.AddListener(OnBaseActionToggleChanged);
            m_BaseActionOverrideToggle.onValueChanged.AddListener(OnBaseActionToggleOverrideChanged);
            m_ActionTStateToggle.onValueChanged.AddListener(OnActionTStateToggleChanged);
            m_ActionTStateTDataToggle.onValueChanged.AddListener(OnTStateTDataActionToggleChanged);

            m_ExampleFlagInvertorButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            bool value = m_FlagSelector.GetValue();
            Dispatcher.Dispatch(ActionBase.From<ReflectAction<ExampleContext>>(new { stateFlag = !value, stateText = $"oldValue = {value}" }));
        }

        private void OnBaseActionToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<DefaultAction>(new { stateFlag = on, stateText = "OnBaseActionToggleChanged" }));
            }
        }

        private void OnBaseActionToggleOverrideChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<SetTextDataAction>("OnBaseActionToggleOverrideChanged"));
                Dispatcher.Dispatch(ActionBase.From<DefaultActionOverride>(on));
            }
        }

        private void OnActionTStateToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<SetTextDataAction>("OnActionTStateToggleChanged"));
                Dispatcher.Dispatch(ActionBase.From<StateDataActionOverride>(on));
            }
        }

        private void OnTStateTDataActionToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<SetTextDataAction>("OnTStateTDataActionToggleChanged"));

                var data = new StateData();
                data.stateFlag = on;
                Dispatcher.Dispatch(ActionBase.From<StateDataTActionTDataOverride>(data));
            }
        }

        struct StateData : IStateFlagData
        {
            public bool stateFlag { get; set; }
        }
    }
}
