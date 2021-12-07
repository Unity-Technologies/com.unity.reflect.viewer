using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Unity.Reflect.Markers
{
    public class MarkerGraphicGenerator : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI m_ProjectNameField;
        [SerializeField]
        TextMeshProUGUI m_MarkerNameField;
        [SerializeField]
        RawImage m_QrImageField;
        [SerializeField]
        RenderTexture m_RenderTexture;
        [SerializeField]
        Texture2D m_BlankQR;

        BarcodeWriter m_Writer;

        public void GenerateGraphic(IMarker marker, UnityProject project, IBarcodeDataParser parser)
        {
            m_ProjectNameField.text = project.Name;
            m_MarkerNameField.text = marker.Name;
            var data = parser.Generate(marker, project);
            m_QrImageField.texture = GenerateQR(data, 256, 256);
            m_QrImageField.color = Color.white;
        }

        public void GenerateBlank()
        {
            m_ProjectNameField.text = "";
            m_MarkerNameField.text = "";
            m_QrImageField.texture = m_BlankQR;
            m_QrImageField.color = Color.white;
        }

        public Texture2D GenerateQR(string text, int width, int height)
        {
            var encoded = new Texture2D(width, height);
            var color32 = Encode(text, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();


            return encoded;
        }

        Color32[] Encode(string textForEncoding, int width, int height)
        {
            if (m_Writer == null)
            {
                m_Writer = new BarcodeWriter {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions {
                        Height = height,
                        Width = width,
                        Margin = 0,
                        ErrorCorrection = ErrorCorrectionLevel.Q,
                        PureBarcode = true
                    }};
            }

            return m_Writer.Write(textForEncoding);
        }

        void OnDestroy()
        {
            m_QrImageField.texture = null;
            m_Writer = null;
        }

        private Texture2D RenderToTexture(RenderTexture renderTexture)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, renderTexture.graphicsFormat, TextureCreationFlags.None);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }

        public Texture2D RenderToTexture()
        {
            return RenderToTexture(m_RenderTexture);
        }
    }
}
