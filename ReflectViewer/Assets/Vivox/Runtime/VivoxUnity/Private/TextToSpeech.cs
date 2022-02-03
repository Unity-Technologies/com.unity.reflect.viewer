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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace VivoxUnity.Private
{
    internal sealed class TTSMessageQueue : ITTSMessageQueue
    {
        private readonly List<TTSMessage> _messages = new List<TTSMessage>();
        private TextToSpeech _parentTTS = null;
        public event EventHandler<ITTSMessageQueueEventArgs> AfterMessageAdded;
        public event EventHandler<ITTSMessageQueueEventArgs> BeforeMessageRemoved;
        public event EventHandler<ITTSMessageQueueEventArgs> AfterMessageUpdated;

        internal TTSMessageQueue(TextToSpeech tts)
        {
            _parentTTS = tts;
        }

        // Internal. Remove messages from the collection without canceling them.
        // Used during tts subsystem shutdown, or when message playback ends/fails.
        internal void Cleanup(TTSMessage message = null)
        {
            if (null == message)
            {
                foreach (TTSMessage msg in _messages)
                    BeforeMessageRemoved?.Invoke(this, new ITTSMessageQueueEventArgs(msg));
                _messages.Clear();
            }
            else if (_messages.Contains(message))
            {
                BeforeMessageRemoved?.Invoke(this, new ITTSMessageQueueEventArgs(message));
                _messages.Remove(message);
            }
        }

        public void Clear()
        {
            foreach (TTSMessage message in _messages.ToList())
            {
                Remove(message);
            }
            _messages.Clear(); // should be redundant
        }

        public bool Contains(TTSMessage message)
        {
            return _messages.Contains(message);
        }

        public int Count => _messages.Count;

        public TTSMessage Dequeue()
        {
            if (_messages.Count == 0)
                throw new InvalidOperationException($"{GetType().Name}: The collection is empty");

            TTSMessage message = _messages[0];
            Remove(message);
            return message;
        }

        public void Enqueue(TTSMessage message)
        {
            if (null == _parentTTS)
                throw new InvalidOperationException("No associated Text-To-Speech subsystem for injection");
            else if (message.AlreadySpoken())
                throw new InvalidOperationException($"{GetType().Name}: message has already been spoken");

            _parentTTS.TTSInitialize();

            uint messageKey;
            message.Voice = _parentTTS.CurrentVoice;
            vx_tts_status status = VivoxCoreInstance.vx_tts_speak(_parentTTS.TTSManagerId, message.Voice.Key, message.Text, (vx_tts_destination)message.Destination, out messageKey);
            if (TextToSpeech.IsNotTTSError(status))
            {
                message.Key = messageKey;
                message.TTS = _parentTTS;
                message.State = TTSMessageState.Enqueued;
                message.PropertyChanged += OnMessage_PropertyChanged;
                _messages.Add(message);
                AfterMessageAdded?.Invoke(this, new ITTSMessageQueueEventArgs(message));
            }
            else
            {
                throw new VivoxApiException((int)status);
            }
        }

        private void OnMessage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
                AfterMessageUpdated?.Invoke(this, new ITTSMessageQueueEventArgs((TTSMessage)sender));
        }

        public TTSMessage Peek()
        {
            if (_messages.Count == 0)
                throw new InvalidOperationException($"{GetType().Name}: The collection is empty");

            return _messages[0];
        }

        public bool Remove(TTSMessage message)
        {
            if (!_messages.Contains(message))
                return false;
            if (null == _parentTTS)
                throw new InvalidOperationException("No associated Text-To-Speech subsystem for cancellation");

            _parentTTS.TTSInitialize();

            vx_tts_status status = VivoxCoreInstance.vx_tts_cancel_utterance(_parentTTS.TTSManagerId, message.Key);
            if (TextToSpeech.IsNotTTSError(status))
            {
                BeforeMessageRemoved?.Invoke(this, new ITTSMessageQueueEventArgs(message));
                return _messages.Remove(message);
            }
            else
            {
                throw new VivoxApiException((int)status);
            }
        }

        public IEnumerator<TTSMessage> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class TextToSpeech : ITextToSpeech
    {

        #region Member Variables

        private readonly Client _client;
        private List<ITTSVoice> _ttsAvailableVoices = new List<ITTSVoice>();
        private TTSVoice _ttsCurrentVoice = null;
        private readonly TTSMessageQueue _ttsMessages = null;

        #endregion

        internal TextToSpeech(Client client)
        {
            _client = client;
            _ttsMessages = new TTSMessageQueue(this);
            VxClient.Instance.EventMessageReceived += InstanceOnEventMessageReceived;
        }

        ~TextToSpeech()
        {
            CleanupTTS();
        }

        #region Event Handlers

        private void InstanceOnEventMessageReceived(vx_evt_base_t eventMessage)
        {
            switch ((vx_event_type)eventMessage.type)
            {
                case vx_event_type.evt_tts_injection_started:
                    HandleTTSInjectionStarted(eventMessage);
                    break;
                case vx_event_type.evt_tts_injection_ended:
                    HandleTTSInjectionEnded(eventMessage);
                    break;
                case vx_event_type.evt_tts_injection_failed:
                    HandleTTSInjectionFailed(eventMessage);
                    break;
            }
        }

        private void HandleTTSInjectionStarted(vx_evt_base_t eventMessage)
        {
            vx_evt_tts_injection_started_t evt = eventMessage;
            Debug.Assert(evt != null);
            TTSMessage matchingMessage = GetTTSMessageFromEvt(evt.utterance_id);
            if (null != matchingMessage)
            {
                matchingMessage.NumConsumers = evt.num_consumers;
                matchingMessage.Duration = evt.utterance_duration;
                matchingMessage.State = TTSMessageState.Playing;
            }
        }

        private void HandleTTSInjectionEnded(vx_evt_base_t eventMessage)
        {
            vx_evt_tts_injection_ended_t evt = eventMessage;
            Debug.Assert(evt != null);
            TTSMessage matchingMessage = GetTTSMessageFromEvt(evt.utterance_id);
            if (null != matchingMessage)
            {
                _ttsMessages.Cleanup(matchingMessage);
            }
        }

        private void HandleTTSInjectionFailed(vx_evt_base_t eventMessage)
        {
            vx_evt_tts_injection_failed_t evt = eventMessage;
            Debug.Assert(evt != null);
            TTSMessage matchingMessage = GetTTSMessageFromEvt(evt.utterance_id);
            if (null != matchingMessage)
            {
                _ttsMessages.Cleanup(matchingMessage);
                throw new VivoxApiException((int)evt.status);
            }
        }

        #endregion

        #region ITextToSpeech Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyCollection<ITTSVoice> AvailableVoices
        {
            get
            {
                // We can cache the available voices the first time we retrieve them since they won't change.
                // If they're already cached, or we can't initialize tts, return whatever we have.
                if (_ttsAvailableVoices.Count == 0)
                {
                    _client.TTSInitialize();
                    vx_tts_voice_t[] voices = VivoxCoreInstance.vx_tts_get_voices(_client.TTSManagerId);
                    _ttsAvailableVoices = voices.Select(v => new TTSVoice(v)).ToList<ITTSVoice>();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableVoices)));
                }

                return _ttsAvailableVoices.AsReadOnly();
            }
        }

        public ITTSVoice CurrentVoice
        {
            get
            {
                // If the current voice has already been set, or hasn't and can't be, return whatever it is.
                // Must first access via AvailableVoices instead of _ttsAvailableVoices in case it's uninitialized.
                if (_ttsCurrentVoice != null || AvailableVoices.Count == 0)
                    return _ttsCurrentVoice;

                // If it hasn't been set and can be, set it to the SDK default.
                // We consider the first available voice returned the SDK default,
                // to be used if no global or user default voice is chosen.
                _ttsCurrentVoice = (TTSVoice)_ttsAvailableVoices[0];

                return _ttsCurrentVoice;
            }
            set
            {
                // Check if an ITTSVoice with this name is available and return the one in the list (in case VoiceId has changed).
                // Guards against stale value loaded from saved settings, especially when using external TTS voices (e.g. from OS)
                // Must access via AvailableVoices instead of _ttsAvailableVoices in case it's uninitialized.
                ITTSVoice newVoice = AvailableVoices.FirstOrDefault(v => v.Name == value.Name);
                if (null == newVoice)
                {
                    throw new ArgumentException($"No voice with name '{value.Name}' found in AvailableVoices");
                }
                else
                {
                    _ttsCurrentVoice = (TTSVoice)newVoice;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentVoice)));
                }
            }
        }

        public void Speak(TTSMessage message)
        {
            _ttsMessages.Enqueue(message);
        }

        public void CancelMessage(TTSMessage message)
        {
            if (!message.AlreadySpoken())
                throw new InvalidOperationException($"{GetType().Name}: message is not Playing or Enqueued.");
            else
                _ttsMessages.Remove(message);
        }

        public void CancelDestination(TTSDestination destination)
        {
            ReadOnlyCollection<TTSMessage> destCol = GetMessagesFromDestination(destination);
            if (destCol.Count == 0)
                return;

            _client.TTSInitialize();

            vx_tts_status status = VivoxCoreInstance.vx_tts_cancel_all_in_dest(TTSManagerId, (vx_tts_destination)destination);
            if (IsNotTTSError(status))
                foreach (TTSMessage message in destCol)
                    _ttsMessages.Cleanup(message);
            else
                throw new VivoxApiException((int)status);
        }

        public void CancelAll()
        {
            if (_ttsMessages.Count == 0)
                return;

            _client.TTSInitialize();

            vx_tts_status status = VivoxCoreInstance.vx_tts_cancel_all(TTSManagerId);
            if (IsNotTTSError(status))
                _ttsMessages.Cleanup();
            else
                throw new VivoxApiException((int)status);
        }

        public ITTSMessageQueue Messages => _ttsMessages;

        public ReadOnlyCollection<TTSMessage> GetMessagesFromDestination(TTSDestination destination)
        {
            return _ttsMessages.Where(m => m.Destination == destination).ToList().AsReadOnly();
        }

        #endregion

        #region Internal

        internal uint TTSManagerId => _client.TTSManagerId;

        internal bool TTSInitialize()
        {
            return _client.TTSInitialize();
        }

        internal static bool IsNotTTSError(vx_tts_status status)
        {
            return (status == vx_tts_status.tts_status_success
                 || status == vx_tts_status.tts_status_input_text_was_enqueued
                 || status == vx_tts_status.tts_status_enqueue_not_necessary);
        }

        internal TTSMessage GetTTSMessageFromEvt(uint utteranceId)
        {
            // If a TTS message matching utteranceId is found, return it, otherwise return null.
            return _ttsMessages.FirstOrDefault(m => m.Key == utteranceId);
        }

        internal void CleanupTTS()
        {
            _ttsAvailableVoices.Clear();
            _ttsCurrentVoice = null;
            _ttsMessages.Cleanup();
            VxClient.Instance.EventMessageReceived -= InstanceOnEventMessageReceived;
        }

        #endregion
    }
}
