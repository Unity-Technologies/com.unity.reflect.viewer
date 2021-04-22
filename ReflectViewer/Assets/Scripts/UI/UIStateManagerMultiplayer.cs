using MLAPI;
using SharpFlux.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Unity.MARS.Providers;
using Unity.Reflect.Data;
using Unity.Reflect.Model;
using Unity.Reflect.Multiplayer;
using Unity.SpatialFramework.Avatar;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Utils;
using VivoxUnity;

namespace Unity.Reflect.Viewer.UI
{
    public partial class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<UIARStateData>, IStore<UIDebugStateData>, IStore<ApplicationStateData>,
        IStore<RoomConnectionStateData>, IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding
    {
        MultiplayerController m_MultiplayerController;
        VivoxManager m_VivoxManager = new VivoxManager();
        MicInput m_MicInput;

        void AwakeMultiplayer()
        {
            m_MultiplayerController = GetComponent<MultiplayerController>();
            m_MicInput = GetComponent<MicInput>();
            if (m_MicInput != null)
            {
                var descriptor = new NetworkedTypeDescriptor(
                    typeof(float),
                    (value) => BitConverter.GetBytes((float)value),
                    (bytes) => BitConverter.ToSingle(bytes, 0));

                NetworkUser.RegisterDescriptorKey("micInput", descriptor);
                m_MicInput.OnMicLevelChanged += SendMicInput;
            }
        }

        void SendMicInput(float micLevel)
        {
            m_RoomConnectionStateData.localUser.voiceStateData.micVolume = micLevel;
            m_RoomConnectionStateData.localUser.networkUser?.SetValue("micInput", micLevel);
            roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
        }

        public UserIdentity GetUserIdentityFromSession(string id)
        {
            if (id == m_UISessionStateData.sessionState.userIdentity.matchmakerId)
                return m_UISessionStateData.sessionState.userIdentity;
            else
            {
                for (int i = 0; i < m_UISessionStateData.sessionState.rooms.Length; i++)
                {
                    for (int j = 0; j < m_UISessionStateData.sessionState.rooms[i].users.Count; j++)
                    {
                        if (m_UISessionStateData.sessionState.rooms[i].users[j].matchmakerId == id)
                            return m_UISessionStateData.sessionState.rooms[i].users[j];
                    }
                }
                for (int j = 0; j < m_UISessionStateData.sessionState.linkSharedProjectRoom.users.Count; j++)
                {
                    if (m_UISessionStateData.sessionState.linkSharedProjectRoom.users[j].matchmakerId == id)
                        return m_UISessionStateData.sessionState.linkSharedProjectRoom.users[j];
                }

            }
            return default;
        }

        void ConnectMultiplayerEvents()
        {
            PlayerClientBridge.MatchmakerManager.OnPlayerDisconnected += RemovePlayerFromSession;
            PlayerClientBridge.MatchmakerManager.OnPlayerDisconnected += RemovePlayerFromRoom;

            PlayerClientBridge.MatchmakerManager.OnPlayerConnected += AddPlayerToSession;
            PlayerClientBridge.MatchmakerManager.OnPlayerConnected += AddPlayerToRoom;

            PlayerClientBridge.MatchmakerManager.OnLobbyConnectionStatusChanged += OnLobbyStatusChanged;
            PlayerClientBridge.MatchmakerManager.OnRoomConnectionStatusChanged += OnRoomStatusChanged;

            PlayerClientBridge.NetcodeManager.OnStatusChanged += OnNetcodeStatusChanged;

            NetworkUser.OnUserPrefabCreated += OnNetcodeUserEntered;
            NetworkUser.OnUserPrefabDestroyed += OnNetcodeUserLeft;

            SyncObjectBinding.OnCreated += SetUserSelectedObject;

            m_VivoxManager.onVivoxParticipantJoined += AssignParticipant;
        }

        private void SetUserSelectedObject(GameObject obj)
        {
            var syncObj = obj.GetComponent<SyncObjectBinding>();

            for (int i = 0; i < m_RoomConnectionStateData.users.Count; i++)
            {
                var original = m_RoomConnectionStateData.users[i];
                if (original.selectedStreamKey.key == syncObj.streamKey.key && original.selectedStreamKey.source == syncObj.streamKey.source && original.selectedObject != obj)
                {
                    original.selectedObject = obj;
                    m_RoomConnectionStateData.users[i] = original; // I dont like this because its changing the iterated elements while continuing the iteration
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                }
            }
        }

        void AssignParticipant(IParticipant obj)
        {
            if(obj.ParticipantId == m_UISessionStateData.sessionState.userIdentity.vivoxParticipantId)
            {
                m_RoomConnectionStateData.localUser.vivoxParticipant = obj;
                roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                OnParticipantPropertyChange(obj, new PropertyChangedEventArgs("IsMutedForAll"));
                obj.PropertyChanged += OnLocalParticipantPropertyChanged;
            }
            else
            {
                var existingUserIndex = m_RoomConnectionStateData.users.FindIndex(u => GetUserIdentityFromSession(u.matchmakerId).vivoxParticipantId == obj.ParticipantId);
                if (existingUserIndex != -1)
                {
                    var original = m_RoomConnectionStateData.users[existingUserIndex];
                    original.vivoxParticipant = obj;
                    m_RoomConnectionStateData.users[existingUserIndex] = original;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    OnParticipantPropertyChange(obj, new PropertyChangedEventArgs("IsMutedForAll"));
                    obj.PropertyChanged += OnParticipantPropertyChange;
                }
            }
        }

        void OnLocalParticipantPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var participant = (IParticipant)sender;
            switch (e.PropertyName)
            {
                case "LocalMute":
                    m_RoomConnectionStateData.localUser.voiceStateData.isLocallyMuted = participant.LocalMute;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    break;
                case "AudioEnergy":
                    m_RoomConnectionStateData.localUser.voiceStateData.micVolume = (float)participant.AudioEnergy;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    break;
                case "IsMutedForAll":
                    m_RoomConnectionStateData.localUser.voiceStateData.isServerMuted = participant.IsMutedForAll;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    break;
            }
        }

