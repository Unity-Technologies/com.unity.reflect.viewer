using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VivoxUnity;
using System.Timers;
using Unity.Reflect.Multiplayer;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer
{
    public class VivoxManager
    {
        Client client;

        [SerializeField]
        ILoginSession loginSession;

        ChannelId channelId = null;

        [SerializeField]
        bool allowAudio = true;
        [SerializeField]
        bool allowText = true;
        [SerializeField]
        bool switchTransmission = true;

        public bool IsConnected => loginSession != null && loginSession.State == LoginState.LoggedIn;

        public Task LoginAsync(VoiceLoginCredentials credentials)
        {
            if (client == null)
            {
                client = new Client();
                var config = new VivoxConfig
                {
                    InitialLogLevel = vx_log_level.log_error
                };
                client.Initialize(config);
            }

            var tcs = new TaskCompletionSource<byte>();

            var accountId = new AccountId(credentials.AccountId);
            loginSession = client.GetLoginSession(accountId);

            loginSession.BeginLogin(credentials.ServerAddress, credentials.AccessToken, ar =>
            {
                try
                {
                    loginSession.EndLogin(ar);
                    tcs.SetResult(0);
                    //Debug.Log("[Vivox] Login sucessful");
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                    Debug.LogError("[Vivox] Login error " + e.Message);
                }
            });

            return tcs.Task;
        }

        public void SetOutputMuted(bool muted)
        {
            if (client != null)
                client.AudioOutputDevices.Muted = muted;
        }

        public void SetInputMuted(bool muted)
        {
            if (client != null)
                client.AudioInputDevices.Muted = muted;
        }

        public Task JoinChannelAsync(VoiceChannelCredentials credentials)
        {
            if (loginSession == null)
            {
                throw new InvalidOperationException("[Vivox] Trying to join a chnanel without being logged in");
            }

            var tcs = new TaskCompletionSource<byte>();

            var tmpChannelId = new ChannelId(credentials.ChannelId);

            var channelSession = loginSession.GetChannelSession(tmpChannelId);

            // Connect to channel
            channelSession.BeginConnect(allowAudio, allowText, switchTransmission, credentials.AccessToken, ar =>
            {
                try
                {
                    channelSession.EndConnect(ar);
                    channelId = tmpChannelId;
                    channelSession.Participants.AfterKeyAdded += OnParticipantJoined;
                    tcs.SetResult(0);

                    //Debug.Log($"[Vivox] Join channel [{channelId}] sucessful");
                    // Subscribe to property changes for all channels.
                    //channelSession.PropertyChanged += SourceOnChannelPropertyChanged;
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                    Debug.LogError($"[Vivox] Join Channel [{channelId}] error: {e}");
                }
                // Reaching this point indicates no error occurred, but the user is still not �in the channel� until the AudioState and/or TextState are in ConnectionState.Connected.
            });

            return tcs.Task;
        }

        public event Action<IParticipant> onVivoxParticipantJoined;

        private void OnParticipantJoined(object sender, KeyEventArg<string> e)
        {
            //Debug.Log($"[Vivox] Participant [{e.Key}]  joined");
            onVivoxParticipantJoined?.Invoke(loginSession.GetChannelSession(channelId).Participants[e.Key]);
        }

        public void LeaveChannel()
        {
            if (channelId == null)
                return;

            loginSession?.GetChannelSession(channelId).Disconnect();
            channelId = null;
        }

        public void Logout()
        {
            loginSession?.Logout();
            client?.Uninitialize();
            client = null;
        }

        public IParticipant GetParticipant(string voiceChatUserName)
        {
            if(channelId != null && loginSession.GetChannelSession(channelId).Participants.ContainsKey(voiceChatUserName))
                return loginSession.GetChannelSession(channelId).Participants[voiceChatUserName];
            return null;
        }
    }
}
