using System;
using TMPro;
using Unity.Reflect.Markers.Selection;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.Examples
{
    public class MarkerScanUI : MonoBehaviour
    {
        [SerializeField] private RawImage cameraImageDisplay = null;
        [SerializeField] private Button cancelButton = null;
        [SerializeField] private Button acceptButton = null;

        [SerializeField] private Transform scanContainer = null;
        [SerializeField] private TextMeshProUGUI scanMessageHeading = null;
        [SerializeField] private TextMeshProUGUI scanMessageSubheading = null;
        [SerializeField] private GameObject markerGizmo = null;
        private GameObject _spawnedGizmo;

        public Action OnAccept { get; set; } = null;
        public Action OnCancel { get; set; } = null;

        public void Open(Texture cameraTexture = null)
        {
            HideCancel();
            HideAccept();
            HideGizmo();
            if (cameraTexture != null)
            {
                cameraImageDisplay.texture = cameraTexture;
                cameraImageDisplay.gameObject.SetActive(true);
            }
            else
            {
                cameraImageDisplay.gameObject.SetActive(false);
            }
            scanContainer.gameObject.SetActive(true);
        }

        public void Close()
        {
            scanContainer.gameObject.SetActive(false);
            HideCancel();
            HideAccept();
            HideGizmo();
            OnAccept = null;
            OnCancel = null;
        }

        public void ShowCancel()
        {
            cancelButton.onClick.AddListener(HandleCancel);
            cancelButton.gameObject.SetActive(true);
        }

        public void HideCancel()
        {
            cancelButton.onClick.RemoveListener(HandleCancel);
            cancelButton.gameObject.SetActive(false);
        }

        public void ShowAccept()
        {
            acceptButton.onClick.AddListener(HandleAccept);
            acceptButton.gameObject.SetActive(true);
        }

        public void HideAccept()
        {
            acceptButton.onClick.RemoveListener(HandleAccept);
            acceptButton.gameObject.SetActive(false);
        }

        public void UpdateInstructions(string heading, string subheading)
        {
            scanMessageHeading.text = heading;
            scanMessageSubheading.text = subheading;
        }

        public void ShowGizmo(Pose worldPose)
        {
            if (!_spawnedGizmo)
                _spawnedGizmo = Instantiate(markerGizmo);
            _spawnedGizmo.SetActive(true);
            _spawnedGizmo.transform.position = worldPose.position;
            _spawnedGizmo.transform.rotation = worldPose.rotation;
        }

        public void HideGizmo()
        {
            if (_spawnedGizmo)
                Destroy(_spawnedGizmo);
        }

        void HandleCancel()
        {
            OnCancel?.Invoke();
        }

        void HandleAccept()
        {
            OnAccept?.Invoke();
        }
    }
}
