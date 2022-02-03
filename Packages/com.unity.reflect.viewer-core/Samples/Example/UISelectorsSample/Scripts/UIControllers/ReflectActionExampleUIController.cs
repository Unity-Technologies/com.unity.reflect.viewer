using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Viewer.Example.ReflectActions
{
    public class ReflectActionExampleUIController : MonoBehaviour
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
            m_ActionBaseOverrideToggle.onValueChanged.RemoveListener(OnCustomReflectActionToggleChanged);
            m_ActionTStateToggle.onValueChanged.RemoveListener(OnReflectActionTStateToggleChanged);
            m_ActionTStateTDataToggle.onValueChanged.RemoveListener(OnActionTStateTDataToggleChanged);

            m_ExampleFlagInvertorButton.onClick.RemoveListener(OnButtonClicked);
        }

        void Start()
        {
            m_ActionBaseToggle.onValueChanged.AddListener(OnBaseActionToggleChanged);
            m_ActionBaseOverrideToggle.onValueChanged.AddListener(OnCustomReflectActionToggleChanged);
            m_ActionTStateToggle.onValueChanged.AddListener(OnReflectActionTStateToggleChanged);
            m_ActionTStateTDataToggle.onValueChanged.AddListener(OnActionTStateTDataToggleChanged);

            m_ExampleFlagInvertorButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            bool value = m_FlagSelector.GetValue();
            CopyProperties(new { stateFlag = !value, stateText = $"oldValue = {value}" });
        }

        private void OnBaseActionToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                CopyProperties(new { stateFlag = on, stateText = "CopyAllPropertiesAction dispatched" });
            }
        }

        void CopyProperties(object data)
        {
            Dispatcher.Dispatch(ActionBase.From<CopyAllPropertiesReflectAction>(data));

            //alternatively
            Dispatcher.Dispatch(ActionBase.From<ReflectAction<ExampleContext>>(data));
        }

        private void OnCustomReflectActionToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<CustomReflectAction>(on));
                SetTextProperty("CustomReflectAction dispatched");
            }
        }

        private void OnReflectActionTStateToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                Dispatcher.Dispatch(ActionBase.From<ReflectAction_StateDataT>(on));
                SetTextProperty("ReflectAction_StateDataT dispatched");
            }
        }

        private void OnActionTStateTDataToggleChanged(bool on)
        {
            if (m_FlagSelector.GetValue() != on)
            {
                var data = new StateData();
                data.stateFlag = on;
                Dispatcher.Dispatch(ActionBase.From<ReflectAction_ActionDataT_StateDataT>(data));
                SetTextProperty("ReflectAction_ActionDataT_StateDataT dispatched");
            }
        }

        void SetTextProperty(string text)
        {
            Dispatcher.Dispatch(ActionBase.From<SetTextDataPropertyReflectAction>(text));

            //alternatively
            Dispatcher.Dispatch(ReflectSetPropertyAction.From<ExampleContext>(text, nameof(IStateTextData.stateText)));
        }

        struct StateData : IStateFlagData
        {
            public bool stateFlag { get; set; }
        }
    }
}
