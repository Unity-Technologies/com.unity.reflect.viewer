using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Properties;
using Unity.Reflect.Multiplayer;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.Core.Actions
{
    public class ToggleMicrophoneAction : ActionBase
    {
        public object Data { get; }

        public ToggleMicrophoneAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            var localUserPropertyName = nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser);
            var usersPropertyName = nameof(IRoomConnectionDataProvider<NetworkUserData>.users);
            var vivoxManagerPropertyName = nameof(IVivoxDataProvider<VivoxManager>.vivoxManager);

            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(localUserPropertyName))
                && PropertyContainer.IsPathValid(ref stateData, new PropertyPath(usersPropertyName))
                && PropertyContainer.IsPathValid(ref stateData, new PropertyPath(vivoxManagerPropertyName)))
            {
                var localUser = PropertyContainer.GetValue<NetworkUserData>(ref boxed, localUserPropertyName);
                var users = PropertyContainer.GetValue<List<NetworkUserData>>(ref boxed, usersPropertyName);
                var vivoxManager = PropertyContainer.GetValue<VivoxManager>(ref boxed, vivoxManagerPropertyName);

                if (vivoxManager.IsConnected)
                {
                    if (data == localUser.matchmakerId)
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

                            var localParticipant = localUser.vivoxParticipant;
                            bool isMuted = localUser.voiceStateData.isServerMuted;
                            localParticipant.SetIsMuteForAll(localParticipant.ParticipantId, !isMuted,
                                muteTask.Result.AccessToken,
                                callback => Dispatcher.Dispatch(ToggleIsMuteForAllAction.From(!isMuted)));
                        });
                    }
                    else
                    {
                        var user = users.Find(d => d.matchmakerId == data);
                        if (user.vivoxParticipant != null)
                        {
                            user.vivoxParticipant.LocalMute = !user.vivoxParticipant.LocalMute;
                            hasChanged |= SetPropertyValue(ref stateData, ref boxed, usersPropertyName, users);
                        }
                    }
                }
                else
                {
                    localUser.voiceStateData.isServerMuted = !localUser.voiceStateData.isServerMuted;
                    hasChanged |= SetPropertyValue(ref stateData, ref boxed, localUserPropertyName, localUser);
                }
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ToggleMicrophoneAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == RoomConnectionContext.current;
        }
    }

    public class ToggleIsMuteForAllAction : ActionBase
    {
        public object Data { get; }

        public ToggleIsMuteForAllAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var localUserPropertyName = nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser);
            var voiceStatePropertyName = nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser.voiceStateData);
            var prefPropertyName = localUserPropertyName + "." + voiceStatePropertyName + "." + nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser.voiceStateData.isServerMuted);

            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, data);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ToggleIsMuteForAllAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == RoomConnectionContext.current;
        }
    }

    public class MuteUserAction : ActionBase
    {
        public object Data { get; }

        public MuteUserAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)actionData;
            object boxed = stateData;
            var stateDataChanged = false;
            var prefPropertyName = nameof(IRoomConnectionDataProvider<NetworkUserData>.userToMute);

            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                stateDataChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, data);
            }
            if (stateDataChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new MuteUserAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == RoomConnectionContext.current;
        }
    }
}
