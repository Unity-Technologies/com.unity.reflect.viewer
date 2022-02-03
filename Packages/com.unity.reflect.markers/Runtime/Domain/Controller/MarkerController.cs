using System;
using Unity.Reflect.Actors;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class MarkerController : MonoBehaviour, IMarkerController
    {
        public bool LoadingComplete
        {
            get => m_LoadingComplete;
            set
            {
                m_LoadingComplete = value;
                if (value)
                    OnDataLoaded?.Invoke();
            }
        }

        public Pose CurrentMarkerPose
        {
            get
            {
                if (!m_MarkerAnchor)
                    return Pose.identity;
                return new Pose(m_MarkerAnchor.transform.position, m_MarkerAnchor.transform.rotation);
            }
            set
            {
                if (m_MarkerAnchorer != null)
                {
                    m_MarkerAnchor = m_MarkerAnchorer.Anchor(value);
                }
                if (!FoundPose)
                {
                    FoundPose = true;
                    OnPoseFound?.Invoke(CurrentMarkerPose);
                }
                OnMarkerPoseUpdated?.Invoke(CurrentMarkerPose);
            }
        }

        public IMarker ActiveMarker
        {
            get
            {
                return m_ActiveMarker;
            }
            set
            {
                if (value != null)
                    m_ActiveMarker = (Marker)value;
                else
                    m_ActiveMarker = default;

                OnMarkerUpdated?.Invoke(m_ActiveMarker);
            }
        }

        public string QueuedMarkerId
        {
            get => m_QueuedMarker;
            set => m_QueuedMarker = value;
        }

        public IMarkerStorage MarkerStorage
        {
            get => m_MarkerStorage;
            set
            {
                DetachFromMarkerStore();
                m_MarkerStorage = value;
                GetUpdatesFromMarkerStore();
            }
        }
        IMarkerStorage m_MarkerStorage;

        public IImageTracker ImageTracker
        {
            get => m_ImageTracker;
            set => m_ImageTracker = value;
        }

        public ICameraSource CameraSource
        {
            get => m_CameraSource;
            set => m_CameraSource = value;
        }

        public IBarcodeDataParser BarcodeDataParser
        {
            get => m_BarcodeDataParser;
            set => m_BarcodeDataParser = value;
        }

        public bool FoundPose
        {
            get;
            private set;
        }

        public IAlignmentObject AlignedObject
        {
            get => m_AlignedObject;
            set
            {
                m_AlignedObject = value;
                OnAlignedObjectUpdated?.Invoke();
            }
        }

        public IMarkerAnchorer MarkerAnchorer
        {
            get => m_MarkerAnchorer;
            set => m_MarkerAnchorer = value;
        }

        public IRenderedMarkerManager RenderedMarkerManager
        {
            get => m_RenderedMarkerManager;
            set => m_RenderedMarkerManager = value;
        }

        public IProjectLinkSource ProjectLinkSource
        {
            get => m_ProjectLinkSource;
            set => m_ProjectLinkSource = value;
        }
        IProjectLinkSource m_ProjectLinkSource;

        public string UnsupportedMessage
        {
            get => m_UnsupportedMessage;
            set
            {
                m_UnsupportedMessage = value;
                if (m_UnsupportedMessage != null)
                {
                    m_Available = false;
                    OnServiceUnsupported?.Invoke(m_UnsupportedMessage);
                    OnServiceInitialized?.Invoke(false);
                }
            }
        }
        string m_UnsupportedMessage = null;

        public bool Available
        {
            get => m_Available;
            set
            {
                m_Available = value;
                if(m_Available)
                    OnServiceInitialized?.Invoke(true);
            }
        }

        bool m_Available = false;

        public bool ReadOnly
        {
            get => m_ReadOnly;
            set
            {
                m_ReadOnly = value;
            }
        }

        bool m_ReadOnly = true;

        public event Action<Pose> OnPoseFound;
        public event Action<IMarker> OnMarkerUpdated;
        public event Action<Pose> OnMarkerPoseUpdated;
        public event Action OnMarkerListUpdated;
        public event Action OnAlignedObjectUpdated;
        public event Action OnBarcodeScanOpen;
        public event Action OnDataLoaded;
        public event Action OnBarcodeScanCanceled;
        public event Action OnBarcodeScanExit;
        public event Action<string> OnServiceUnsupported;
        public event Action<bool> OnServiceInitialized;

        GameObject m_MarkerAnchor;
        Marker m_ActiveMarker;
        IAlignmentObject m_AlignedObject;
        IImageTracker m_ImageTracker;
        ICameraSource m_CameraSource;
        IBarcodeDataParser m_BarcodeDataParser;
        IMarkerAnchorer m_MarkerAnchorer;
        IRenderedMarkerManager m_RenderedMarkerManager;
        bool m_LoadingComplete;
        string m_QueuedMarker;

        public bool CreateMarker(IMarker newMarker)
        {
            if (ReadOnly)
                return false;
            if (MarkerStorage == null)
            {
                Debug.LogError("No storage attached");
                return false;
            }

            newMarker.LastUpdatedTime = newMarker.CreatedTime = newMarker.LastUsedTime = DateTime.Now.ToUniversalTime();
            Marker result = (Marker)newMarker;
            return MarkerStorage.Create(result);
        }

        public bool EditMarker(IMarker editedMarker)
        {
            if (ReadOnly)
                return false;
            if (MarkerStorage == null)
            {
                Debug.LogError("No storage attached");
                return false;
            }

            // Create marker if it's not in the container.
            if (!MarkerStorage.Contains(editedMarker.Id))
                return CreateMarker(editedMarker);

            Visualize(editedMarker);

            Marker updatedMarker = (Marker)editedMarker;
            updatedMarker.LastUpdatedTime = updatedMarker.LastUsedTime = DateTime.Now.ToUniversalTime();
            ActiveMarker = updatedMarker;
            MarkerStorage.Update(updatedMarker);
            return true;
        }

        public void DeleteMarker(IMarker removedMarker)
        {
            if (ReadOnly)
                return;
            Marker marker = (Marker)removedMarker;
            if (ActiveMarker != null && ActiveMarker.Id == marker.Id)
                ActiveMarker = null;
            if (!MarkerStorage.Delete(marker))
                OnMarkerListUpdated?.Invoke();
        }

        public void Visualize(IMarker data)
        {
            m_RenderedMarkerManager?.Visualize(data);
            if (!FoundPose || AlignedObject == null)
                return;
            data.LastUsedTime = DateTime.Now.ToUniversalTime();
            var existingAlignment = AlignedObject.Get();
            var newAlignment = Marker.AlignObject(data, existingAlignment, CurrentMarkerPose);
            AlignedObject.Move(newAlignment);
        }

        public IMarker FirstOrDefaultMarker()
        {
            IMarker response = null;
            if (MarkerStorage == null)
            {
                Debug.LogError("[MarkerController] Missing a MarkerStorage");
            }
            else if (MarkerStorage.Markers.Count == 0)
            {
                var newMarker = new Marker();
                newMarker.Id = new SyncId(Guid.NewGuid().ToString());
                newMarker.Name = "Reflect Marker";
                CreateMarker(newMarker);
                return newMarker;
            }
            else
            {
                response = MarkerStorage.Markers[0];
            }

            return response;
        }

        void GetUpdatesFromMarkerStore()
        {
            if (MarkerStorage == null)
                return;

            MarkerStorage.OnUpdated += HandleMarkerStoreUpdate;
        }

        void DetachFromMarkerStore()
        {
            if (MarkerStorage == null)
                return;

            MarkerStorage.OnUpdated -= HandleMarkerStoreUpdate;
        }

        void HandleMarkerStoreUpdate(Delta<Marker> delta)
        {
            if (!m_ActiveMarker.Equals(default))
            {
                foreach (var items in delta.Changed)
                {
                    if (m_ActiveMarker.Id != items.Next.Id)
                        continue;
                    m_ActiveMarker = items.Next;
                    Visualize(m_ActiveMarker);
                    OnMarkerUpdated?.Invoke(m_ActiveMarker);
                }
            }

            OnMarkerListUpdated?.Invoke();
            if (!LoadingComplete)
                LoadingComplete = true;
        }

        public void ScanBarcode()
        {
            OnBarcodeScanOpen?.Invoke();
        }

        public void CancelBarcode()
        {
            OnBarcodeScanCanceled?.Invoke();
        }

        public void BarcodeScannerExited()
        {
            OnBarcodeScanExit?.Invoke();
        }
    }
}


