using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Markers.UI
{
    public class MarkerEditContext : ContextBase<MarkerEditContext>
    {
        public override List<Type> implementsInterfaces { get; }
    }
}
