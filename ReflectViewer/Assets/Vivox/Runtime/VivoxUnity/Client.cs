/*
Copyright (c) 2014-2018 by Mercer Road Corp

Permission to use, copy, modify or distribute this software in binary or source form
for any purpose is allowed only under explicit prior consent in writing from Mercer Road Corp

THE SOFTWARE IS PROVIDED "AS IS" AND MERCER ROAD CORP DISCLAIMS
ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL MERCER ROAD CORP
BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL
DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR
PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS
ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using VivoxUnity.Common;
using VivoxUnity.Private;

namespace VivoxUnity
{
    /// <summary>
    /// Provides access to the Vivox System. An application should have one and only one Client object
    /// (this may be enforced with a singleton at a later date. 
    /// </summary>
    public sealed class Client : IDisposable
    {
        #region Member Variables
        private readonly ReadWriteDictionary<AccountId, ILoginSession, LoginSession> _loginSessions = new ReadWriteDictionary<AccountId, ILoginSession, LoginSession>();
        private readonly AudioInputDevices _inputDevices = new AudioInputDevices(VxClient.Instance);
        private readonly AudioOutputDevices _outputDevices = new AudioOutputDevices(VxClient.Instance);
        private static int _nextHandle;
        private string _connectorHandle;
        private readonly Queue<IAsyncResult> _pendingConnectorCreateRequests = new Queue<IAsyncResult>();
        private bool _ttsIsInitialized;

        private uint _ttsManagerId;
        internal uint TTSManagerId => _ttsManagerId;
        #endregion

        #region Helpers

        void CheckInitialized()
        {
            if (!Initialized)
                throw new InvalidOperationException();
        }

        #endregion

        /// <summary>
        /// Initializes this Client instance.
        /// If the client is already initialized, it will do nothing.
        /// <param name="config">Optional: config to set on initialize</param>
        /// </summary>
        public void Initialize(VivoxConfig config = null)
        {
            if (Initialized)
                return;

            VxClient.Instance.Start(config);

            // Refresh audio devices to ensure they are up to date when the client is initialized.
            AudioInputDevices.BeginRefresh(null);
            AudioOutputDevices.BeginRefresh(null);
        }

        internal IAsyncResult BeginGetConnectorHandle(Uri server, AsyncCallback callback)
        {
            CheckInitialized();

            var result = new AsyncResult<string>(callback);
            if (!string.IsNullOrEmpty(_connectorHandle))
            {
                result.SetCompletedSynchronously(_connectorHandle);
                return result;
            }

            _pendingConnectorCreateRequests.Enqueue(result);
            if (_pendingConnectorCreateRequests.Count > 1)
            {
                return result;
            }

            var request = new vx_req_connector_create_t();
            request.acct_mgmt_server = server.ToString();
            string connectorHandle = $"C{_nextHandle++}";
            request.connector_handle = connectorHandle;
            VxClient.Instance.BeginIssueRequest(request, ar =>
            {
                try
                {
                    VxClient.Instance.EndIssueRequest(ar);
                }
                catch (Exception e)
                {
                    VivoxDebug.Instance.VxExceptionMessage($"{request.GetType().Name} failed: {e}");
                    _connectorHandle = null;
                    while (_pendingConnectorCreateRequests.Count > 0)
                    {
                        ((AsyncResult<string>)(_pendingConnectorCreateRequests).Dequeue()).SetComplete(e);
                    }
                    if (VivoxDebug.Instance.throwInternalExcepetions)
                    {
                        throw;
                    }
                    return;
                }
                _connectorHandle = connectorHandle;
                while (_pendingConnectorCreateRequests.Count > 0)
                {
                    ((AsyncResult<string>)(_pendingConnectorCreateRequests).Dequeue()).SetComplete(_connectorHandle);
                }
            });
            return result;
        }

        internal string EndGetConnectorHandle(IAsyncResult result)
        {
            return ((AsyncResult<string>)result).Result;
        }

        /// <summary>
        /// Uninitialize this Client instance.
        /// If this Client instance is not initialized, it will do nothing.
        /// </summary>
        public void Uninitialize()
        {
            if (Initialized)
            {
                VxClient.Instance.Stop();
                TTSShutdown();
                _inputDevices.Clear();
                _outputDevices.Clear();
                _loginSessions.Clear();
                _connectorHandle = null;
            }
        }

        public static void Cleanup()
        {
            VxClient.Instance.Stop();
            VivoxCoreInstance.Uninitialize();
        }

        /// <summary>
        /// The internal version the low level vivoxsdk library
        /// </summary>
        public static string InternalVersion => VxClient.GetVersion();

        /// <summary>
        /// Gets the LoginSession object for the provided accountId, creating one if necessary
        /// </summary>
        /// <param name="accountId">the AccountId</param>
        /// <returns>the login session for that accountId</returns>
        /// <exception cref="ArgumentNullException">thrown when accountId is null or empty</exception>
        /// <remarks>If a new LoginSession is created, LoginSessions.AfterKeyAdded will be raised.</remarks>
        public ILoginSession GetLoginSession(AccountId accountId)
        {
            if (AccountId.IsNullOrEmpty(accountId))
                throw new ArgumentNullException(nameof(accountId));

            CheckInitialized();
            if (_loginSessions.ContainsKey(accountId))
            {
                return _loginSessions[accountId];
            }
            var loginSession = new LoginSession(this, accountId);
            _loginSessions[accountId] = loginSession;
            loginSession.PropertyChanged += delegate (object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != nameof(loginSession.State)) return;
                if (loginSession.State == LoginState.LoggedOut)
                    _loginSessions.Remove(accountId);
            };
            return loginSession;
        }

        /// <summary>
        /// Whether or not the client is initialized. True if initialized, false if uninitialized. 
        /// The state of this is managed by the core SDK and the wrapper is simply forwarding that info.
        /// </summary>
        public bool Initialized => Convert.ToBoolean(VivoxCoreInstancePINVOKE.vx_is_initialized());

        /// <summary>
        /// All the Login Sessions associated with this Client instance.
        /// </summary>
        public IReadOnlyDictionary<AccountId, ILoginSession> LoginSessions => _loginSessions;
        /// <summary>
        /// The Audio Input Devices associated with this Client instance
        /// </summary>
        public IAudioDevices AudioInputDevices => _inputDevices;
        /// <summary>
        /// The Audio Output Devices associated with this Client instance
        /// </summary>
        public IAudioDevices AudioOutputDevices => _outputDevices;

        /// <summary>
        /// Indicates whether Vivox's Software Echo Cancellation feature is enabled or not.
        /// This is completely independent of any hardware-provided Acoustic Echo Cancellation that may be present on a device.
        /// </summary>
        public bool IsAudioEchoCancellationEnabled => VivoxCoreInstance.IsAudioEchoCancellationEnabled();

        /// <summary>
        /// Used for turning Vivox's audio echo cancellation feature on or off.
        /// </summary>
        /// <param name="onOff">True for on, False for off.</param>
        public void SetAudioEchoCancellation(bool onOff)
        {
            CheckInitialized();

            if (IsAudioEchoCancellationEnabled != onOff)
            {
                VivoxCoreInstance.vx_set_vivox_aec_enabled(Convert.ToInt32(onOff));
            }
        }

        void IDisposable.Dispose()
        {
            Uninitialize();
        }

        internal static string GetRandomUserId(string prefix)
        {
            return Helper.GetRandomUserId(prefix);
        }

        internal static string GetRandomChannelUri(string prefix, string realm)
        {
            return Helper.GetRandomChannelUri(prefix, realm);
        }

        public static void Run(LoopDone done)
        {
            MessagePump.Instance.RunUntil(done);
        }

        public static bool Run(WaitHandle handle, TimeSpan until)
        {
            DateTime then = DateTime.Now + until;
            MessagePump.Instance.RunUntil(() => MessagePump.IsDone(handle, then));
            if (handle != null) return handle.WaitOne(0);
            return false;
        }

        /// <summary>
        /// Process all asynchronous messages. 
        /// This must be called periodically by the application at a frequency of no less than every 100ms.
        /// </summary>
        public static void RunOnce()
        {
            MessagePump.Instance.RunUntil(() => MessagePump.IsDone(null, DateTime.Now));
        }

        internal bool TTSInitialize()
        {
            if (!_ttsIsInitialized)
            {
                // NB: once there's more than one tts_engine type available we'll need to make a public TTSInitialize() method.
                vx_tts_status status = VivoxCoreInstance.vx_tts_initialize(vx_tts_engine_type.tts_engine_vivox_default, out _ttsManagerId);
                if (vx_tts_status.tts_status_success == status)
                    _ttsIsInitialized = true;
            }
            return _ttsIsInitialized;
        }

        internal void TTSShutdown()
        {
            if (_ttsIsInitialized)
            {
                var status = VivoxCoreInstance.vx_tts_shutdown();
                foreach (ILoginSession session in _loginSessions)
                {
                    ((TextToSpeech)session.TTS).CleanupTTS();
                }
                _ttsIsInitialized = false;
            }
        }
    }
}
