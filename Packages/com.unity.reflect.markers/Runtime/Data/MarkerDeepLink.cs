using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers
{
    public class MarkerDeepLink
    {
        public string Value => $"?marker={marker.Id}";

        IMarker marker;
        Uri baseUri;

        public MarkerDeepLink(Uri baseUri, IMarker marker)
        {
            this.marker = marker;
        }

        private MarkerDeepLink()
        {
            throw new NotSupportedException("Project  & Marker required to form a Deep Link");
        }
    }
}
