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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VivoxUnity
{
    /// <summary>The arguments for ITTSMessageQueue event notifications.</summary>
    public sealed class ITTSMessageQueueEventArgs : EventArgs
    {
        public ITTSMessageQueueEventArgs(TTSMessage message) { Message = message; }

        /// <summary>The TTS message.</summary>
        public TTSMessage Message { get; }
    }

    public interface ITTSMessageQueue : IEnumerable<TTSMessage>
    {
        /// <summary>Raised when a TTS Message is added to the Text-To-Speech subsystem.</summary>
        event EventHandler<ITTSMessageQueueEventArgs> AfterMessageAdded;

        /// <summary>Raised when a TTS Message is removed from the Text-To-Speech subsystem.</summary>
        /// <remarks>May result from either cancellation or playback completion.</remarks>
        event EventHandler<ITTSMessageQueueEventArgs> BeforeMessageRemoved;

        /// <summary>Raised when playback begins for a TTS Message in the collection.</summary>
        event EventHandler<ITTSMessageQueueEventArgs> AfterMessageUpdated;

        /// <summary>
        /// Removes all objects from the collection and cancels them.
        /// </summary>
        /// <seealso cref="ITextToSpeech.CancelAll" />
        void Clear();

        /// <summary>
        /// Determines whether a TTSMessage is in the collection.
        /// </summary>
        /// <param name="message">The TTSMessage to locate in the collection.</param>
        bool Contains(TTSMessage message);

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Removes and returns the oldest TTSMessage in the collection, cancelling it.
        /// </summary>
        /// <seealso cref="ITextToSpeech.CancelMessage" />
        TTSMessage Dequeue();

        /// <summary>
        /// Adds a message and speaks it as the user the collection belongs to.
        /// </summary>
        /// <param name="message">The TTSMessage to add and speak.</param>
        /// <seealso cref="ITextToSpeech.Speak" />
        void Enqueue(TTSMessage message);

        /// <summary>
        /// Returns the oldest TTSMessage in the collection without removing it.
        /// </summary>
        TTSMessage Peek();

        /// <summary>
        /// Removes a specific message from the collection, cancelling it.
        /// </summary>
        /// <param name="message">The TTSMessage to remove and cancel.</param>
        /// <seealso cref="ITextToSpeech.CancelMessage" />
        bool Remove(TTSMessage message);
    }

    /// <summary>
    /// Interface for events and methods related to Text-To-Speech.
    /// </summary>
    public interface ITextToSpeech : INotifyPropertyChanged
    {
        /// <summary>
        /// All voices available to the Text-To-Speech subsystem for speech synthesis.
        /// </summary>
        ReadOnlyCollection<ITTSVoice> AvailableVoices { get; }

        /// <summary>
        /// The voice used by Text-To-Speech methods called from this ILoginSession.
        /// </summary>
        /// <remarks>
        /// The SDK default voice will be used if never set. Valid ITTSVoices can be obtained from AvailableVoices.
        /// When setting, ObjectNotFoundException will be raised if the new voice isn't available
        /// (possible, for instance, when loaded from saved settings after updating).
        /// </remarks>
        ITTSVoice CurrentVoice { get; set; }

        /// <summary>
        /// Injects a new Text-To-Speech message into the TTS subsystem.
        /// </summary>
        /// <param name="message">A TTSMessage containing the text to be converted into speech and the destination for TTS injection.</param>
        /// <remarks>
        /// The Voice and State properties of message will be set by this function.
        /// See CurrentVoice for how the ITTSVoice used for speech synthesis will be selected.
        /// Synthesized speech sent to remote destinations will play in connected channel sessions
        /// according to the transmission policy (the same sessions that basic voice transmits to).
        /// </remarks>
        void Speak(TTSMessage message);

        /// <summary>
        /// Cancels a single currently playing or enqueued Text-To-Speech message.
        /// </summary>
        /// <param name="message">The TTSMessage to cancel.</param>
        /// <remarks>
        /// In destinations with queues, cancelling an ongoing message will automatically trigger playback of
        /// the next message. Cancelling an enqueued message will shift all later messages up one place in the queue.
        /// </remarks>
        void CancelMessage(TTSMessage message);

        /// <summary>
        /// Cancels all Text-To-Speech messages in a destination (ongoing and enqueued).
        /// </summary>
        /// <param name="destination">The TTSDestination to clear of messages.</param>
        /// <remarks>
        /// The TTSDestinations QueuedRemoteTransmission and QueuedRemoteTransmissionWithLocalPlayback
        /// share a queue but are not the same destination. Canceling all messages in one of these destinations
        /// will automatically trigger playback of the next message from the other in the shared queue.
        /// </remarks>
        void CancelDestination(TTSDestination destination);

        /// <summary>
        /// Cancels all Text-To-Speech messages (ongoing and enqueued) from all destinations.
        /// </summary>
        void CancelAll();

        /// <summary>
        /// Contains all TTS messages playing or waiting to be played in all destinations.
        /// </summary>
        /// <remarks>
        /// Use the ITTSMessageQueue events to get notifications when messages are spoken, canceled, or playback starts or ends.
        /// Methods to Enqueue(), Dequeue(), Remove(), or Clear() items directly from this collection will result in the same
        /// behaviour as using other class methods to Speak() or Cancel*() TTS messages in the Text-To-Speech subsystem.
        /// </remarks>
        ITTSMessageQueue Messages { get; }

        /// <summary>
        /// Retrieves ongoing or enqueued TTS messages in the specified destination.
        /// </summary>
        /// <param name="destination">The TTSDestination to retrieve messages for.</param>
        /// <returns>A queue containing the messages of a single destination.</returns>
        /// <remarks>
        /// Queued destinations return their ITTSMessageQueue in queue order, and others in the order that speech was injected.
        /// </remarks>
        ReadOnlyCollection<TTSMessage> GetMessagesFromDestination(TTSDestination destination);
    }
}
