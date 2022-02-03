using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class MarkerGraphicManager : MonoBehaviour
    {
        public bool GraphicsAvailable
        {
            get => m_GraphicsAvailable;
        }
        bool m_GraphicsAvailable = false;

        public Dictionary<SyncId, (Color32[], int, int, TextureFormat)> GeneratedGraphics => m_GeneratedGraphics;

        public event Action OnGraphicsAvailable;

        [SerializeField]
        MarkerGraphicGenerator m_GeneratorPrefab;
        MarkerGraphicGenerator m_Generator;

        [SerializeField]
        MarkerController m_MarkerController;

        //(Color32[], int, int, TextureFormat) m_BlankGraphic;
        //public (Color32[], int, int, TextureFormat) BlankGraphic => m_BlankGraphic;

        Dictionary<SyncId, (Color32[], int, int, TextureFormat)> m_GeneratedGraphics = new Dictionary<SyncId, (Color32[], int, int, TextureFormat)>();
        UnityProject m_UnityProject;

        IEnumerator m_Routine;

        public UnityProject UnityProject
        {
            set => m_UnityProject = value;
        }

        void Start()
        {
            m_MarkerController.OnMarkerListUpdated += HandleMarkerListUpdated;
            m_GraphicsAvailable = false;
        }

        void HandleMarkerListUpdated()
        {
            m_GraphicsAvailable = false;
        }

        void OnDestroy()
        {
            if (m_Generator)
                Destroy(m_Generator.gameObject);
            m_GeneratedGraphics.Clear();
        }

        public void Generate()
        {
            if (m_GraphicsAvailable)
                return;
            if (m_Routine != null)
                StopCoroutine(m_Routine);
            m_Routine = GenerateRoutine();
            StartCoroutine(m_Routine);
        }

        IEnumerator GenerateBlankMarker()
        {
            // Generate graphic for AR Tracking
            m_Generator.GenerateBlank();
            yield return null;
            var blankTexture = m_Generator.RenderToTexture();
            //m_BlankGraphic = (blankTexture.GetPixels32(), blankTexture.width, blankTexture.height, blankTexture.format);
            string markerpath = $"{Application.temporaryCachePath}/marker-blank.png";
            File.WriteAllBytes(markerpath, blankTexture.EncodeToPNG());
            Application.OpenURL($"file://{markerpath}");
        }

        IEnumerator GenerateRoutine()
        {
            if (m_GraphicsAvailable)
                yield break;

            if (!m_Generator)
                m_Generator = Instantiate(m_GeneratorPrefab);

            //yield return GenerateBlankMarker();
            // Generate graphics for individual markers
            List<SyncId> activeIds = new List<SyncId>();
            var markers = m_MarkerController.MarkerStorage.Markers;
            var parser = m_MarkerController.BarcodeDataParser;
            foreach (var marker in markers)
            {
                activeIds.Add(marker.Id);
                if (m_GeneratedGraphics.ContainsKey(marker.Id))
                    continue;

                m_Generator.GenerateGraphic(marker, m_UnityProject, parser);
                yield return null;
                var texture = m_Generator.RenderToTexture();

                m_GeneratedGraphics.Add(marker.Id, (texture.GetPixels32(), texture.width, texture.height, texture.format));
                yield return null;
                Destroy(texture);
            }

            // Mark graphics as generated
            m_Routine = null;
            m_GraphicsAvailable = true;
            Destroy(m_Generator);
            OnGraphicsAvailable?.Invoke();
        }

        public void PrintAll()
        {
            StartCoroutine(GeneratePDF());
        }

        public void PrintMarkers(List<SyncId> markers)
        {
            StartCoroutine(GeneratePDF(markers));
        }

        public void PrintMarker(SyncId markerId)
        {
            StartCoroutine(GenerateSinglePDF(markerId));
        }

        IEnumerator GeneratePDF()
        {
            //@@TODO Notify the user that the process is running in the background.
            yield return GenerateRoutine();

            List<byte[]> markerPng = new List<byte[]>();
            foreach (var graphic in GeneratedGraphics)
            {
                Texture2D newTexture = new Texture2D(graphic.Value.Item2, graphic.Value.Item3, graphic.Value.Item4, false);
                newTexture.SetPixels32(graphic.Value.Item1);
                newTexture.Apply();
                yield return null;
                markerPng.Add(newTexture.EncodeToPNG());
            }
            var pdfGenerator = new MarkerPdfGenerator(markerPng);

            string markerpath = $"{Application.temporaryCachePath}/marker.pdf";
            pdfGenerator.Generate(markerpath);
            yield return null;

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
            Application.OpenURL($"file://{markerpath}");
#else
            NativeShare nativeShare = new NativeShare();
            nativeShare.AddFile(markerpath, "application/pdf");
            nativeShare.Share();
#endif
        }

        IEnumerator GeneratePDF(List<SyncId> markerids)
        {
            //@@TODO Notify the user that the process is running in the background.
            yield return GenerateRoutine();

            List<byte[]> markerPng = new List<byte[]>();

            foreach (var id in markerids)
            {
                var graphic = GeneratedGraphics[id];
                Texture2D newTexture = new Texture2D(graphic.Item2, graphic.Item3, graphic.Item4, false);
                newTexture.SetPixels32(graphic.Item1);
                newTexture.Apply();
                yield return null;
                markerPng.Add(newTexture.EncodeToPNG());
            }
            var pdfGenerator = new MarkerPdfGenerator(markerPng);

            string markerpath = $"{Application.temporaryCachePath}/marker.pdf";
            pdfGenerator.Generate(markerpath);
            yield return null;

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
            Application.OpenURL($"file://{markerpath}");
#else
            NativeShare nativeShare = new NativeShare();
            nativeShare.AddFile(markerpath, "application/pdf");
            nativeShare.Share();
#endif
        }

        IEnumerator GenerateSinglePDF(SyncId markerId)
        {
            //@@TODO Notify the user that the process is running in the background.
            yield return GenerateRoutine();

            List<byte[]> markerPng = new List<byte[]>();
            var graphic = GeneratedGraphics[markerId];
            Texture2D newTexture = new Texture2D(graphic.Item2, graphic.Item3, graphic.Item4, false);
            newTexture.SetPixels32(graphic.Item1);
            newTexture.Apply();
            yield return null;
            markerPng.Add(newTexture.EncodeToPNG());
            var pdfGenerator = new MarkerPdfGenerator(markerPng);

            string markerpath = $"{Application.temporaryCachePath}/marker.pdf";
            pdfGenerator.Generate(markerpath);
            yield return null;

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
            Application.OpenURL($"file://{markerpath}");
#else
            NativeShare nativeShare = new NativeShare();
            nativeShare.AddFile(markerpath, "application/pdf");
            nativeShare.Share();
#endif
        }
    }
}
