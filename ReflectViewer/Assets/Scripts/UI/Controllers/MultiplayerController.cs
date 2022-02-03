using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Multiplayer;
using Unity.SpatialFramework.Avatar;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.Reflect.Viewer.UI
{
    public class MultiplayerController: MonoBehaviour
    {
        public bool connectToLocalServer = false;
        public AvatarControls networkedPlayerPrefab;
        [SerializeField]
        NetworkingManager m_NetworkingManager;
        XRRig m_XRRig;
        Vector3 m_VRCameraOffset;
        IUISelector<Transform> m_RootSelector;
        Transform m_MainCamera;
        Transform m_WalkCamera;
        IUISelector<bool> m_VREnableSelector;
        IUISelector<bool> m_WalkModeEnableSelector;
        IUISelector<NetworkUserData> m_LocalUserSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_LocalUserSelector = UISelectorFactory.createSelector<NetworkUserData>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser)));
            m_DisposeOnDestroy.Add(m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnableChanged));
            m_DisposeOnDestroy.Add(m_WalkModeEnableSelector = UISelectorFactory.createSelector<bool>(WalkModeContext.current, nameof(IWalkModeDataProvider.walkEnabled)));

            m_DisposeOnDestroy.Add(m_RootSelector = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode)));

            if (Camera.main != null)
                m_MainCamera = Camera.main.transform;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            IEnumerator WaitAFrame()
            {
                yield return null;
                m_XRRig = Camera.main.GetComponentInParent<XRRig>();
                if (m_XRRig != null)
                {
                    m_VRCameraOffset.y = m_XRRig.cameraYOffset;
                }
                else if (m_VREnableSelector.GetValue())
                {
                    Debug.LogError("XRRig not found.");
                }
            }
            StartCoroutine(WaitAFrame());
        }

        void OnVREnableChanged(bool newData)
        {
            if (!newData)
            {
                m_VRCameraOffset = Vector3.zero;
            }
        }

        void Update()
        {
            if (m_LocalUserSelector.GetValue() != null && m_LocalUserSelector.GetValue().networkUser != null)
            {
                if (m_MainCamera == null || !m_MainCamera.gameObject.activeInHierarchy)
                {
                    SetCamera();
                }
                var (pos, rot) = GetCameraPositionAndRotation();
                pos = m_RootSelector.GetValue().InverseTransformPoint(pos);
                m_LocalUserSelector.GetValue().networkUser.SetValue(NetworkUser.k_PositionDataKey, pos, true);
                m_LocalUserSelector.GetValue().networkUser.SetValue(NetworkUser.k_RotationDataKey, rot, true);
            }
        }

        void SetCamera()
        {
            if (m_WalkModeEnableSelector.GetValue())
            {
                m_WalkCamera = Camera.main.transform;
                m_MainCamera = m_WalkCamera.parent;
            }
            else
            {
                m_WalkCamera = null;
                m_MainCamera = Camera.main.transform;
            }
        }

        (Vector3, Quaternion) GetCameraPositionAndRotation()
        {
            var rotation = Quaternion.Inverse(m_RootSelector.GetValue().rotation) * m_MainCamera.localRotation;
            if (m_XRRig != null && m_VREnableSelector.GetValue())
            {
                rotation = Quaternion.Inverse(m_RootSelector.GetValue().rotation) * (m_XRRig.transform.localRotation * m_MainCamera.localRotation);
                return (m_XRRig.transform.localPosition + m_MainCamera.localPosition + m_VRCameraOffset, rotation);
            }
            if (m_WalkCamera != null && m_WalkModeEnableSelector.GetValue())
            {
                rotation *= m_WalkCamera.localRotation;
                return (m_MainCamera.localPosition + m_WalkCamera.localPosition, rotation);
            }
            return (m_MainCamera.localPosition, rotation);
        }

        public AvatarControls CreateVisualRepresentation(Transform parent) => Instantiate(networkedPlayerPrefab, parent);

        void OnApplicationQuit()
        {
            PlayerClientBridge.MatchmakerManager.Disconnect();
        }
    }
}
