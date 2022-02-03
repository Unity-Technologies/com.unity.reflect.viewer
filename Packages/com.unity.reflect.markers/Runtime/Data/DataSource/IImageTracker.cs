using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

public interface IImageTracker
{
    Action<Pose, string> OnTrackedFound { get; set; }
    Action<Pose, string> OnTrackedPositionUpdate { get; set; }
    Action OnTrackingLost { get; set; }
    Pose Value { get; }

    bool IsAvailable { get; }
    bool IsTracking { get; }

    void Run();
    void Stop();
}
