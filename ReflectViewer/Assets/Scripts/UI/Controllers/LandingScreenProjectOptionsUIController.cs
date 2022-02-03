using System;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Viewer;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class LandingScreenProjectOptionsUIController : MonoBehaviour
    {
        public event Action downloadButtonClicked;
        public event Action deleteButtonClicked;

#pragma warning disable 649
        [SerializeField]
        TextMeshProUGUI m_NameText;
        [SerializeField]
        TextMeshProUGUI m_ServerText;
        [SerializeField]
        TextMeshProUGUI m_OrganizationText;
        [SerializeField]
        TextMeshProUGUI m_DateText;
        [SerializeField]
        Button m_DownloadButton;
        [SerializeField]
        TextMeshProUGUI m_DownloadButtonLabel;
        [SerializeField]
        Button m_DeleteButton;
        [SerializeField]
        TextMeshProUGUI m_DeleteButtonLabel;
#pragma warning restore 649

        Project m_Project;

        RectTransform m_RectTransform;

        float m_DesiredHeight;
        float m_ContentHeight;

        void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();

            m_DownloadButton.onClick.AddListener(OnDownloadButtonClicked);
            m_DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        void OnEnable()
        {
            ReflectProjectsManager.projectStatusChanged += OnProjectStatusChanged;
            ReflectProjectsManager.projectDownloadProgressChanged += OnProjectDownloadProgressChanged;
        }

        void OnDisable()
        {
            m_Project = null;

            ReflectProjectsManager.projectStatusChanged -= OnProjectStatusChanged;
            ReflectProjectsManager.projectDownloadProgressChanged -= OnProjectDownloadProgressChanged;
        }

        void OnProjectStatusChanged(Project project, ProjectsManager.Status status)
        {
            if (project != m_Project)
                return;

            Refresh(status);
        }

        void OnProjectDownloadProgressChanged(Project project, int progress, int total)
        {
            if (project != m_Project)
                return;

            m_DownloadButtonLabel.text = $"Downloading {Mathf.RoundToInt((progress/(float)total) * 100)}%";
        }

        void Refresh()
        {
            Refresh(ReflectProjectsManager.GetStatus(m_Project));
        }

        void Refresh(ProjectsManager.Status status)
        {
            if (m_Project == Project.Empty)
            {
                m_NameText.text = string.Empty;
                m_OrganizationText.text = string.Empty;
                m_DateText.text = string.Empty;
                m_ServerText.text = string.Empty;

                m_DownloadButton.interactable = false;
                m_DeleteButton.interactable = false;
                return;
            }

            var organizationName = m_Project.UnityProject.Organization?.Name;
            if (string.IsNullOrEmpty(organizationName))
                organizationName = "None";

            m_NameText.text = m_Project.name;
            m_ServerText.text = m_Project.description;
            m_OrganizationText.text = organizationName;

            if (m_Project is EmbeddedProject)
            {
                m_DateText.text = "-";
                SetButtonVisible(m_DownloadButton, false);
                SetButtonVisible(m_DeleteButton, false);
            }
            else
            {
                var isLocal = m_Project.IsLocal;
                var hasUpdate = m_Project.hasUpdate;

                m_DateText.text =  m_Project.lastPublished.ToShortDateString();

                var displayDeleteButton = false;

                switch (status)
                {
                    case ProjectsManager.Status.QueuedForDownload:
                        m_DownloadButtonLabel.text = "Downloading (Queued)";
                        m_DownloadButton.interactable = false;
                        m_DeleteButton.interactable = false;
                        displayDeleteButton = isLocal;
                        break;
                    case ProjectsManager.Status.Downloading:
                        m_DownloadButtonLabel.text = "Downloading ...";
                        m_DownloadButton.interactable = false;
                        m_DeleteButton.interactable = false;
                        displayDeleteButton = isLocal;
                        break;
                    case ProjectsManager.Status.QueuedForDelete:
                        m_DeleteButtonLabel.text = "Removing (Queued)";
                        m_DownloadButton.interactable = false;
                        m_DeleteButton.interactable = false;
                        displayDeleteButton = true;
                        break;
                    case ProjectsManager.Status.Deleting:
                        m_DeleteButtonLabel.text = "Removing ...";
                        m_DownloadButton.interactable = false;
                        m_DeleteButton.interactable = false;
                        displayDeleteButton = true;
                        break;
                    case ProjectsManager.Status.Downloaded:
                    case ProjectsManager.Status.Deleted:
                    case ProjectsManager.Status.Unknown:
                    default:
                        if (hasUpdate)
                        {
                            m_DownloadButtonLabel.text = "Download Latest";
                        }
                        else
                        {
                            m_DownloadButtonLabel.text = isLocal ? "Redownload" : "Download";
                        }

                        m_DeleteButtonLabel.text = "Remove From Device";
                        m_DownloadButton.interactable = m_Project.IsConnectedToServer;
                        m_DeleteButton.interactable = isLocal;
                        displayDeleteButton = isLocal;
                        break;
                }

                SetButtonVisible(m_DownloadButton, true);
                SetButtonVisible(m_DeleteButton, displayDeleteButton);
            }

            UpdatePosition();
        }

        public void Show(Project project, float desiredHeight, float contentHeight)
        {
            if (m_Project != null)
                return;

            m_Project = project;
            m_DesiredHeight = desiredHeight;
            m_ContentHeight = contentHeight;

            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }

        static void SetButtonVisible(Button button, bool enabled)
        {
            button.gameObject.SetActive(enabled);
        }

        void OnDownloadButtonClicked()
        {
            Dispatcher.Dispatch(DownloadProjectAction.From(m_Project));
            Refresh();

            downloadButtonClicked?.Invoke();
        }

        void OnDeleteButtonClicked()
        {
            Dispatcher.Dispatch(RemoveProjectAction.From(m_Project));
            Refresh();

            deleteButtonClicked?.Invoke();
        }

        void UpdatePosition()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform); // Make sure to refresh size since it might have changed.
            var minPos = m_RectTransform.rect.height - m_ContentHeight;

            var pos = Mathf.Clamp(m_DesiredHeight, minPos, 0.0f);

            m_RectTransform.anchoredPosition = new Vector2(-86.0f, pos); // Placed by hand to align with ProjectListItem chevron
        }
    }
}
