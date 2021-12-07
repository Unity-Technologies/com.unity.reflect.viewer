using System;
using System.Collections.Generic;
using System.IO;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using UnityEngine;

/// <summary>
/// Generates a PDF document for one or more markers using file paths.
/// Uses the PDF Pig package: https://github.com/UglyToad/PdfPig
/// </summary>
public class MarkerPdfGenerator
{
    private readonly List<byte[]> m_Markers;

    public MarkerPdfGenerator(List<byte[]> markers)
    {
        m_Markers = markers;
    }

    public void Generate(string pdfFileName)
    {
        //Set up the page
        PdfDocumentBuilder builder = new PdfDocumentBuilder();

        int pageCount = CalculatePageCount(m_Markers.Count);

        // page sizes in Millimeters
        // 210 wide, 279 high
        // 10mm page margins
        // 5mm marker margins
        // marker sizes
        // 100mm wide 120mm high
        // Images should be 1182 x 1418 px


        for (int i = 0; i < pageCount; i++)
        {
            PdfPageBuilder page = builder.AddPage(MillimeterToPoint(210), MillimeterToPoint(279));

            //Add the images and border
            double halfWidth = (page.PageSize.Width / 2);
            double halfHeight = page.PageSize.Height / 2;
            double pageMargin = MillimeterToPoint(5);
            double markerMargin = MillimeterToPoint(2);
            double markerWidth = MillimeterToPoint(100);
            double markerHeight = MillimeterToPoint(120);

            int index = i * 4;
            AddMarker(page, index, new PdfPoint(pageMargin, halfHeight + markerMargin), markerWidth, markerHeight);
            AddMarker(page, index + 1, new PdfPoint(halfWidth + markerMargin, halfHeight + markerMargin), markerWidth, markerHeight);
            AddMarker(page, index + 2, new PdfPoint(pageMargin, pageMargin), markerWidth, markerHeight);
            AddMarker(page, index + 3, new PdfPoint(halfWidth + markerMargin, pageMargin), markerWidth, markerHeight);
        }

        //Export the document
        byte[] documentBytes = builder.Build();
        File.WriteAllBytes(pdfFileName, documentBytes);
    }

    private int CalculatePageCount(int markerCount)
    {
        decimal count = (decimal)markerCount / 4;
        int pageCount = (int)Math.Ceiling(count);
        return pageCount;
    }

    private void AddMarker(PdfPageBuilder page, int markerIndex, PdfPoint bottomPoint, double width, double height)
    {
        if (markerIndex >= m_Markers.Count)
            return;

        byte[] marker = m_Markers[markerIndex];
        page.AddPng(marker, new PdfRectangle(bottomPoint.X, bottomPoint.Y, bottomPoint.X + width, bottomPoint.Y + height));
        page.DrawRectangle(bottomPoint, (decimal)width, (decimal)height);
    }

    static double MillimeterToPoint(double millimeter)
    {
        return (millimeter / 25.4) * 72;
    }
}
