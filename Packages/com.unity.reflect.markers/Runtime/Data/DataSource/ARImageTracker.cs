using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.Reflect.Markers.ImageTracker
{
    /// <summary>
    /// Manages obtaining poses for markers from the ARFoundation Image Tracker.
    /// </summary>
    public class ARImageTracker : ImageTrackerBase
    {
        [SerializeField]
        ARTrackedImageManager m_TrackedImageManager;
        [SerializeField]
        ARSessionOrigin m_SessionOrigin;
        [SerializeField]
        XRReferenceImageLibrary m_ReferenceImageLibrary;
        [SerializeField]
        DebugImageTracker m_DebugImageTracker;

        List<ARTrackedImage> m_Tracked = new List<ARTrackedImage>();
        ARTrackedImage m_FoundMarker;
        IEnumerator m_Routine;
        //MarkerGraphicManager m_GraphicManager;

        public override bool IsAvailable
        {
            get
            {
                if (!m_TrackedImageManager)
                    return false;
                return m_TrackedImageManager.isActiveAndEnabled;
            }
        }

        void Awake()
        {
            AttachReferences();
        }

        void OnDestroy()
        {
            Stop();
        }

        public override void Run()
        {
            AttachReferences();
            if (m_Routine != null)
                StopCoroutine(m_Routine);

            if (m_TrackedImageManager.referenceLibrary is XRReferenceImageLibrary xrReferenceImageLibrary && xrReferenceImageLibrary == m_ReferenceImageLibrary)
            {
                Debug.Log("XRReferenceImageLibrary already set with same library.");
            }
            else if (m_ReferenceImageLibrary != null)
            {
                m_TrackedImageManager.referenceLibrary = m_ReferenceImageLibrary;
            }
            else if(m_TrackedImageManager.referenceLibrary != null)
            {
                Debug.Log("XRReferenceImageLibrary already set");
            }
            else
            {
                m_TrackedImageManager.referenceLibrary = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
            }

            m_Routine = Scan();
            StartCoroutine(m_Routine);
            ShowGizmo(true);
        }

        void AttachReferences()
        {
            if (m_SessionOrigin == null)
            {
                m_SessionOrigin = FindObjectOfType<ARSessionOrigin>(true);
            }

            if (m_TrackedImageManager == null)
            {
                m_TrackedImageManager = FindObjectOfType<ARTrackedImageManager>(true);
                if (!m_TrackedImageManager && m_SessionOrigin != null)
                {
                    m_TrackedImageManager = m_SessionOrigin.gameObject.AddComponent<ARTrackedImageManager>();

                }
            }

            if (!markerController)
                markerController = FindObjectOfType<MarkerController>();

            if (m_TrackedImageManager != null)
            {
                if ((ARImageTracker)markerController.ImageTracker != this)
                    markerController.ImageTracker = this;
            }
            else
            {
                Debug.LogWarning("Assigning debug as ImageTracker");
                // Fallback to the debug image tracker when AR isn't available
                markerController.ImageTracker = m_DebugImageTracker;
            }
        }

        public override void Stop()
        {
            ShowGizmo(false);
            StopScan();
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            if (trackedImage == null)
                return;
            if (trackedImage.trackingState != TrackingState.None)
            {

                Pose pose = new Pose()
                {
                    position = trackedImage.transform.position,
                    rotation = trackedImage.transform.rotation
                };
                UpdateValue(pose, trackedImage.referenceImage.name);
            }
            else
            {
                try
                {
                    OnTrackingLost?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            if (!IsTracking)
                return;
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                AddedTrackedImage(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                AddedTrackedImage(trackedImage);
                UpdateInfo(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in eventArgs.removed)
            {
                RemoveTrackedImage(trackedImage);
            }
        }

        void AddedTrackedImage(ARTrackedImage trackedImage)
        {
            if (m_Tracked.Contains(trackedImage))
                return;

            m_FoundMarker = trackedImage;

            m_Tracked.Add(trackedImage);
            var transform1 = trackedImage.transform;
            var pose = new Pose()
            {
                position = transform1.position,
                rotation = transform1.rotation
            };

            UpdateValue(pose, trackedImage.referenceImage.name);

            try
            {
                OnTrackedFound?.Invoke(Value, trackedImage.referenceImage.name);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        void RemoveTrackedImage(ARTrackedImage trackedImage)
        {
            if (m_Tracked.Contains(trackedImage))
                m_Tracked.Remove(trackedImage);
        }

        void StopScan()
        {
            if (m_TrackedImageManager != null)
                m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            m_Tracked.Clear();

            IsTracking = false;
            try
            {
                if (m_Routine != null)
                    StopCoroutine(m_Routine);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        IEnumerator Scan()
        {
            m_TrackedImageManager.enabled = true;
            IsTracking = true;
            m_FoundMarker = null;

            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            //Wait until Image is found for marker
            while (!m_FoundMarker)
            {
                yield return new WaitForSeconds(1f);
            }

            UpdateInfo(m_FoundMarker);
        }
    }
}
