using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.VisuaLive.Markers.UI
{
    [RequireComponent(typeof(Button))]
    public class PopoutButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button = null;

        public UnityEvent onClick
        {
            get
            {
                if (!button)
                    button = GetComponent<Button>();
                return button.onClick;
            } 
        }
    }
}
