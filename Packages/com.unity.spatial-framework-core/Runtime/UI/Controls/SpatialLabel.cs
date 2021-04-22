using System;
using TMPro;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Displays a label in space with a text renderer. The label is controlled via the ILabel interface
    /// </summary>
    public class SpatialLabel : MonoBehaviour, ILabel
    {
        [SerializeField, Tooltip("The text component to display the label")]
        TextMeshProUGUI m_LabelTextRenderer;

        /// <summary>
        /// The text component to display the label
        /// </summary>
        public TextMeshProUGUI labelTextRenderer
        {
            get => m_LabelTextRenderer;
            set => m_LabelTextRenderer = value;
        }

        Func<string> ILabel.getText { get; set; }

        Func<Vector3> ILabel.getPosition { get; set; }

        bool IPooledUI.active
        {
            get => gameObject.activeSelf;
            set
            {
                if (value && !gameObject.activeSelf) // Update before activating so components get new position and text when they enable
                    Update();

                gameObject.SetActive(value);
            }
        }

        void Update()
        {
            var label = ((ILabel)this);
            if (label.getText != null)
                m_LabelTextRenderer.text = label.getText();

            if (label.getPosition != null)
                transform.position = label.getPosition();
        }

        void IPooledUI.Destroy()
        {
            UnityObjectUtils.Destroy(gameObject);
        }
    }
}
