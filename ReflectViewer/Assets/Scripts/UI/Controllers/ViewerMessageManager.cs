using System;
using System.Collections;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ViewerMessageManager : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        StatusUIController m_StatusDialog;
        [SerializeField]
        StatusUIController m_StatusWarningDialog;
        [SerializeField]
        float m_WaitingDelayToCloseDialog = 3f;
        [SerializeField]
        Button m_StatusDialogDismissButton;
        [SerializeField]
        Button m_StatusWarningDialogDismissButton;
#pragma warning restore CS0649

        DialogWindow m_StatusDialogWindow;
        DialogWindow m_StatusWarningDialogWindow;

        bool m_InstructionMode = false;

        Coroutine m_StatusDialogCloseCoroutine;
        Coroutine m_StatusWarningDialogCloseCoroutine;
        WaitForSeconds m_WaitDelay;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_WaitDelay = new WaitForSeconds(m_WaitingDelayToCloseDialog);

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(MessageManagerContext.current, nameof(IStatusMessageData.isInstructionMode), OnInstructionModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<StatusMessageData>(MessageManagerContext.current, nameof(IStatusMessageData.statusMessageData), OnStatusMessageChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(MessageManagerContext.current, nameof(IStatusMessageData.isClearAll), OnClearStatus));

            m_StatusDialogWindow = m_StatusDialog.GetComponent<DialogWindow>();
            m_StatusWarningDialogWindow = m_StatusWarningDialog.GetComponent<DialogWindow>();

            m_StatusDialogDismissButton.onClick.AddListener(CloseStatusDialog);
            m_StatusWarningDialogDismissButton.onClick.AddListener(CloseStatusWarningDialog);
        }

        void Start()
        {
            m_StatusDialogWindow.Close();
            m_StatusWarningDialogWindow.Close();
        }

        void OnClearStatus(bool newData)
        {
            if (newData)
            {
                ClearAllMessage();
            }
        }

        public void ClearAllMessage()
        {
            CloseAllDialogs();
        }

        void OnStatusMessageChanged(StatusMessageData newData)
        {
            SetStatusMessage(newData.text, newData.type);
        }

        public void SetStatusMessage(string text, StatusMessageType type = StatusMessageType.Info)
        {
            if (m_StatusWarningDialogWindow != null && m_StatusDialogWindow != null)
            {
                if (type == StatusMessageType.Warning)
                {
                    m_StatusWarningDialog.message = text;
                    m_StatusWarningDialogWindow.Open();

                    if (m_StatusWarningDialogCloseCoroutine != null)
                    {
                        StopCoroutine(m_StatusWarningDialogCloseCoroutine);
                        m_StatusWarningDialogCloseCoroutine = null;
                    }

                    m_StatusWarningDialogCloseCoroutine = StartCoroutine(WaitCloseStatusWarningDialog());
                }
                else
                {
                    if (type == StatusMessageType.Info)
                    {
                        if (m_InstructionMode)
                            return;
                    }

                    m_StatusDialog.message = text;
                    m_StatusDialogWindow.Open();

                    if (m_StatusDialogCloseCoroutine != null)
                    {
                        StopCoroutine(m_StatusDialogCloseCoroutine);
                        m_StatusDialogCloseCoroutine = null;
                    }

                    if (type == StatusMessageType.Info)
                    {
                        m_StatusDialogCloseCoroutine = StartCoroutine(WaitCloseStatusDialog());
                    }
                }
            }
        }

        void OnInstructionModeChanged(bool newData)
        {
            m_InstructionMode = newData;
        }

        public void CloseAllDialogs()
        {
            CloseStatusDialog();
            CloseStatusWarningDialog();
        }

        public void CloseStatusDialog()
        {
            m_StatusDialogWindow?.Close();
            if (m_StatusDialogCloseCoroutine != null)
            {
                StopCoroutine(m_StatusDialogCloseCoroutine);
                m_StatusDialogCloseCoroutine = null;
            }
        }

        public void CloseStatusWarningDialog()
        {
            m_StatusWarningDialogWindow?.Close();
            if (m_StatusWarningDialogCloseCoroutine != null)
            {
                StopCoroutine(m_StatusWarningDialogCloseCoroutine);
                m_StatusWarningDialogCloseCoroutine = null;
            }
        }

        IEnumerator WaitCloseStatusDialog()
        {
            yield return m_WaitDelay;

            m_StatusDialogWindow.Close();
            m_StatusDialogCloseCoroutine = null;
        }

        IEnumerator WaitCloseStatusWarningDialog()
        {
            yield return m_WaitDelay;

            m_StatusWarningDialogWindow.Close();
            m_StatusWarningDialogCloseCoroutine = null;
        }
    }
}
