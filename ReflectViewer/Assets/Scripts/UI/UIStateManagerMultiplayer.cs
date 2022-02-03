using SharpFlux.Stores;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if UNITY_ANDROID
using System.Linq;
#endif
using System.Threading.Tasks;
using Unity.MARS.Providers;
using Unity.Reflect.Actors;
using Unity.Reflect.Data;
using Unity.Reflect.Model;
using Unity.Reflect.Multiplayer;
using Unity.SpatialFramework.Avatar;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Reflect.Utils;
using UnityEngine.Reflect.Viewer.Core;
using VivoxUnity;

namespace Unity.Reflect.Viewer.UI
{
    public partial class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<UIARStateData>, IStore<UIDebugStateData>, IStore<ApplicationSettingsStateData>, IStore<PipelineStateData>,
        IStore<RoomConnectionStateData>, IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding, IStore<SceneOptionData>, IStore<VRStateData>, IStore<MessageManagerStateData>, IStore<ProjectSettingStateData>, IStore<LoginSettingStateData>, IStore<DeltaDNAStateData>
    {
        [SerializeField]
        Transform m_AvatarRoot;

        MultiplayerController m_MultiplayerController;
        IChannelSession m_ChannelSession;

        void AwakeMultiplayer()
        {
            m_RoomConnectionStateData.vivoxManager = new VivoxManager();
            m_MultiplayerController = GetComponent<MultiplayerController>();
            var descriptor = new NetworkedTypeDescriptor(
                typeof(float),
                (value) => BitConverter.GetBytes((float)value),
                (bytes) => BitConverter.ToSingle(bytes, 0));

            NetworkUser.RegisterDescriptorKey("micInput", descriptor);
        }

        void SetMicLevel(float micLevel)
        {
            var localUser = m_RoomConnectionStateData.localUser;
            localUser.voiceStateData.micVolume = micLevel;
            localUser.networkUser?.SetValue("micInput", micLevel);

            m_RoomConnectionStateData.localUser = localUser;
            m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
        }

        public UserIdentity GetUserIdentityFromSession(string id)
        {
            if (id == null || id == ((UserIdentity)m_UISessionStateData.userIdentity).matchmakerId)
                return (UserIdentity)m_UISessionStateData.userIdentity;
            else
            {
                for (int i = 0; i < m_UISessionStateData.rooms.Length; i++)
                {
                    ProjectRoom projectRoom = (ProjectRoom)m_UISessionStateData.rooms[i];
                    for (int j = 0; j < projectRoom.users.Count; j++)
                    {
                        if (projectRoom.users[j].matchmakerId == id)
                            return projectRoom.users[j];
                    }
                }

                ProjectRoom linkSharedProjectRoom = (ProjectRoom)m_UISessionStateData.linkSharedProjectRoom;
                for (int j = 0; j < linkSharedProjectRoom.users.Count; j++)
                {
                    if (linkSharedProjectRoom.users[j].matchmakerId == id)
                        return linkSharedProjectRoom.users[j];
                }
            }

            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData, UpdateNotification.ForceNotify);
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

            m_RoomConnectionStateData.vivoxManager.onVivoxParticipantJoined += AssignParticipant;
        }

        void ConnectMultiplayerToActorSystem()
        {
            m_ViewerBridge.GameObjectCreating += SetUserSelectedObject;
        }

        void SetUserSelectedObject(GameObjectCreating data)
        {
            foreach (var go in data.GameObjectIds)
            {
                var syncObj = go.GameObject.GetComponent<SyncObjectBinding>();

                for (int i = 0; i < m_RoomConnectionStateData.users.Count; i++)
                {
                    var original = m_RoomConnectionStateData.users[i];
                    if (original.selectedStreamKey.key == syncObj.streamKey.key && original.selectedStreamKey.source == syncObj.streamKey.source && original.selectedObject != go.GameObject)
                    {
                        original.selectedObject = go.GameObject;
                        m_RoomConnectionStateData.users[i] = original; // I dont like this because its changing the iterated elements while continuing the iteration
                        m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                    }
                }
            }
        }

