using System;
using System.Collections.Generic;
using Unity.Reflect.Collections;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    public abstract class VRPointer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        protected Transform m_ControllerTransform;
        [SerializeField]
        protected XRBaseInteractable m_SelectionTarget;
#pragma warning restore 0649
        protected bool m_ShowPointer;
        protected List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();
        XRRayInteractor m_XRRayInteractor;

        Ray m_Ray;
        ISpatialPickerAsync<Tuple<GameObject, RaycastHit>> m_ObjectPicker;
        bool m_IsPicking;
        float m_PickingFrequency = 0.03f;
        float m_Time;
        bool m_IsInit;
        UIProjectStateData m_UIProjectStateDataCache;

        void Update()
        {
            // Cannot do this in Awake function, because inherited class will override its own one.
            if (!m_IsInit)
            {
                m_IsInit = true;
                m_XRRayInteractor = GetComponent<XRRayInteractor>();
            }

            if (!m_ShowPointer || m_Time < m_PickingFrequency)
            {
                m_Time += Time.deltaTime;
                return;
            }

            UpdateTarget();
        }

        protected void StateChange()
        {
            if (m_SelectionTarget == null || m_SelectionTarget.gameObject == null)
                return;

            if (!m_ShowPointer)
                CleanCache();

            m_SelectionTarget.gameObject.SetActive(m_ShowPointer);
        }

        void UpdateTarget()
        {
            // disable the target first so it doesn't interfere with the raycasts
            m_SelectionTarget.gameObject.SetActive(false);

            // check if we hit an UI element.
            if (m_XRRayInteractor != null && m_XRRayInteractor.TryGetCurrentRaycast(out var raycastHit, out var raycastHitIndex, out var uiRaycastHit, out var uiRaycastHitIndex, out var isUIHitClosest))
            {
                if (isUIHitClosest &&
                    !uiRaycastHit.Value.gameObject.CompareTag(OrphanUIController.k_TagName))
                {
                    return;
                }
            }

            // pick
            if (!m_IsPicking)
            {
                m_Ray.origin = m_ControllerTransform.position;
                m_Ray.direction = m_ControllerTransform.forward;

                m_IsPicking = true;
                m_ObjectPicker.Pick(m_Ray, results =>
                {
                    m_Results = results;
                    m_IsPicking = false;
                    m_Time = 0;
                });
            }

            // enable the target if there is a valid hit
            if (m_Results.Count == 0)
                return;

            m_SelectionTarget.transform.position = m_Results[0].Item2.point;
            m_SelectionTarget.gameObject.SetActive(true);
        }

        protected void CleanCache()
        {
            if (m_ObjectPicker != null)
            {
                ((SpatialSelector)m_ObjectPicker).CleanCache();
            }
            m_Results.Clear();
        }

        protected void OnObjectSelectorChanged(IPicker newData)
        {
            m_ObjectPicker = (ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>) newData;
        }

        protected virtual void OnDestroy()
        {
            CleanCache();
        }
    }
}
