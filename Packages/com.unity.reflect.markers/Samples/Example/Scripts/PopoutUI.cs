using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.VisuaLive.Markers.UI
{
    /// <summary>
    /// Popout menu opens with a PopoutButtonUI
    /// Then closes with an internal button
    /// </summary>
    public class PopoutUI : MonoBehaviour
    {
        [SerializeField] private GameObject popoutContainer = null;
        public Action<bool> OnToggled { get; set; } = null;

        private bool _open = false;
        public bool IsOpen
        {
            get => _open;
        }

        public void Open()
        {
            if (IsOpen)
                return;
            _open = true;
            popoutContainer.SetActive(true);
            foreach (var button in popoutContainer.GetComponentsInChildren<PopoutButtonUI>())
            {
                button.onClick.AddListener(Close);
            }
            OnToggled?.Invoke(true);
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            _open = false;
            popoutContainer.SetActive(false);
            foreach (var button in popoutContainer.GetComponentsInChildren<PopoutButtonUI>())
            {
                button.onClick.RemoveListener(Close);
            }
            OnToggled?.Invoke(false);
        }
    }
}
