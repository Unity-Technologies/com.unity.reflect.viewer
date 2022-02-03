using System;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class ProjectTabButton : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_Button;
        [SerializeField]
        TextMeshProUGUI m_ButtonText;
        [SerializeField]
        GameObject m_Line;

        [SerializeField]
        SetLandingScreenFilterProjectServerAction.ProjectServerType m_Type;
#pragma warning restore CS0649

        public event Action<SetLandingScreenFilterProjectServerAction.ProjectServerType> projectTabButtonClicked;

        public SetLandingScreenFilterProjectServerAction.ProjectServerType type => m_Type;

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClicked);
        }

        void OnButtonClicked()
        {
            projectTabButtonClicked?.Invoke(m_Type);
        }

        public void SelectButton(bool select)
        {
            m_Line.SetActive(select);
            if (select)
            {
                m_ButtonText.color = UIConfig.projectTabTextSelectedColor;
            }
            else
            {
                m_ButtonText.color = UIConfig.projectTabTextBaseColor;
            }
        }
    }
}
