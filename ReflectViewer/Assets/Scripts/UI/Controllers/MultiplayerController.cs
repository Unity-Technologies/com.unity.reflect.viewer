using MLAPI;
using System;
using System.Collections.Generic;
using Unity.Reflect.Multiplayer;
using Unity.SpatialFramework.Avatar;
using UnityEngine;
using UnityEngine.Reflect.Utils;

namespace Unity.Reflect.Viewer.UI
{
    public class MultiplayerController : MonoBehaviour
    {
        public bool connectToLocalServer = false;
        public AvatarControls networkedPlayerPrefab;
        [SerializeField]
        private NetworkingManager m_NetworkingManager;
        NetworkUser m_User;
        Transform rootNode;

        public void Awake()
        {
            UIStateManager.roomConnectionStateChanged += OnConnectionStateChanged;
        }

        private void OnConnectionStateChanged(RoomConnectionStateData obj)
        {
            if(obj.localUser.networkUser != m_User)
            {
                m_User = obj.localUser.networkUser;
                rootNode = UIStateManager.current.m_RootNode.transform;
            }
        }

        private void Update()
        {
            if(m_User != null)
            {
                var pos = rootNode.InverseTransformPoint(Camera.main.transform.localPosition);
                var rot = Quaternion.Inverse(rootNode.transform.rotation) * Camera.main.transform.localRotation;
                m_User.SetValue(NetworkUser.k_PositionDataKey, pos, true);
                m_User.SetValue(NetworkUser.k_RotationDataKey, rot, true);
            }
        }

        public AvatarControls CreateVisualRepresentation(Transform parent) => Instantiate(networkedPlayerPrefab, parent);

        private void OnApplicationQuit()
        {
            PlayerClientBridge.MatchmakerManager.Disconnect();
        }
    }
}
