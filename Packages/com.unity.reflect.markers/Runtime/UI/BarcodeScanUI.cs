using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.UI
{
    public class BarcodeScanUI : MonoBehaviour
    {
        [SerializeField]
        RawImage m_CameraImageDisplay;
        [SerializeField]
        RectTransform m_CameraImageContainer;
        [SerializeField]
        ButtonControl m_CancelButton;
        [SerializeField]
        ButtonControl m_AcceptButton;
        [SerializeField]
        Image m_BarcodeFill;
        [SerializeField]
        float m_FillSpeed = 1f;

        [SerializeField]
        Canvas m_UICanvas;

        public event Action OnAccept = null;
        public event Action OnCancel = null;

        float m_fillAmmount = 0f;
        float m_fillPause = 0f;

        const string k_Instruction = "Fit barcode within brackets";

        public void Open(Texture cameraTexture = null)
        {
            ShowCancel();
            ShowAccept();
            if (cameraTexture != null)
            {
                m_CameraImageDisplay.texture = cameraTexture;
                m_CameraImageContainer.gameObject.SetActive(true);
                StartCoroutine(ScaleImageToTexture());
            }
            else
            {
                m_CameraImageContainer.gameObject.SetActive(false);
            }

            m_UICanvas.enabled = true;
        }

        /// <summary>
        /// Scale the input image to the image container
        /// </summary>
        IEnumerator ScaleImageToTexture()
        {
            var parentContainer = m_CameraImageDisplay.transform.parent.GetComponent<RectTransform>();
            var parentRect = parentContainer.rect;
            var imageContainer = m_CameraImageDisplay.GetComponent<RectTransform>();

            var imgWidth = m_CameraImageDisplay.texture.width;
            var imgHeight = m_CameraImageDisplay.texture.height;

            while (imgWidth < 100 || imgHeight < 100)
            {
                yield return null;
                imgWidth = m_CameraImageDisplay.texture.width;
                imgHeight = m_CameraImageDisplay.texture.height;
            }

            float imgRatio = (float)imgWidth / (float)imgHeight;

            var height = parentRect.height;
            var width = height * imgRatio;
            if (width > parentRect.width)
            {
                width = parentRect.width;
                height = width / imgRatio;
            }

            imageContainer.sizeDelta = new Vector2(width, height);
            Debug.Log($"Image size: {imgWidth}x{imgHeight} parent size {parentRect.width}x{parentRect.height}");
        }

        public void Close()
        {
            m_UICanvas.enabled = false;
            HideCancel();
            HideAccept();
        }

        public void ShowCancel()
        {
            if (!m_CancelButton)
                return;
            m_CancelButton.onControlUp.AddListener(HandleCancelButton);
            m_CancelButton.gameObject.SetActive(true);
        }

        public void HideCancel()
        {
            if (!m_CancelButton)
                return;
            m_CancelButton.onControlUp.RemoveListener(HandleCancelButton);
            m_CancelButton.gameObject.SetActive(false);
        }

        public void ShowAccept()
        {
            if (!m_AcceptButton)
                return;
            m_AcceptButton.onControlUp.AddListener(HandleAcceptButton);
            m_AcceptButton.gameObject.SetActive(true);
        }

        public void HideAccept()
        {
            if (!m_AcceptButton)
                return;
            m_AcceptButton.onControlUp.RemoveListener(HandleAcceptButton);
            m_AcceptButton.gameObject.SetActive(false);
        }

        void HandleCancelButton(BaseEventData evt)
        {
            Cancel();
        }

        void HandleAcceptButton(BaseEventData evt)
        {
            Accept();
        }

        public void Accept()
        {
            OnAccept?.Invoke();
            Close();
        }

        public void Cancel()
        {
            OnCancel?.Invoke();
            Close();
        }

        void Update()
        {
            FillAnimate();
        }

        void FillAnimate()
        {
            if (!m_UICanvas.enabled && m_BarcodeFill != null)
                return;
            m_fillAmmount += m_FillSpeed * Time.deltaTime;
            if (m_fillAmmount >= 1f)
            {
                m_fillPause += m_fillAmmount - 1;
                m_BarcodeFill.fillAmount = 1f;
                if (m_fillPause > 1f)
                {
                    m_fillAmmount = 0f;
                    m_fillPause = 0f;
                }
            }
            else
            {
                m_BarcodeFill.fillAmount = m_fillAmmount;
            }
        }
    }
}