        void AssignParticipant(IParticipant obj)
        {
            if (obj.ParticipantId == ((UserIdentity)m_UISessionStateData.userIdentity).vivoxParticipantId)
            {
                var localUser = m_RoomConnectionStateData.localUser;
                localUser.vivoxParticipant = obj;
                m_RoomConnectionStateData.localUser = localUser;
                m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
                OnParticipantPropertyChange(obj, new PropertyChangedEventArgs("IsMutedForAll"));
                m_ChannelSession = obj.ParentChannelSession;
                obj.PropertyChanged += OnLocalParticipantPropertyChanged;
            }
            else
            {
                var existingUserIndex = m_RoomConnectionStateData.users.FindIndex(u => GetUserIdentityFromSession(u.matchmakerId).vivoxParticipantId == obj.ParticipantId);
                if (existingUserIndex != -1)
                {
                    var original = m_RoomConnectionStateData.users[existingUserIndex];
                    original.vivoxParticipant = obj;
                    original.voiceStateData.isServerMuted = obj.IsMutedForAll;
                    m_RoomConnectionStateData.users[existingUserIndex] = original;
                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                    OnParticipantPropertyChange(obj, new PropertyChangedEventArgs("IsMutedForAll"));
                    obj.PropertyChanged += OnParticipantPropertyChange;
                }
            }
        }

