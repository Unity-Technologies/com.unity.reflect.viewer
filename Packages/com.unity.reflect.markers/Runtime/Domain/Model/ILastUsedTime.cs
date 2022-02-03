using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    public interface ILastUsedTime
    {
        public DateTime LastUsedTime { get; set; }
    }
}
