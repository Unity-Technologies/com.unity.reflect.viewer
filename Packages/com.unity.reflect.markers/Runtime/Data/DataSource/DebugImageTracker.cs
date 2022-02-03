using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.ImageTracker
{
    public class DebugImageTracker : ImageTrackerBase
    {
        [SerializeField]
        Transform m_DebugRealityMarkerLocation = null;
        [SerializeField]
        float m_DebugDetectionDelaySeconds = 3f;

        IEnumerator routine;

        public override bool IsAvailable => true;
        public override bool IsTracking => true;

        public override void Run()
        {
            routine = TriggerCountdown();
            StartCoroutine(routine);

            ShowGizmo(true);
        }

        public override void Stop()
        {
            if (routine != null)
                StopCoroutine(routine);

            ShowGizmo(false);
        }

        IEnumerator TriggerCountdown()
        {
            yield return new WaitForSeconds(m_DebugDetectionDelaySeconds);
            Pose newPose = new Pose()
            {
                position = m_DebugRealityMarkerLocation.position,
                rotation = m_DebugRealityMarkerLocation.rotation
            };
            OnTrackedFound?.Invoke(newPose, null);
            UpdateValue(newPose);
        }

        void Start()
        {
            #if UNITY_EDITOR
            markerController = FindObjectOfType<MarkerController>();
            markerController.ImageTracker = this;
            #endif
        }
    }
}