        void OnLocalParticipantPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (m_ChannelSession?.ChannelState == ConnectionState.Connected)
            {
                var participant = (IParticipant)sender;
	            var localUser = m_RoomConnectionStateData.localUser;
                switch (e.PropertyName)
                {
                    case "LocalMute":
	                    localUser.voiceStateData.isLocallyMuted = participant.LocalMute;
	                    m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
                        break;
                    case "AudioEnergy":
                        SetMicLevel((float)participant.AudioEnergy);
                        break;
                    case "IsMutedForAll":
	                    localUser.voiceStateData.isServerMuted = participant.IsMutedForAll;
	                    m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
                        break;
                }
            }
        }

        void OnParticipantPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (m_ChannelSession?.ChannelState == ConnectionState.Connected)
            {
                var participant = (IParticipant)sender;
                var existingUserIndex = m_RoomConnectionStateData.users.FindIndex(u => GetUserIdentityFromSession(u.matchmakerId).vivoxParticipantId == participant.ParticipantId);
                if (existingUserIndex != -1)
                {
	                var users = m_RoomConnectionStateData.users;
	                var original = users[existingUserIndex];
                    switch (e.PropertyName)
                    {
                        case "LocalMute":
                            original.voiceStateData.isLocallyMuted = participant.LocalMute;
                            users[existingUserIndex] = original;
                            break;
                        case "IsMutedForAll":
                            original.voiceStateData.isServerMuted = participant.IsMutedForAll;
                            original.visualRepresentation.muted = original.voiceStateData.isServerMuted;
	                        users[existingUserIndex] = original;
                            break;
						default:
							return;
                    }
	                m_RoomConnectionStateData.users = users;
	                m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
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
                    localParticipant.SetIsMuteForAll(localParticipant.ParticipantId, !isMuted, muteTask.Result.AccessToken,
                        callback => m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify));
                });
            }
            else
            {
                var user = m_RoomConnectionStateData.users.Find(data => data.matchmakerId == matchmakerId);
                if (user.vivoxParticipant != null)
                {
                    user.vivoxParticipant.LocalMute = !user.vivoxParticipant.LocalMute;
                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                }
            }
        }

        void MuteUser(string matchmakerId)
        {
            var user = m_RoomConnectionStateData.users.Find(data => data.matchmakerId == matchmakerId);

            if (user.voiceStateData.isServerMuted)
                return;

            if (m_RoomConnectionStateData.vivoxManager.IsConnected)
            {
                if (user.vivoxParticipant != null)
                {
                    PlayerClientBridge.ChatClientManager.RequestMuteUnmuteCredentialsAsync(user.vivoxParticipant.ParticipantId, muteTask =>
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
                        user.vivoxParticipant.SetIsMuteForAll(localParticipant.ParticipantId, true, muteTask.Result.AccessToken,
                            callback => m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify));
                    });
                }
            }
            else
            {
                user.voiceStateData.isServerMuted = true;
                m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
            }
        }

        void ShowAvatars(bool value)
        {
            m_AvatarRoot.gameObject.SetActive(value);
        }

        void OnLobbyStatusChanged(ConnectionStatusDetails obj)
        {
            switch (obj.Status)
            {
                case Multiplayer.ConnectionStatus.Connected:
                    m_UISessionStateData.collaborationState |= CollaborationState.ConnectedMatchmaker;
                    m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);

                    // Look for async ready condition
                    Debug.Log($"OnLobbyStatusChanged TryConsumeDeepLink");
                    TryConsumeInteropRequest();
                    break;
                case Multiplayer.ConnectionStatus.Disconnected:
                    m_UISessionStateData.collaborationState &= ~CollaborationState.ConnectedMatchmaker;
                    m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
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
                    m_UISessionStateData.collaborationState |= CollaborationState.ConnectedRoom;
                    m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
                    break;
                case Multiplayer.ConnectionStatus.Disconnected:
                    m_UISessionStateData.collaborationState &= ~CollaborationState.ConnectedRoom;
                    m_RoomConnectionStateData.vivoxManager.LeaveChannel();
                    m_RoomConnectionStateData.vivoxManager.Logout();
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
                    m_UISessionStateData.collaborationState |= CollaborationState.ConnectedNetcode;
                    m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
                    break;
                case Multiplayer.ConnectionStatus.Disconnected:
                    m_UISessionStateData.collaborationState &= ~CollaborationState.ConnectedNetcode;
                    m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData);
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
            m_RoomConnectionStateData.vivoxManager.LoginAsync(loginTask.Result).ContinueWith(t =>
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

            m_RoomConnectionStateData.vivoxManager.JoinChannelAsync(channelTask.Result);
        }

        void AddPlayerToSession(PlayerInfo playerInfo)
        {
            var identity = new UserIdentity(playerInfo.Id, playerInfo.ColorIndex, playerInfo.Name, playerInfo.Connected, playerInfo.VoiceChatId);

            if (playerInfo.IsSelf)
            {
                m_UISessionStateData.userIdentity = identity;
            }
            else
            {
                // Add or update user in the session
                var roomIndex = Array.FindIndex(m_UISessionStateData.rooms, (r) => ((ProjectRoom)r).project.serverProjectId == playerInfo.RoomId);
                if (roomIndex != -1)
                {
                    var projectRoom = (ProjectRoom)m_UISessionStateData.rooms[roomIndex];
                    var userIndex = projectRoom.users.FindIndex(u => u.matchmakerId == playerInfo.Id);

                    if (userIndex == -1)
                        projectRoom.users.Add(identity);
                    else
                        projectRoom.users[userIndex] = identity;
                }
                else
                {
                    var linkSharedProjectRoom = (ProjectRoom)m_UISessionStateData.linkSharedProjectRoom;
                    var userIndex = linkSharedProjectRoom.users.FindIndex(u => u.matchmakerId == playerInfo.Id);

                    if (userIndex == -1)
                        linkSharedProjectRoom.users.Add(identity);
                    else
                        linkSharedProjectRoom.users[userIndex] = identity;
                }
            }

            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData, UpdateNotification.ForceNotify);
        }

        void RemovePlayerFromSession(PlayerInfo playerInfo)
        {
            if (playerInfo.IsSelf)
            {
                m_UISessionStateData.userIdentity = new UserIdentity(null, -1, m_UISessionStateData.user?.DisplayName, DateTime.MinValue, null);
            }
            else
            {
                //Remove user from session rooms
                var roomIndex = Array.FindIndex(m_UISessionStateData.rooms, (r) => ((ProjectRoom)r).project.serverProjectId == playerInfo.RoomId);
                if (roomIndex != -1)
                {
                    var userIndex = ((ProjectRoom)m_UISessionStateData.rooms[roomIndex]).users.FindIndex(u => u.matchmakerId == playerInfo.Id);
                    ((ProjectRoom)m_UISessionStateData.rooms[roomIndex]).users.RemoveAt(userIndex);
                }
                else
                {
                    var userIndex = ((ProjectRoom)m_UISessionStateData.linkSharedProjectRoom).users.FindIndex(u => u.matchmakerId == playerInfo.Id);
                    ((ProjectRoom)m_UISessionStateData.linkSharedProjectRoom).users.RemoveAt(userIndex);
                }
            }

            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData, UpdateNotification.ForceNotify);
        }

        void AddPlayerToRoom(PlayerInfo playerInfo)
        {
            if (m_ProjectSettingStateData.activeProject.serverProjectId != playerInfo.RoomId)
                return;

            if (playerInfo.IsSelf)
            {
                var localUser = m_RoomConnectionStateData.localUser;
                localUser.matchmakerId = playerInfo.Id;
                m_RoomConnectionStateData.localUser = localUser;
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
                    var participant = m_RoomConnectionStateData.vivoxManager.GetParticipant(identity.vivoxParticipantId);
                    if (participant != null)
                        AssignParticipant(participant);
                }
            }

            m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
        }

        void RemovePlayerFromRoom(PlayerInfo playerInfo)
        {
            //Remove user from room connection
            if (m_ProjectSettingStateData.activeProject.serverProjectId == playerInfo.RoomId)
            {
                if (playerInfo.IsSelf)
                {
                    m_RoomConnectionStateData.localUser = NetworkUserData.defaultData;
                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData);
                }
                else
                {
                    var userRoomIndex = m_RoomConnectionStateData.users.FindIndex((data) => data.matchmakerId == playerInfo.Id);
                    if (userRoomIndex == -1)
                    {
                        Debug.LogError("Player left matchmaker room but was already removed from UI room");
                        return;
                    }

                    OnNetcodeUserLeft(m_RoomConnectionStateData.users[userRoomIndex].matchmakerId, m_RoomConnectionStateData.users[userRoomIndex].networkUser);
                    m_RoomConnectionStateData.users.RemoveAt(userRoomIndex);
                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
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
                m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData);
            }
        }

        void OnNetcodeUserEntered(string userId, NetworkUser user)
        {
            if (user.IsOwner)
            {
                var localUser = m_RoomConnectionStateData.localUser;
                localUser.networkUser = user;
                localUser.matchmakerId = userId;

                m_RoomConnectionStateData.localUser = localUser;
                user.OnValueChange += DispatchLocalUserDataChanged;
                m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);

                SetNetworkSelected((ObjectSelectionInfo)(m_UIProjectStateData.objectSelectionInfo ?? new ObjectSelectionInfo()));
            }
            else
            {
                //Get or create new userdata
                var index = m_RoomConnectionStateData.users.FindIndex((userData) => userData.matchmakerId == userId);

                NetworkUserData userData = index == -1 ? new NetworkUserData(userId) : m_RoomConnectionStateData.users[index];
                userData.networkUser = user;

                if (userData.visualRepresentation == null)
                {
                    userData.visualRepresentation = m_MultiplayerController.CreateVisualRepresentation(m_AvatarRoot);
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

                m_RoomConnectionStateData.users = new List<NetworkUserData>(m_RoomConnectionStateData.users);
                user.OnValueChange += DispatchRemoteUserDataChanged;

                if (userData.vivoxParticipant == null && identity != default)
                {
                    var participant = m_RoomConnectionStateData.vivoxManager.GetParticipant(identity.vivoxParticipantId);
                    if (participant != null)
                        AssignParticipant(participant);
                }
            }

            Debug.LogFormat("{0} MLAPI user [{1}:{2}] joined room", user.IsOwner ? "Local" : "Remote", userId, user.OwnerClientId);
            m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
        }

        void OnNetcodeUserLeft(string userId, NetworkUser user)
        {
            if (userId == m_RoomConnectionStateData.localUser.matchmakerId)
            {
                var localUser = m_RoomConnectionStateData.localUser;
                localUser.networkUser = null;
                m_RoomConnectionStateData.localUser = localUser;
                m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
            }
            else
            {
                var indexToRemove = m_RoomConnectionStateData.users.FindIndex((data) => data.matchmakerId == userId);
                if (indexToRemove == -1)
                    return;
                if (indexToRemove != -1)
                {
                    var originalData = m_RoomConnectionStateData.users[indexToRemove];

                    if (originalData.visualRepresentation != null)
                        Destroy(originalData.visualRepresentation.gameObject);
                    originalData.visualRepresentation = null;
                    originalData.networkUser = null;
                    m_RoomConnectionStateData.users[indexToRemove] = originalData;
                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                }
            }

            if (user != null)
            {
                Debug.LogFormat("{0} MLAPI user [{1}:{2}] left room", user.IsOwner ? "Local" : "Remote", userId, user.OwnerClientId);
            }
            else
            {
                Debug.LogFormat($"MLAPI user [{userId}:] left room");
            }
        }

        void DispatchLocalUserDataChanged(NetworkUser user, string key, object value)
        {
            switch (key)
            {
                case NetworkUser.k_SelectionDataKey:
                    var casted = (Multiplayer.Netcode.StreamKey)value;
                    var streamKey = new StreamKey(casted.Source, PersistentKey.GetKey<SyncObjectInstance>(casted.PersistentKeyName));
                    var localUser = m_RoomConnectionStateData.localUser;
                    localUser.selectedStreamKey = streamKey;

                    if (string.IsNullOrEmpty(streamKey.source) || streamKey.key == default)
                    {
                        localUser.selectedObject = null;
                        m_RoomConnectionStateData.localUser = localUser;
                        m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
                        return;
                    }

                    var manifestRef = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<ManifestActor>();
                    var golRef = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<GameObjectLifecycleActor>();
                    m_Bridge.ForwardRpcBlocking(manifestRef, new GetStableId<StreamKey>(streamKey),
                        (Boxed<EntryStableGuid> b) =>
                        {
                            m_Bridge.ForwardRpcBlocking(golRef, new RunFuncOnGameObject(default, b.Value, go => go),
                                (GameObject go) =>
                                {
                                    localUser.selectedObject = go;
                                    m_RoomConnectionStateData.localUser = localUser;
                                    m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
                                }, ex =>
                                {
                                    if (ex is MissingGameObjectException)
                                    {
                                        localUser.selectedObject = null;
                                        m_RoomConnectionStateData.localUser = localUser;
                                        m_RoomConnectionContextTarget.UpdateValueWith(nameof(m_RoomConnectionStateData.localUser), ref localUser);
                                    }
                                    else if (!(ex is OperationCanceledException))
                                        Debug.LogException(ex);
                                });
                        },
                        ex => Debug.LogException(ex));
                    break;
            }
        }

        public void OnApplicationQuit()
        {
            m_RoomConnectionStateData.vivoxManager.LeaveChannel();
            m_RoomConnectionStateData.vivoxManager.Logout();
        }

        void ClearRoomUIState()
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
            m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
        }

        void ClearSessionUIState()
        {
            m_UISessionStateData.userIdentity = new UserIdentity(null, -1, m_UISessionStateData.user?.DisplayName, DateTime.UtcNow, null);
            foreach (var room in m_UISessionStateData.rooms)
            {
                ((ProjectRoom)room).users.Clear();
            }

            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData, UpdateNotification.ForceNotify);
        }

        void DispatchRemoteUserDataChanged(NetworkUser user, string key, object value)
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
                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
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

                    var manifestHandle = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<ManifestActor>();
                    var goLifecycleHandle = m_Reflect.Hook.Systems.ActorRunner.GetActorHandle<GameObjectLifecycleActor>();
                    if (string.IsNullOrEmpty(streamKey.source) || streamKey.key == default)
                    {
                        originalData.selectedObject = null;
                        m_RoomConnectionStateData.users[index] = originalData;
                        m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                        return;
                    }

                    m_Bridge.ForwardRpcBlocking(manifestHandle, new GetStableId<StreamKey>(streamKey),
                        (Boxed<EntryStableGuid> b) =>
                        {
                            m_Bridge.ForwardRpcBlocking(goLifecycleHandle, new RunFuncOnGameObject(default, b.Value, go => go),
                                (GameObject go) =>
                                {
                                    originalData.selectedObject = go;
                                    m_RoomConnectionStateData.users[index] = originalData;
                                    m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                                }, ex =>
                                {
                                    if (ex is MissingGameObjectException)
                                    {
                                        originalData.selectedObject = null;
                                        m_RoomConnectionStateData.users[index] = originalData;
                                        m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                                    }
                                    else if (!(ex is OperationCanceledException))
                                        Debug.LogException(ex);
                                });
                        },
                        ex =>
                        {
                            // key may not exist yet. This may happen if the scenes are not identical between viewers (live sync, baked objects)
                            // Just remove the selectedObject for this user
                            originalData.selectedObject = null;
                            m_RoomConnectionStateData.users[index] = originalData;
                            m_RoomConnectionContextTarget.UpdateWith(ref m_RoomConnectionStateData, UpdateNotification.ForceNotify);
                        });
                    break;
            }
        }

#if UNITY_ANDROID
        void OnApplicationPause(bool pause)
        {
            m_RoomConnectionStateData.vivoxManager.SetInputMuted(pause);
            m_RoomConnectionStateData.vivoxManager.SetOutputMuted(pause);

            if (pause)
            {
                PlayerClientBridge.MatchmakerManager.Disconnect();
            }
            else if(m_UISessionStateData.user != null)
            {
                PlayerClientBridge.MatchmakerManager.Connect(m_UISessionStateData.user.AccessToken, m_MultiplayerController.connectToLocalServer);
                PlayerClientBridge.MatchmakerManager.MonitorRooms(m_UISessionStateData.rooms.Select(r => ((ProjectRoom)r).project.serverProjectId));
                if (m_ProjectSettingStateData.activeProject != Project.Empty)
                {
                    PlayerClientBridge.MatchmakerManager.JoinRoom(m_ProjectSettingStateData.activeProject.serverProjectId, () => m_ProjectSettingStateData.accessToken.CloudServicesAccessToken);
                }
            }
        }
#endif
    }
}
