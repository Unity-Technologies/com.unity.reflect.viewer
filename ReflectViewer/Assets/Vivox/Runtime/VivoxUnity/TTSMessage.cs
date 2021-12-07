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
using VivoxUnity.Properties;

namespace VivoxUnity
{
    /// <summary>
    /// Represents a message to be spoken in the Text-To-Speech subsystem.
    /// </summary>
    public sealed class TTSMessage
    {
        private readonly string _text;
        private readonly TTSDestination _destination;
        private ITTSVoice _voice;
        private TTSMessageState _state;
        private uint _numConsumers;
        private double _duration;
        private uint _key;
        private ITextToSpeech _ttsSubSystem;

        /// <summary>
        /// Creates a new TTS message in the NotEnqueued state.
        /// </summary>
        /// <param name="text">The text to be synthesized into speech.</param>
        /// <param name="name">The destination for this message.</param>
        /// <remarks>
        /// To synthesize this text into speech and inject it into the destination,
        /// use ILoginSession.TTS.Speak(), or ILoginSession.TTS.Messages.Enqueue().
        /// </remarks>
        public TTSMessage(string text, TTSDestination destination)
        {
            if (text.Length > VivoxCoreInstance.VX_TTS_CHARACTER_COUNT_LIMIT)
                throw new ArgumentOutOfRangeException($"{GetType().Name}: {text.Length} exceeds the " +
                    $"{VivoxCoreInstance.VX_TTS_CHARACTER_COUNT_LIMIT} maximum characters allowed for input text");

            _text = text;
            _destination = destination;
            _voice = null;
            _state = TTSMessageState.NotEnqueued;
            _numConsumers = 0;
            _duration = 0.0;
            _key = 0;
            _ttsSubSystem = null;
        }

        /// <summary>
        /// The text to be synthesized into speech.
        /// </summary>
        public string Text { get { return _text; } }

        /// <summary>
        /// The destination of this message.
        /// </summary>
        public TTSDestination Destination { get { return _destination; } }

        /// <summary>
        /// Voice used to synthesize this message.
        /// </summary>
        public ITTSVoice Voice {
            get { return _voice; }
            internal set
            {
                _voice = value;
                OnPropertyChanged(nameof(Voice));
            }
        }

        /// <summary>
        /// The Playing/Enqueued state of the message.
        /// </summary>
        public TTSMessageState State {
            get { return _state; }
            internal set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        /// <summary>
        /// The number of active sessions (for remote playback destinations) or local players
        /// (for local playback destinations), or both, that the TTS message is playing into. 
        /// </summary>
        /// <remarks>NumConsumers is set when playback starts, and always 0 until then.</remarks>
        public uint NumConsumers
        {
            get { return _numConsumers; }
            internal set
            {
                _numConsumers = value;
                OnPropertyChanged(nameof(NumConsumers));
            }
        }

        /// <summary>
        /// The duration of the synthesized voice clip in seconds.
        /// </summary>
        /// <remarks>Duration is set when playback starts, and always 0.0 until then.</remarks>
        public double Duration
        {
            get { return _duration; }
            internal set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        internal uint Key
        {
            get { return _key; }
            set { _key = value; }
        }
        internal ITextToSpeech TTS
        {
            get { return _ttsSubSystem; }
            set { _ttsSubSystem = value; }
        }

        /// <summary>
        /// Injects a new Text-To-Speech message as the supplied user.
        /// </summary>
        /// <param name="userSpeaking">The local user which will speak the message.</param>
        /// <remarks>This method is a shortcut for `userSpeaking.TTS.Speak(this)`.</remarks>
        public void Speak(ILoginSession userSpeaking)
        {
            userSpeaking.TTS.Speak(this);
        }

        /// <summary>
        /// Cancels this message if Playing or Enqueued.
        /// </summary>
        public void Cancel()
        {
            if (null == _ttsSubSystem)
                throw new InvalidOperationException($"{GetType().Name}: message is not Playing or Enqueued.");
            else
                _ttsSubSystem.CancelMessage(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (GetType() != obj.GetType()) return false;

            return Equals((TTSMessage)obj);
        }

        private bool Equals(TTSMessage other)
        {
            return ITTSVoice.Equals(Voice, other.Voice) && string.Equals(Text, other.Text)
                && TTSDestination.Equals(Destination, other.Destination)
                && TTSMessageState.Equals(State, other.State) && uint.Equals(NumConsumers, other.NumConsumers)
                && double.Equals(Duration, other.Duration) && uint.Equals(Key, other.Key)
                && ITextToSpeech.Equals(TTS, other.TTS);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hc = (_text?.GetHashCode() ?? 0);
                hc *= 397;
                hc ^= (_destination.GetHashCode());
                hc *= 397;
                hc ^= (_voice.GetHashCode());
                hc *= 397;
                hc ^= (_state.GetHashCode());
                hc *= 397;
                hc ^= (_numConsumers.GetHashCode());
                hc *= 397;
                hc ^= (_duration.GetHashCode());
                hc *= 397;
                hc ^= (_key.GetHashCode());
                hc *= 397;
                hc ^= (_ttsSubSystem.GetHashCode());
                return hc;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal bool AlreadySpoken()
        {
            return 0 != _key;
        }
    }
}
