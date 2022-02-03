using System;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    /// <summary>
    /// Manages the state and componets of the Marker System
    /// </summary>
    public interface IMarkerController
    {
        public bool Available { get; set; }
        public bool ReadOnly { get; set; }
        public bool LoadingComplete { get; set; }
        public Pose CurrentMarkerPose { get; set; }

        /// <summary>
        /// Marker that's actively being used by marker based AR
        /// </summary>
        public IMarker ActiveMarker { get; set; }

        /// <summary>
        /// Marker selected through DeepLink or other means, queued to be opened when AR is enabled.
        /// </summary>
        public string QueuedMarkerId { get; set; }
        public IMarkerStorage MarkerStorage { get; }
        public IImageTracker ImageTracker { get; set; }
        public ICameraSource CameraSource { get; set; }
        public IBarcodeDataParser BarcodeDataParser { get; set; }
        public IProjectLinkSource ProjectLinkSource { get; set; }
        public bool FoundPose { get; }
        public IAlignmentObject AlignedObject { get; set; }
        public IMarkerAnchorer MarkerAnchorer { get; set; }
        public IRenderedMarkerManager RenderedMarkerManager { get; set; }

        public event Action<Pose> OnPoseFound;
        public event Action<IMarker> OnMarkerUpdated;
        public event Action<Pose> OnMarkerPoseUpdated;
        public event Action OnMarkerListUpdated;
        public event Action OnAlignedObjectUpdated;
        public event Action OnBarcodeScanOpen;
        public event Action OnBarcodeScanCanceled;
        public event Action OnDataLoaded;
        public event Action OnBarcodeScanExit;
        public event Action<string> OnServiceUnsupported;
        public event Action<bool> OnServiceInitialized;
        public string UnsupportedMessage { get; set; }

        /// <summary>
        /// Create a new marker
        /// </summary>
        /// <param name="newMarker"></param>
        /// <returns></returns>
        public bool CreateMarker(IMarker newMarker);

        /// <summary>
        /// Write marker changes
        /// </summary>
        /// <param name="editedMarker"></param>
        /// <returns></returns>
        public bool EditMarker(IMarker editedMarker);

        /// <summary>
        /// Pick the first marker in the MarkerStorage, or create a new default.
        /// </summary>
        /// <returns></returns>
        public IMarker FirstOrDefaultMarker();

        /// <summary>
        /// Visualize the marker alignment.
        /// Requires CurrentMarkerPose, and AlignedObject
        /// </summary>
        /// <param name="data">Marker to align to</param>
        public void Visualize(IMarker data);

        /// <summary>
        /// Start the barcode scanner
        /// </summary>
        public void ScanBarcode();

        /// <summary>
        /// Stop active barcode scanner
        /// </summary>
        public void CancelBarcode();

        /// <summary>
        /// Called when QR scanner exits, triggers OnBarcodeScanExit
        /// </summary>
        public void BarcodeScannerExited();

    }
}
