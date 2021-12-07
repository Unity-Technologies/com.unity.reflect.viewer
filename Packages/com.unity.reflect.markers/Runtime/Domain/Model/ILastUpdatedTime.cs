using System;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    public interface ILastUpdatedTime
    {
        public DateTime LastUpdatedTime { get; set; }
    }
}
