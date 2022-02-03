using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class SyncToggleButton: MonoBehaviour
    {
        [SerializeField]
        Sprite m_SyncEnabledSprite;
        [SerializeField]
        Sprite m_SyncDisabledSprite;

        Button m_Button;
        bool m_Visibility;
        IUISelector<bool> m_SyncEnabledSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;

        bool? m_CachedSyndEnabled;
        bool m_Interactable;
        IUISelector<bool> m_ToolBarEnabledSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_Button = GetComponent<Button>();
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));

            m_DisposeOnDestroy.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled), UpdateButtonInteractable));

            //UIStateManager.stateChanged += OnStateDataChanged;
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));

            m_Visibility = true;
            m_Interactable = true;
            m_DisposeOnDestroy.Add(m_SyncEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(ISyncModeDataProvider.syncEnabled), OnSyncEnable));
            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<NetworkReachability>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.networkReachability), NetworkReachabilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectServerConnection), ProjectServerConnectionChanged));
        }

        void OnSyncEnable(bool newData)
        {
            m_Button.image.sprite = newData ? m_SyncEnabledSprite : m_SyncDisabledSprite;
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClick);
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType data)
        {
            m_Button.transform.parent.gameObject.SetActive(m_Visibility && data != OpenDialogAction.DialogType.LandingScreen);
        }

        void OnButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.Sync))
                return;

            Dispatcher.Dispatch(EnableSyncModeAction.From(!m_SyncEnabledSelector.GetValue()));
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.Sync)
            {
                m_Visibility = data.visible;
                m_Button.transform.parent.gameObject.SetActive(m_Visibility &&
                    m_ActiveDialogSelector.GetValue() != OpenDialogAction.DialogType.LandingScreen
                    );
            }
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.Sync)
            {
                m_Interactable = data.interactable;
                UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
            }
        }

        void UpdateButtonInteractable(bool toolbarsEnabled)
        {
            m_Button.interactable = toolbarsEnabled && m_Interactable &&
                UIStateManager.current.IsNetworkConnected;
        }

        void ProjectServerConnectionChanged(bool _)
        {
            UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
        }

        void NetworkReachabilityChanged(NetworkReachability _)
        {
            UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
        }
    }
}
