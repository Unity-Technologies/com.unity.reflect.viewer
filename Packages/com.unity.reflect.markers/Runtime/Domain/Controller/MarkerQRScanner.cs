 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Threading.Tasks;
using Unity.Reflect.Markers.Camera;
 using Unity.Reflect.Markers.Domain.Controller;
 using Unity.Reflect.Markers.Storage;
 using Unity.Reflect.Markers.UI;
 using UnityEngine;

 namespace Unity.Reflect.Markers.Selection
{
    public class MarkerQRScanner : MonoBehaviour
    {
        [SerializeField]
        BarcodeScanUI m_ScanUI;

        [SerializeField]
        MarkerController m_MarkerController;
        IBarcodeDecoder m_BarcodeDecoder;
        ICameraSource m_Source;

        List<string> m_Results = new List<string>();
        IEnumerator m_ResultHandler = null;
        object m_Lock = null;

        public bool IsScanning { get; private set; } = false;

        private void OnApplicationPause(bool pauseStatus)
        {
#if UNITY_WSA && !UNITY_EDITOR
            if (pauseStatus)
                Close();
#endif
        }

        private void OnApplicationFocus(bool hasFocus)
        {
#if UNITY_WSA && !UNITY_EDITOR
            if(CompatibilityChecker.isHololens2())
                if (!hasFocus)
                    Close();
#endif
        }

        private void OnApplicationQuit()
        {
#if UNITY_WSA && !UNITY_EDITOR
            Close();
#endif
        }

        void Start()
        {
            m_MarkerController.OnBarcodeScanOpen += Open;
            m_MarkerController.OnBarcodeScanCanceled += Close;

            m_ScanUI.OnCancel += m_MarkerController.CancelBarcode;
        }

        void OnDestroy()
        {
            Close();
            if (m_MarkerController != null)
            {
                m_MarkerController.OnBarcodeScanOpen -= Open;
                m_MarkerController.OnBarcodeScanCanceled -= Close;
                m_ScanUI.OnCancel -= m_MarkerController.CancelBarcode;
            }
        }

        public void Open()
        {
            if (IsScanning)
                return;

            //Turn this on right away to avoid a second scanner from starting right away.
            IsScanning = true;
            m_Source = m_MarkerController.CameraSource;
            m_Source.RequestedHeight = 720;
            m_Source.RequestedWidth = 1280;
            m_Source.Run();
            m_ScanUI.Open(m_Source.Texture);
            m_BarcodeDecoder = new QRBarcodeDecoder();
            _ = Scan();

            if (m_ResultHandler != null)
                StopCoroutine(m_ResultHandler);

            m_ResultHandler = ResultHandler();
            StartCoroutine(m_ResultHandler);
        }

        public void Close()
        {
            Debug.Log($"[ARInstructionUI] Closing barcode scanner: IsScanning {IsScanning}");
            if (!IsScanning)
                return;
            IsScanning = false;
            m_ScanUI.Close();
            m_MarkerController.CameraSource?.Stop();
            m_BarcodeDecoder?.Dispose();
            m_BarcodeDecoder = null;
            m_Source = null;
            m_MarkerController.BarcodeScannerExited();
        }

        async Task Scan()
        {
            while (IsScanning)
            {
                Color32Result image = await UpdateCamera();
                if (image != null)
                {
                    string result = null;
                    try
                    {
                        result = await m_BarcodeDecoder.Decode(image);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in Decoder {e}");
                    }
                    if (result == null)
                    {
                        continue;
                    }


                    // Trigger attached actions
                    try
                    {
                        m_Lock = true;
                        m_Results.Add(result);
                        m_Lock = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                else
                {
                    Debug.LogError("Update Camera Failed!");
                }

                await Task.Delay(100);
            }
        }

        IEnumerator ResultHandler()
        {
            while (IsScanning)
            {
                yield return new WaitUntil(()=>
                    m_Lock == null
                    && m_Results.Count > 0
                    );
                var results = new List<string>(m_Results);
                m_Results.Clear();
                for (int i = 0; i < results.Count; i++)
                {
                    if (m_MarkerController.BarcodeDataParser.TryParse(results[i], out IMarker marker))
                    {
                        if (marker != null)
                            m_MarkerController.ActiveMarker = marker;
                        Close();
                    }
                }
            }

            m_ResultHandler = null;
        }

        async Task<Color32Result> UpdateCamera()
        {
            while (!m_Source.Ready && IsScanning)
            {
                await Task.Delay(20);
            }

            if (!IsScanning)
                return null;
            return await m_Source.GrabFrame();
        }
    }
}
