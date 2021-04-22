using System;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class LandingScreenProjectOptionsUIController : MonoBehaviour
    {
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
        Button m_DeleteButton;
#pragma warning restore 649

        Project m_CurrentProject;

        void Start()
        {
            m_DownloadButton.onClick.AddListener(OnDownloadButtonClicked);
            m_DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        public void InitProjectOption(Project project)
        {
            m_CurrentProject = project;
            if (project == Project.Empty)
            {
                m_NameText.text = String.Empty;
                m_OrganizationText.text = String.Empty;
                m_DateText.text = String.Empty;
                m_ServerText.text = String.Empty;

                m_DownloadButton.interactable = false;
                m_DeleteButton.interactable = false;
                return;
            }

            var organizationName = project.UnityProject.Organization?.Name;
            if (string.IsNullOrEmpty(organizationName))
                organizationName = "None";

            m_NameText.text = project.name;
            m_ServerText.text = project.description;
            m_OrganizationText.text = organizationName;
            m_DateText.text =  project.lastPublished.ToShortDateString();
            m_DownloadButton.interactable = project.isAvailableOnline;
            m_DeleteButton.interactable = ReflectPipelineFactory.HasLocalData(project);
        }

        void OnDownloadButtonClicked()
        {
            m_CurrentProject.SetLocal(true);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.DownloadProject, m_CurrentProject));
        }

        void OnDeleteButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.RemoveProject, m_CurrentProject));
        }
    }
}