        void OnParticipantPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            var participant = (IParticipant)sender;
            var existingUserIndex = m_RoomConnectionStateData.users.FindIndex(u => GetUserIdentityFromSession(u.matchmakerId).vivoxParticipantId == participant.ParticipantId);
            if (existingUserIndex != -1)
            {
                var original = m_RoomConnectionStateData.users[existingUserIndex];
                switch (e.PropertyName)
                {
                    case "LocalMute":
                        original.voiceStateData.isLocallyMuted = participant.LocalMute;
                        original.visualRepresentation.muted = original.voiceStateData.isLocallyMuted || original.voiceStateData.isServerMuted;
                        m_RoomConnectionStateData.users[existingUserIndex] = original;
                        roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                        break;
                    case "IsMutedForAll":
                        original.voiceStateData.isServerMuted = participant.IsMutedForAll;
                        original.visualRepresentation.muted = original.voiceStateData.isLocallyMuted || original.voiceStateData.isServerMuted;
                        m_RoomConnectionStateData.users[existingUserIndex] = original;
                        roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                        break;
                }
            }
        }

        protected void ToggleUserMicrophone(string matchmakerId)
        {
            if (matchmakerId == m_RoomConnectionStateData.localUser.matchmakerId)
            {
                PlayerClientBridge.ChatClientManager.RequestMuteUnmuteSelfCredentialsAsync(muteTask =>
                {
                    if (muteTask.IsCanceled)
                    {
                        return;
                    }
                    if (muteTask.IsFaulted)
                    {
                        Debug.LogError(muteTask.Exception);
                        return;
                    }

                    var localParticipant = m_RoomConnectionStateData.localUser.vivoxParticipant;
                    bool isMuted = m_RoomConnectionStateData.localUser.voiceStateData.isServerMuted;
                    m_MicInput.enabled = isMuted;
                    localParticipant.SetIsMuteForAll(localParticipant.ParticipantId, !isMuted, muteTask.Result.AccessToken,
                        callback => roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData));
                });
            }
            else
            {
                var user = m_RoomConnectionStateData.users.Find(data => data.matchmakerId == matchmakerId);
                if (user.vivoxParticipant != null)
                {
                    user.vivoxParticipant.LocalMute = !user.vivoxParticipant.LocalMute;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                }
            }
        }

        void OnLobbyStatusChanged(ConnectionStatusDetails obj)
        {
            switch (obj.Status)
            {
                case Multiplayer.ConnectionStatus.Connected:
                    m_UISessionStateData.sessionState.collaborationState |= CollaborationState.ConnectedMatchmaker;
                    sessionStateChanged?.Invoke(m_UISessionStateData);
                    break;
                case Multiplayer.ConnectionStatus.Disconnected:
                    m_UISessionStateData.sessionState.collaborationState &= ~CollaborationState.ConnectedMatchmaker;
                    if (obj.Error == null)
                    {
                        // Voluntary Disconnect, we clear the UI instantle
                        ClearSessionUIState();
                    }
                    else
                    {
                        // TODO: Involuntary Disconnect, we can do something different
                        ClearSessionUIState();
                    }
                    break;
            }
        }

        void OnRoomStatusChanged(ConnectionStatusDetails obj)
        {
            switch (obj.Status)
            {
                case Multiplayer.ConnectionStatus.Connected:
                    m_UISessionStateData.sessionState.collaborationState |= CollaborationState.ConnectedRoom;
                    sessionStateChanged?.Invoke(m_UISessionStateData);
                    break;
                case Multiplayer.ConnectionStatus.Disconnected:
                    m_UISessionStateData.sessionState.collaborationState &= ~CollaborationState.ConnectedRoom;
                    m_VivoxManager.LeaveChannel();
                    m_VivoxManager.Logout();
                    m_RoomConnectionStateData.localUser = NetworkUserData.defaultData;
                    if (obj.Error == null)
                    {
                        // Voluntary Disconnect, we clear the UI instantle
                        ClearRoomUIState();
                    }
                    else
                    {
                        // TODO: Involuntary Disconnect, we can do something different
                        ClearRoomUIState();
                    }
                    break;
            }
        }

        void OnNetcodeStatusChanged(ConnectionStatusDetails obj)
        {
            switch (obj.Status)
            {
                case Multiplayer.ConnectionStatus.Connected:
                    m_UISessionStateData.sessionState.collaborationState |= CollaborationState.ConnectedNetcode;
                    sessionStateChanged?.Invoke(m_UISessionStateData);
                    break;
                case Multiplayer.ConnectionStatus.Disconnected:
                    m_UISessionStateData.sessionState.collaborationState &= ~CollaborationState.ConnectedNetcode;
                    if (obj.Error == null)
                    {
                        // Voluntary Disconnect, we clear the UI instantle
                        ClearRoomUIState();
                    }
                    else
                    {
                        // TODO: Involuntary Disconnect, we can do something different
                        ClearRoomUIState();
                    }
                    break;
            }
        }

        void OnVivoxLoginResult(Task<VoiceLoginCredentials> loginTask)
        {
            m_VivoxManager.LoginAsync(loginTask.Result).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                if (t.IsFaulted)
                {
                    Debug.LogError(t.Exception);
                    return;
                }


                PlayerClientBridge.ChatClientManager.RequestChannelCredentialsAsync(m_RoomConnectionStateData.localUser.voiceStateData.isServerMuted, OnVivoxChannelResult);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void OnVivoxChannelResult(Task<VoiceChannelCredentials> channelTask)
        {
            if (channelTask.IsCanceled)
                return;

            if (channelTask.IsFaulted)
            {
                Debug.LogError(channelTask.Exception);
                return;
            }
            m_VivoxManager.JoinChannelAsync(channelTask.Result);
        }

        void AddPlayerToSession(PlayerInfo playerInfo)
        {
            var identity = new UserIdentity(playerInfo.Id, playerInfo.ColorIndex, playerInfo.Name, playerInfo.Connected, playerInfo.VoiceChatId);

            if(playerInfo.IsSelf)
            {
                m_UISessionStateData.sessionState.userIdentity = identity;
            }
            else
            {
                // Add or update user in the session
                var roomIndex = Array.FindIndex(m_UISessionStateData.sessionState.rooms, (r) => r.project.serverProjectId == playerInfo.RoomId);
                if(roomIndex != -1)
                {
                    var userIndex = m_UISessionStateData.sessionState.rooms[roomIndex].users.FindIndex(u => u.matchmakerId == playerInfo.Id);

                    if (userIndex == -1)
                        m_UISessionStateData.sessionState.rooms[roomIndex].users.Add(identity);
                    else
                        m_UISessionStateData.sessionState.rooms[roomIndex].users[userIndex] = identity;
                }
                else
                {
                    var userIndex = m_UISessionStateData.sessionState.linkSharedProjectRoom.users.FindIndex(u => u.matchmakerId == playerInfo.Id);

                    if (userIndex == -1)
                        m_UISessionStateData.sessionState.linkSharedProjectRoom.users.Add(identity);
                    else
                        m_UISessionStateData.sessionState.linkSharedProjectRoom.users[userIndex] = identity;
                }
            }
            sessionStateChanged?.Invoke(m_UISessionStateData);
        }

        void RemovePlayerFromSession(PlayerInfo playerInfo)
        {
            if (playerInfo.IsSelf)
            {
                m_UISessionStateData.sessionState.userIdentity = new UserIdentity(null, -1, sessionStateData.sessionState.user?.DisplayName, DateTime.MinValue, null);
            }
            else
            {
                //Remove user from session rooms
                var roomIndex = Array.FindIndex(m_UISessionStateData.sessionState.rooms, (r) => r.project.serverProjectId == playerInfo.RoomId);
                if (roomIndex != -1)
                {
                    var userIndex = m_UISessionStateData.sessionState.rooms[roomIndex].users.FindIndex(u => u.matchmakerId == playerInfo.Id);
                    m_UISessionStateData.sessionState.rooms[roomIndex].users.RemoveAt(userIndex);
                }
                else
                {
                    var userIndex = m_UISessionStateData.sessionState.linkSharedProjectRoom.users.FindIndex(u => u.matchmakerId == playerInfo.Id);
                    m_UISessionStateData.sessionState.linkSharedProjectRoom.users.RemoveAt(userIndex);
                }
            }
            sessionStateChanged?.Invoke(m_UISessionStateData);
        }

        void AddPlayerToRoom(PlayerInfo playerInfo)
        {
            if (m_UIProjectStateData.activeProject.serverProjectId != playerInfo.RoomId)
                return;

            if (playerInfo.IsSelf)
            {
                m_RoomConnectionStateData.localUser.matchmakerId = playerInfo.Id;
                PlayerClientBridge.ChatClientManager.RequestLoginCredentialsAsync(OnVivoxLoginResult);
            }
            else
            {
                if (m_RoomConnectionStateData.users == null)
                    m_RoomConnectionStateData.users = new List<NetworkUserData>();

                var existingUserIndex = m_RoomConnectionStateData.users.FindIndex(u => u.matchmakerId == playerInfo.Id);

                //Get or create new userdata
                NetworkUserData userData = existingUserIndex == -1 ? new NetworkUserData(playerInfo.Id) : m_RoomConnectionStateData.users[existingUserIndex];

                if (userData.visualRepresentation != null)
                {
                    var identity = GetUserIdentityFromSession(playerInfo.Id);
                    var color = m_UIStateData.colorPalette[identity.colorIndex];
                    userData.visualRepresentation.avatarName = identity.fullName;
                    userData.visualRepresentation.color = color;
                    userData.visualRepresentation.avatarInitials = UIUtils.CreateInitialsFor(identity.fullName);

                    if (!userData.visualRepresentation.gameObject.activeSelf)
                        userData.visualRepresentation.gameObject.SetActive(true);
                }

                // Update or add created userdata
                if (existingUserIndex == -1)
                    m_RoomConnectionStateData.users.Add(userData);
                else
                    m_RoomConnectionStateData.users[existingUserIndex] = userData;


                if (userData.vivoxParticipant == null)
                {
                    var identity = GetUserIdentityFromSession(playerInfo.Id);
                    var participant = m_VivoxManager.GetParticipant(identity.vivoxParticipantId);
                    if (participant != null)
                        AssignParticipant(participant);
                }
            }
            roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
        }

        void RemovePlayerFromRoom(PlayerInfo playerInfo)
        {
            //Remove user from room connection
            if (m_UIProjectStateData.activeProject.serverProjectId == playerInfo.RoomId)
            {
                if (playerInfo.IsSelf)
                {
                    m_RoomConnectionStateData.localUser = NetworkUserData.defaultData;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                }
                else
                {
                    var userRoomIndex = m_RoomConnectionStateData.users.FindIndex((data) => data.matchmakerId == playerInfo.Id);
                    if (userRoomIndex == -1)
                    {
                        Debug.LogError("Player left matchaker room but was already removed from UI room");
                        return;
                    }
                    OnNetcodeUserLeft(m_RoomConnectionStateData.users[userRoomIndex].matchmakerId, m_RoomConnectionStateData.users[userRoomIndex].networkUser);
                    m_RoomConnectionStateData.users.RemoveAt(userRoomIndex);
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                }
            }
        }
        void SetNetworkSelected(ObjectSelectionInfo info)
        {
            if (m_RoomConnectionStateData.localUser.networkUser != null)
            {
                Multiplayer.Netcode.StreamKey key;
                if (info.CurrentSelectedObject() != null)
                {
                    var currentObj = info.CurrentSelectedObject().GetComponentInParent<SyncObjectBinding>();
                    key = new Multiplayer.Netcode.StreamKey
                    {
                        Source = currentObj.streamKey.source,
                        PersistentKeyName = currentObj.streamKey.key.Name
                    };
                }
                else
                {
                    key = new Multiplayer.Netcode.StreamKey
                    {
                        Source = "",
                        PersistentKeyName = ""
                    };
                }
                m_RoomConnectionStateData.localUser.networkUser.SetValue(NetworkUser.k_SelectionDataKey, key);
            }
        }

        private void OnNetcodeUserEntered(string userId, NetworkUser user)
        {
            if (user.IsOwner)
            {
                m_RoomConnectionStateData.localUser.networkUser = user;
                m_RoomConnectionStateData.localUser.matchmakerId = userId;
                user.OnValueChange += DispatchLocalUserDataChanged;
                SetNetworkSelected(m_UIProjectStateData.objectSelectionInfo);
            }
            else
            {
                //Get or create new userdata
                var index = m_RoomConnectionStateData.users.FindIndex((userData) => userData.matchmakerId == userId);

                NetworkUserData userData = index == -1 ? new NetworkUserData(userId) : m_RoomConnectionStateData.users[index];
                userData.networkUser = user;

                if (userData.visualRepresentation == null)
                {
                    userData.visualRepresentation = m_MultiplayerController.CreateVisualRepresentation(m_RootNode.transform);
                }

                var identity = GetUserIdentityFromSession(userId);
                if (identity != default)
                {
                    userData.visualRepresentation.name = "Avatar - " + identity.fullName;
                    userData.visualRepresentation.avatarName = identity.fullName;
                    userData.visualRepresentation.color = m_UIStateData.colorPalette[identity.colorIndex];
                    userData.visualRepresentation.avatarInitials = UIUtils.CreateInitialsFor(identity.fullName);
                }
                else
                {
                    userData.visualRepresentation.gameObject.SetActive(false);
                }

                // Update or add created userdata
                if (index != -1) // User was in a room
                    m_RoomConnectionStateData.users[index] = userData;
                else
                    m_RoomConnectionStateData.users.Add(userData);

                user.OnValueChange += DispatchRemoteUserDataChanged;

                if (userData.vivoxParticipant == null && identity != default)
                {
                    var participant = m_VivoxManager.GetParticipant(identity.vivoxParticipantId);
                    if (participant != null)
                        AssignParticipant(participant);
                }
            }
            Debug.LogFormat("{0} MLAPI user [{1}:{2}] joined room", user.IsOwner ? "Local" : "Remote", userId, user.OwnerClientId);
            roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
        }

        private void OnNetcodeUserLeft(string userId, NetworkUser user)
        {
            if(userId == m_RoomConnectionStateData.localUser.matchmakerId)
            {
                m_RoomConnectionStateData.localUser.networkUser = null;
                roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
            }
            else
            {
                var indexToRemove = m_RoomConnectionStateData.users.FindIndex((data) => data.matchmakerId == userId);
                if(indexToRemove == -1)
                    return;
                if (indexToRemove != -1)
                {
                    var originalData = m_RoomConnectionStateData.users[indexToRemove];

                    if (originalData.visualRepresentation != null)
                        Destroy(originalData.visualRepresentation.gameObject);
                    originalData.visualRepresentation = null;
                    originalData.networkUser = null;
                    m_RoomConnectionStateData.users[indexToRemove] = originalData;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                }
            }
            Debug.LogFormat("{0} MLAPI user [{1}:{2}] left room", user.IsOwner ? "Local" : "Remote", userId, user.OwnerClientId);
        }

        private void DispatchLocalUserDataChanged(NetworkUser user, string key, object value)
        {
            switch (key)
            {
                case NetworkUser.k_SelectionDataKey:
                    var casted = (Multiplayer.Netcode.StreamKey)value;
                    var streamKey = new StreamKey(casted.Source, PersistentKey.GetKey<SyncObjectInstance>(casted.PersistentKeyName));
                    m_RoomConnectionStateData.localUser.selectedStreamKey = streamKey;
                    if (m_ReflectPipeline.TryGetNode(out InstanceConverterNode converter) && converter.processor.TryGetInstance(streamKey, out var instance))
                    {
                        m_RoomConnectionStateData.localUser.selectedObject = instance.gameObject;
                    }
                    else
                    {
                        m_RoomConnectionStateData.localUser.selectedObject = null;
                    }
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    break;
            }
        }

        public void OnApplicationQuit()
        {
            m_VivoxManager.LeaveChannel();
            m_VivoxManager.Logout();
        }

        private void ClearRoomUIState()
        {
            for (int i = 0; i < m_RoomConnectionStateData.users.Count; i++)
            {
                var original = m_RoomConnectionStateData.users[i];
                if (original.visualRepresentation != null)
                    Destroy(original.visualRepresentation.gameObject);
                original.vivoxParticipant = null;
                original.voiceStateData = VoiceStateData.defaultData;
                m_RoomConnectionStateData.users[i] = original;
            }
            m_RoomConnectionStateData.users.Clear();
            roomConnectionStateChanged?.Invoke(roomConnectionStateData);
        }

        private void ClearSessionUIState()
        {
            m_UISessionStateData.sessionState.userIdentity = new UserIdentity(null, -1, sessionStateData.sessionState.user?.DisplayName, DateTime.UtcNow, null);
            foreach (var room in m_UISessionStateData.sessionState.rooms)
            {
                room.users.Clear();
            }
            sessionStateChanged?.Invoke(sessionStateData);
        }

        private void DispatchRemoteUserDataChanged(NetworkUser user, string key, object value)
        {
            var index = m_RoomConnectionStateData.users.FindIndex((userData) => userData.matchmakerId == user.MatchmakerUserId);
            if (index == -1)
            {
                Debug.LogWarning("MLAPI Changed event received before matchmaker event, discarding...");
                return;
            }

            var originalData = m_RoomConnectionStateData.users[index];
            switch (key)
            {
                case "micInput":
                    originalData.visualRepresentation.normalizedMicLevel = (float)value;
                    originalData.voiceStateData.micVolume = (float)value;
                    m_RoomConnectionStateData.users[index] = originalData;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    break;
                case NetworkUser.k_PositionDataKey:
                    if (originalData.visualRepresentation == null)
                        return;
                    originalData.visualRepresentation.transform.localPosition = (Vector3)value;
                    break;
                case NetworkUser.k_RotationDataKey:
                    if (originalData.visualRepresentation == null)
                        return;
                    originalData.visualRepresentation.transform.localRotation = (Quaternion)value;
                    break;
                case NetworkUser.k_SelectionDataKey:
                    var casted = (Multiplayer.Netcode.StreamKey)value;
                    var streamKey = new StreamKey(casted.Source, PersistentKey.GetKey<SyncObjectInstance>(casted.PersistentKeyName));
                    originalData.selectedStreamKey = streamKey;
                    if (m_ReflectPipeline.TryGetNode(out InstanceConverterNode converter) && converter.processor.TryGetInstance(streamKey, out var instance))
                    {
                        originalData.selectedObject = instance.gameObject;
                    }
                    else
                    {
                        originalData.selectedObject = null;
                    }
                    m_RoomConnectionStateData.users[index] = originalData;
                    roomConnectionStateChanged?.Invoke(m_RoomConnectionStateData);
                    break;
            }
        }

#if UNITY_ANDROID
        private void OnApplicationPause(bool pause)
        {
            m_VivoxManager.SetInputMuted(pause);
            m_VivoxManager.SetOutputMuted(pause);

            if (pause)
            {
                PlayerClientBridge.MatchmakerManager.Disconnect();
            }
            else
            {
                PlayerClientBridge.MatchmakerManager.Connect(m_UISessionStateData.sessionState.user.AccessToken, m_MultiplayerController.connectToLocalServer);
                PlayerClientBridge.MatchmakerManager.MonitorRooms(sessionStateData.sessionState.rooms.Select(r => r.project.serverProjectId));
                if (m_UIProjectStateData.activeProject != Project.Empty)
                {
                    PlayerClientBridge.MatchmakerManager.JoinRoom(m_UIProjectStateData.activeProject.serverProjectId);
                }
            }
        }
#endif
    }
}
