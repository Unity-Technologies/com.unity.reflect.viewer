using System;
using Unity.Reflect.Multiplayer;
using Unity.SpatialFramework.Avatar;
using UnityEngine;
using VivoxUnity;

namespace Unity.Reflect.Viewer
{
    [Serializable]
    public struct VoiceStateData
    {
        public static readonly VoiceStateData defaultData = new VoiceStateData { micVolume = 0.0f, isServerMuted = true, isTransmitting = false, isLocallyMuted = false };

        [Range(0.0f, 1.0f)]
        public float micVolume;
        public bool isTransmitting;
        public bool isLocallyMuted;
        public bool isServerMuted;
    }

    [Serializable]
    public struct NetworkUserData : IEquatable<NetworkUserData>
    {
        public static readonly NetworkUserData defaultData = new NetworkUserData(null);

        public string matchmakerId;
        public DateTime lastUpdateTimeStamp;

        public NetworkUser networkUser;
        public AvatarControls visualRepresentation;
        public StreamKey selectedStreamKey;
        public GameObject selectedObject;
        [SerializeReference]
        public IParticipant vivoxParticipant;
        public VoiceStateData voiceStateData;

        public NetworkUserData(string id)
        {
            this.matchmakerId = id;
            this.visualRepresentation = null;
            this.networkUser = null;
            this.lastUpdateTimeStamp = DateTime.Now;
            selectedStreamKey = default;
            selectedObject = null;
            vivoxParticipant = null;
            voiceStateData = VoiceStateData.defaultData;
        }

        public static bool operator ==(NetworkUserData a, NetworkUserData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(NetworkUserData a, NetworkUserData b)
        {
            return !(a == b);
        }

        public bool Equals(NetworkUserData other)
        {
            return matchmakerId == other.matchmakerId &&
                networkUser == other.networkUser &&
                visualRepresentation == other.visualRepresentation &&
                lastUpdateTimeStamp == other.lastUpdateTimeStamp;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkUserData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = matchmakerId.GetHashCode();
                hashCode = (hashCode * 397) ^ lastUpdateTimeStamp.GetHashCode();
                if (networkUser != null)
                    hashCode = (hashCode * 397) ^ networkUser.GetHashCode();
                if (visualRepresentation != null)
                    hashCode = (hashCode * 397) ^ visualRepresentation.GetHashCode();
                return hashCode;
            }
        }
    }
}
