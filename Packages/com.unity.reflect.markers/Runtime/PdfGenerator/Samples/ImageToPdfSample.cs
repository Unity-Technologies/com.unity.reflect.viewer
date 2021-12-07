using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageToPdfSample : MonoBehaviour
{
    [SerializeField] private string[] paths;

    private void Start()
    {
        List<byte[]> markers = new List<byte[]>();
        for (int i = 0; i < paths.Length; i++)
        {
            markers.Add(File.ReadAllBytes(paths[i]));
        }

        string pdfPath = Application.temporaryCachePath + "/sample.pdf";
        MarkerPdfGenerator markersMarkerPdf = new MarkerPdfGenerator(markers);
        markersMarkerPdf.Generate(pdfPath);
        Application.OpenURL("file://" + Application.temporaryCachePath + "/sample.pdf");
    }
}

