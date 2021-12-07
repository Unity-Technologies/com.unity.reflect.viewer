using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.ImageTracker;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.Selection;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Markers.UI;
using UnityEngine;

namespace Unity.Reflect.Markers.Examples
{
    /// <summary>
    /// @@TODO: Fix this controller, the system changed and this is no longer a valid example
    /// </summary>
    public class QuickScanController : MonoBehaviour
    {
        [SerializeField] private MarkerScanUI _scanUI = null;
        [SerializeField] private WebCameraSource _webCameraSource = null;
        [SerializeField] private XRCameraSource _xrCameraSource = null;
        [SerializeField] private ImageTrackerBase _imageTracker = null;
        [SerializeField] private LocalMarkerStore _markerStorage = null;
        [SerializeField] private MarkerQRScanner _qrScanner = null;
        [SerializeField]
        MarkerController _markerController = null;

        private IBarcodeDecoder _decoder = null;
        private bool _usingWebcam = false;

        public Action<Pose> OnPlaced { get; set; } = null;
        private Pose _value = Pose.identity;
        private IMarker _foundMarker = null;

        public void Run()
        {
            ResumeQRSearch();
            _scanUI.OnCancel += Cancel;
            _scanUI.ShowCancel();
        }

        public void Cancel()
        {
            Close();
        }

        void ResumeQRSearch()
        {

            if (_xrCameraSource)
            {
                _usingWebcam = false;
                Debug.Log("Using XR Source");
                _markerController.CameraSource = _xrCameraSource;
                _qrScanner.Open();
                _scanUI.Open();
            }
            else if (_webCameraSource)
            {
                _usingWebcam = true;
                Debug.Log("Using Webcam Source");
                _markerController.CameraSource = _webCameraSource;
                _qrScanner.Open();
                _scanUI.Open(_webCameraSource.Texture);
            }
            else
            {
                Debug.Log("No Available Camera Source");
                return;
            }

            //_qrScanner.OnQRFound += HandleQRFound;
            _scanUI.UpdateInstructions("Scan Marker QR Code", "Fit QR code inside brackets, and hold still for a moment.");
        }

        void Close()
        {
            if (_scanUI)
            {

                _scanUI.Close();
                _scanUI.OnCancel = null;
                _scanUI.OnAccept = null;
            }
            if (_qrScanner)
            {
                _qrScanner.Close();
                //_qrScanner.OnQRFound -= HandleQRFound;
            }
            if (_imageTracker.IsTracking)
            {
                _imageTracker.Stop();
                _imageTracker.OnTrackedFound -= HandleTrackableFound;
                _imageTracker.OnTrackedPositionUpdate -= HandleTrackableUpdate;
            }
        }

        public void HandleQRFound(List<string> data)
        {
            foreach (var item in data)
            {
                HandleQRFound(item);
            }
        }

        public void HandleQRFound(string qrData)
        {
            //_qrScanner.OnQRFound -= HandleQRFound;
            Debug.Log($"QR Found {qrData}");
            _scanUI.UpdateInstructions("Loading Marker", "Searching for marker record, wait a moment.");
            _foundMarker = _markerStorage.Get(qrData);

            // If not a proper marker, keep seeking
            if (_foundMarker == null)
            {
                Debug.LogWarning("Marker not recognized, resuming search.");
                ResumeQRSearch();
                return;
            }

            // If this is a marker, begin gizmo alignment.
            _imageTracker.OnTrackedFound += HandleTrackableFound;
            _imageTracker.OnTrackedPositionUpdate += HandleTrackableUpdate;
            _scanUI.UpdateInstructions("Anchor Marker", "Face marker straight on, when gizmo aligns with graphic press Accept.");
            _imageTracker.Run();
        }

        public void HandleTrackableFound(Pose pose, string trackableId)
        {
            _scanUI.OnAccept += HandleAcceptPlacement;
            _scanUI.ShowAccept();
            _value = pose;
            _scanUI.ShowGizmo(pose);
        }

        public void HandleTrackableUpdate(Pose pose, string trackableId)
        {
            _value = pose;
            _scanUI.ShowGizmo(pose);
        }

        void HandleAcceptPlacement()
        {
            //if (_foundMarker != null)
                //_foundMarker.Value.AlignObject();
            OnPlaced?.Invoke(_value);
            Close();
        }
    }
}
