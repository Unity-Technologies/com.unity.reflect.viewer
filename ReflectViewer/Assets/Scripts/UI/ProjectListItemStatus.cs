using System;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectListItemStatus : MonoBehaviour
    {
        public enum Status
        {
            Unknown,
            AvailableForDownload,
            AvailableOffline,
            UpdateAvailable,
            QueuedForDownload,
            Downloading,
            QueuedForDelete,
            Deleting,
            DownloadError,
            DeleteError,
            ConnectionError,
        }

#pragma warning disable CS0649
        [SerializeField]
        GameObject m_Unknown;
        [SerializeField]
        Button m_DownloadButton;
        [SerializeField]
        GameObject m_AvailableOffline;
        [SerializeField]
        Button m_UpdateAvailable;
        [SerializeField]
        GameObject m_QueuedForDownload;
        [SerializeField]
        GameObject m_QueuedForDelete;
        [SerializeField]
        ProgressIndicatorControl m_DownloadIndicator;
        [SerializeField]
        ProgressIndicatorControl m_DeleteIndicator;
        [SerializeField]
        GameObject m_DownloadError;
        [SerializeField]
        GameObject m_DeleteError;
        [SerializeField]
        GameObject m_ConnectionError;
#pragma warning restore CS0649

        GameObject m_Active;

        void Awake()
        {
            if (m_Unknown != null)
                m_Unknown.SetActive(false);

            if (m_DownloadButton != null)
                m_DownloadButton.gameObject.SetActive(false);

            if (m_AvailableOffline != null)
                m_AvailableOffline.SetActive(false);

            if (m_UpdateAvailable != null)
                m_UpdateAvailable.gameObject.SetActive(false);

            if (m_QueuedForDownload != null)
                m_QueuedForDownload.SetActive(false);

            if (m_DownloadIndicator != null)
            {
                m_DownloadIndicator.Initialize();
                m_DownloadIndicator.gameObject.SetActive(false);
            }

            if (m_DownloadError != null)
                m_DownloadError.SetActive(false);

            if (m_QueuedForDelete != null)
                m_QueuedForDelete.SetActive(false);

            if (m_DeleteIndicator != null)
            {
                m_DeleteIndicator.Initialize();
                m_DeleteIndicator.gameObject.SetActive(false);
            }

            if (m_DeleteError != null)
                m_DeleteError.SetActive(false);

            if (m_ConnectionError != null)
                m_ConnectionError.SetActive(false);
        }

        public void SetStatus(Status status)
        {
            GameObject selected = null;

            switch (status)
            {
                case Status.AvailableForDownload:
                    selected = m_DownloadButton.gameObject;
                    break;

                case Status.AvailableOffline:
                    selected = m_AvailableOffline;
                    break;

                case Status.UpdateAvailable:
                    selected = m_UpdateAvailable.gameObject;
                    break;

                case Status.QueuedForDownload:
                    selected = m_QueuedForDownload;
                    break;

                case Status.Downloading:
                    selected = m_DownloadIndicator.gameObject;
                    break;

                case Status.DownloadError:
                    selected = m_DownloadError;
                    break;

                case Status.QueuedForDelete:
                    selected = m_QueuedForDelete;
                    break;

                case Status.Deleting:
                    selected = m_DeleteIndicator.gameObject;
                    break;

                case Status.DeleteError:
                    selected = m_DeleteError;
                    break;

                case Status.ConnectionError:
                    selected = m_ConnectionError;
                    break;

                case Status.Unknown:
                default:
                    selected = m_Unknown;
                    break;
            }

            if (m_Active == selected)
                return;

            if (m_Active != null)
                m_Active.SetActive(false);

            m_Active = selected;

            if (m_Active != null)
                m_Active.SetActive(true);

            if (status == Status.Downloading)
            {
                m_DownloadIndicator.StartLooping();
            }
            else
            {
                m_DownloadIndicator.StopLooping();
            }

            if (status == Status.Deleting)
            {
                m_DeleteIndicator.StartLooping();
            }
            else
            {
                m_DeleteIndicator.StopLooping();
            }
        }

        public void RegisterDownloadButtonClick(UnityAction call)
        {
            m_DownloadButton.onClick.AddListener(call);
            m_UpdateAvailable.onClick.AddListener(call);
        }

        public void SetProgress(float progress)
        {
            m_DownloadIndicator.SetProgress(progress);
        }
    }
}
