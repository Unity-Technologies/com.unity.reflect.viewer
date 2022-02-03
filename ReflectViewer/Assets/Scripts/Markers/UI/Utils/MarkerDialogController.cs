using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class MarkerDialogController : MonoBehaviour
    {
        [SerializeField]
        MarkerController m_MarkerController;

        [SerializeField]
        ToolButton m_DialogButton;

        [SerializeField]
        GameObject m_EditPanel;
        [SerializeField]
        GameObject m_ListPanel;

        DialogWindow m_MarkerDialogWindow;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        public DialogWindow MarkerDialog => m_MarkerDialogWindow;
        public event Action<bool> OnEditToggled;

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnStateDataChanged));
            m_DisposeOnDestroy.Add(m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode)));
        }

        void Start()
        {
            m_MarkerDialogWindow = GetComponent<DialogWindow>();
            // Initialize buttons
            m_DialogButton.buttonClicked += HandleDialogButton;

            // Close edit panel & dialog
            m_EditPanel.SetActive(false);
            m_MarkerDialogWindow.Close();

            // Open edit panel on a selected marker.
            m_MarkerController.OnMarkerUpdated += HandleMarkerSelected;
            m_MarkerController.OnServiceUnsupported += SetUnsupported;
            m_MarkerController.OnServiceInitialized += HandleServiceInitialized;

            m_DialogButton.button.interactable = true;
        }

        public void SetUnsupported(string unsupportedReason)
        {
        }

        public void HandleServiceInitialized(bool available)
        {
            m_DialogButton.button.interactable = (available && !m_MarkerController.ReadOnly);
        }

        void HandleDialogButton()
        {
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            bool open = m_MarkerDialogWindow.open;
            var dialogType = open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.Marker;

            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));

            if (!open && m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Normal)
            {
                Dispatcher.Dispatch(SetStatusMessage.From("To scan markers, open AR Mode."));
            }
        }

        void HandleMarkerSelected(IMarker newMarker)
        {
            if (newMarker == null)
            {
                m_EditPanel.SetActive(false);
            }
            else
            {
                m_EditPanel.SetActive(true);
            }
        }

        void OnStateDataChanged(OpenDialogAction.DialogType data)
        {
            if (m_MarkerDialogWindow != null)
            {
                bool open = data == OpenDialogAction.DialogType.Marker;
                m_DialogButton.selected = open;

                if (!open)
                {
                    SetEditPanel(false);
                }
                else if (open && m_MarkerController.ReadOnly)
                {
                    m_MarkerDialogWindow.Close();
                }
            }
        }

        public void SetEditPanel(bool open)
        {
            m_EditPanel.SetActive(open);
            OnEditToggled?.Invoke(open);
        }

        void OnDestroy()
        {
            m_DialogButton.buttonClicked -= HandleDialogButton;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }
    }
}
