using System;
using System.Collections;
using Unity.TouchFramework;
using UnityEngine;

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
#pragma warning restore CS0649

        DialogWindow m_StatusDialogWindow;
        DialogWindow m_StatusWarningDialogWindow;

        bool m_InstructionMode = false;

        Coroutine m_StatusDialogCloseCoroutine;
        Coroutine m_StatusWarningDialogCloseCoroutine;
        WaitForSeconds m_WaitDelay;

        void Awake()
        {
            m_StatusDialogWindow = m_StatusDialog.GetComponent<DialogWindow>();
            m_StatusWarningDialogWindow = m_StatusWarningDialog.GetComponent<DialogWindow>();
            m_WaitDelay = new WaitForSeconds(m_WaitingDelayToCloseDialog);


            m_StatusDialogWindow.Close();
            m_StatusWarningDialogWindow.Close();
        }

        public void ClearAllMessage()
        {
            CloseDialog();
        }

        public void SetStatusMessage(string text, StatusMessageType type = StatusMessageType.Info)
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

        public void SetInstructionMode(bool mode)
        {
            m_InstructionMode = mode;
        }

        public void SetInstructionMessage(string text)
        {

        }

        public void CloseDialog()
        {
            m_StatusDialogWindow.Close();
            if (m_StatusDialogCloseCoroutine != null)
            {
                StopCoroutine(m_StatusDialogCloseCoroutine);
                m_StatusDialogCloseCoroutine = null;
            }

            m_StatusWarningDialogWindow.Close();
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
