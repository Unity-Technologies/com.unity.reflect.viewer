using System;
using System.Collections;
using System.IO;
using System.Text;
using TMPro;
using Unity.Reflect.IO;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectListItem : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Image m_ItemImage;
        [SerializeField]
        TextMeshProUGUI m_TitleText;
        [SerializeField]
        TextMeshProUGUI m_DescriptionText;
        [SerializeField]
        TextMeshProUGUI m_DateText;
        [SerializeField]
        Button m_ItemButton;
        [SerializeField]
        Button m_OptionButton;
        [SerializeField]
        Image m_ConnectedImage;
        [SerializeField]
        Image m_DisconnectedImage;
        [SerializeField]
        Image m_ThumbnailImage;

#pragma warning restore CS0649

        string m_ThumbnailPath;
        Project m_project;

        ProjectServerType m_ProjectServerType = ProjectServerType.None;


        public Project project => m_project;
        public Sprite projectThumbnail
        {
            get => m_ThumbnailImage.sprite;
            set
            {
                if(value != null)
                    m_ThumbnailImage.sprite = value;
            }
        }

        public ProjectServerType projectServerType => m_ProjectServerType;

        public event Action<Project> projectItemClicked;
        public event Action<Project> optionButtonClicked;

        void Start()
        {
            m_ItemButton.onClick.AddListener(OnItemButtonClicked);
            m_OptionButton.onClick.AddListener(OnOptionButtonClicked);
        }

        public void InitProjectItem(Project project, Sprite thumbnail)
        {
            m_project = project;
            m_TitleText.text = project.name;
            projectThumbnail = thumbnail;

            m_DescriptionText.text = project.description;
            if (project.description == "Local")
            {
                m_ProjectServerType = ProjectServerType.Local;
            }
            else if (project.description == "Cloud")
            {
                m_ProjectServerType = ProjectServerType.Cloud;
            }
            else if (project.description == "Network")
            {
                m_ProjectServerType = ProjectServerType.Network;
            }

            m_DateText.text = project.lastPublished.ToShortDateString();

            bool isConnected = project.isAvailableOnline;
            m_ConnectedImage.gameObject.SetActive(isConnected);
            m_DisconnectedImage.gameObject.SetActive(!isConnected);
        }

        public void SetHighlightString(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                m_TitleText.text = m_project.name;
            }
            else
            {
                var indexOf = m_project.name.IndexOf(search, StringComparison.OrdinalIgnoreCase);
                if (indexOf != -1)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<font=\"LiberationSans SDF\">");
                    int currentIndex = 0;

                    while (indexOf != -1)
                    {
                        sb.Append(m_project.name.Substring(currentIndex, indexOf - currentIndex));
                        sb.Append("<mark=#2096F3>");
                        sb.Append(m_project.name.Substring(indexOf, search.Length));
                        sb.Append("</mark>");
                        currentIndex = indexOf + search.Length;

                        indexOf = m_project.name.IndexOf(search, currentIndex, StringComparison.OrdinalIgnoreCase);
                    }
                    sb.Append(m_project.name.Substring(currentIndex));
                    m_TitleText.text = sb.ToString();
                }
            }
        }

        public void SetHighlight(bool highlight)
        {
            m_ItemImage.color = highlight ? UIConfig.projectItemSelectedColor : UIConfig.projectItemBaseColor;
        }

        void OnItemButtonClicked()
        {
            projectItemClicked?.Invoke(m_project);
        }

        void OnOptionButtonClicked()
        {
            optionButtonClicked?.Invoke(m_project);
        }
    }
}
