using System;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class ProjectOptionsUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        TextMeshProUGUI m_NameText;
        [SerializeField]
        TextMeshProUGUI m_StatusText;
        [SerializeField]
        TextMeshProUGUI m_DateText;
        [SerializeField]
        TextMeshProUGUI m_ServerText;
        [SerializeField]
        Button m_DownloadButton;
        [SerializeField]
        Button m_DeleteButton;
#pragma warning restore 649

        Project m_CurrentProject;
        int m_CurrentIndex;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            m_DownloadButton.onClick.AddListener(OnDownloadButtonClicked);
            m_DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        void InitProjectOption(Project project)
        {
            if (project == Project.Empty)
            {
                m_NameText.text = String.Empty;
                m_StatusText.text = String.Empty;
                m_DateText.text = String.Empty;
                m_ServerText.text = String.Empty;

                m_DownloadButton.interactable = false;
                m_DeleteButton.interactable = false;
                return;
            }

            m_NameText.text = project.name;
            m_StatusText.text = string.Empty; // TODO
            m_DateText.text =  project.lastPublished.ToShortDateString();
            m_ServerText.text = project.description;
            m_DownloadButton.interactable = project.isAvailableOnline;
            m_DeleteButton.interactable = ReflectPipelineFactory.HasLocalData(project);
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (data.selectedProjectOption != m_CurrentProject || data.projectOptionIndex != m_CurrentIndex)
            {
                //Init Option Dialog
                InitProjectOption(data.selectedProjectOption);
                m_CurrentProject = data.selectedProjectOption;
                m_CurrentIndex = data.projectOptionIndex;
            }
        }

        void OnDownloadButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.DownloadProject, m_CurrentProject));
        }

        void OnDeleteButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.RemoveProject, m_CurrentProject));
        }
    }
}
