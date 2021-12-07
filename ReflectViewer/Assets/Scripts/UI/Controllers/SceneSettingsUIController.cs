using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class SceneSettingsUIController: MonoBehaviour
    {
#pragma warning disable 649

        [SerializeField]
        ToolButton m_DialogButton;
        [SerializeField]
        SlideToggle m_TextureToggle;
        [SerializeField]
        SlideToggle m_LightDataToggle;

#pragma warning restore 649

        DialogWindow m_DialogWindow;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_DialogButton.buttonClicked += OnDialogButtonClicked;

            m_TextureToggle.onValueChanged.AddListener(OnTextureToggleChanged);
            m_LightDataToggle.onValueChanged.AddListener(OnLightDataToggleChanged);

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableLightData), newData => { m_LightDataToggle.on = newData; }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableTexture), newData => { m_TextureToggle.on = newData; }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType data)
        {
            m_DialogButton.selected = data == OpenDialogAction.DialogType.SceneSettings;
        }

        void OnTextureToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetEnableTextureAction.From(on));
        }

        void OnLightDataToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { enableLightData = on }));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.SceneSettings;
            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
        }
    }
}
