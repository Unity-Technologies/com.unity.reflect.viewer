using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Reflect.Markers.ImageTracker;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Markers.UI;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.Examples
{
    public class SimpleMarkerDemo : MonoBehaviour
    {
        [SerializeField]
        Marker _defaultMarker = new Marker()
        {
            Id = new SyncId("42"),
            Name = "Test marker"
        };

        [SerializeField]
        Transform _defaultPoseLocation = null;
        [SerializeField]
        LocalMarkerStore _markerStorage = null;
        [SerializeField]
        ImageTrackerBase _imageTracker = null;
        [SerializeField]
        MarkerScanUI _scanUI = null;
        [SerializeField]
        BasicTransformUI _transformUI = null;
        [SerializeField]
        ExampleModelController _modelController = null;
        [SerializeField]
        Button _updateMarkerButton = null;

        private Pose _currentMarkerPose = Pose.identity;
        private Pose _acceptedPose = Pose.identity;
        private IMarker _foundMarker = null;

        // Start is called before the first frame update
        void Start()
        {
            if (_markerStorage.Markers.Count == 0)
            {
                Debug.Log("Creating default marker");
                _markerStorage.Create(_defaultMarker);
            }
            else
            {
                Debug.Log($"Markers are already available: {_markerStorage.Markers.Count} total");
            }

            // For testing purposes, just use first marker.
            _foundMarker = _markerStorage.Markers[0];
            _currentMarkerPose = new Pose(_defaultPoseLocation.position, _defaultPoseLocation.rotation);
            _imageTracker.OnTrackedFound += HandleTrackableFound;
            _imageTracker.OnTrackedPositionUpdate += HandleTrackableUpdate;
            _updateMarkerButton.onClick.AddListener(HandleUpdateMarker);
            _imageTracker.Run();
            _scanUI.Open();
            // Currently no cancel action.
            _scanUI.HideCancel();
            _scanUI.UpdateInstructions("Center on Marker", "Fit marker image within the center of the screen.");
            _scanUI.OnAccept = ActivateMarker;
            _scanUI.ShowAccept();
        }

        private void OnDestroy()
        {
            if (_scanUI)
            {

                _scanUI.Close();
                _scanUI.OnCancel = null;
                _scanUI.OnAccept = null;
            }
            if (_imageTracker.IsTracking)
            {
                _imageTracker.Stop();
                _imageTracker.OnTrackedFound -= HandleTrackableFound;
                _imageTracker.OnTrackedPositionUpdate -= HandleTrackableUpdate;
            }
        }

        public void HandleTrackableFound(Pose pose, string trackableId)
        {
            Debug.Log($"Trackable Found: {trackableId}");
            if (_markerStorage.Markers == null || _markerStorage.Markers.Count == 0)
            {
                Debug.LogError("No Markers");
                return;
            }
            // Grab first marker for now
            //_foundMarker = _markerStorage.Get(_markerStorage.Markers[0].ID);
            if (_foundMarker == null)
            {
                Debug.LogError("No Marker available!");
                return;
            }
            Debug.Log($"Aligning to {_foundMarker.Name}");
            _currentMarkerPose = pose;
            _scanUI.ShowGizmo(pose);
            _scanUI.OnAccept = ActivateMarker;
            _scanUI.ShowAccept();
        }

        public void HandleTrackableUpdate(Pose pose, string trackableId)
        {
            _currentMarkerPose = pose;
            _scanUI.ShowGizmo(pose);
        }

        public void ActivateMarker()
        {
            Debug.Log("Aligning Object");
            Debug.Log(_modelController.Model);
            _acceptedPose = _currentMarkerPose;
            var result = Marker.AlignObject(_foundMarker, new TransformData(_modelController.Model.transform), _acceptedPose);
            _modelController.Model.transform.position = result.position;
            _modelController.Model.transform.rotation = result.rotation;
            _modelController.Model.transform.localScale = result.scale;
            Debug.Log("Opening the transform tools");
            _transformUI.Open(_foundMarker, _modelController.Model.transform);
        }

        void HandleUpdateMarker()
        {
            if (_foundMarker == null)
                return;

            Marker.SetWorldPosition(_foundMarker, _modelController.Model.transform, _acceptedPose);
            _markerStorage.Update((Marker)_foundMarker);
        }
    }
}
