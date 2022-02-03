using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public interface IMarkerAnchorer
    {
        public GameObject Anchor(Pose position);
    }
}
