using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class MarkerDeepLinkBarcodeParser : MonoBehaviour, IBarcodeDataParser
    {

        [SerializeField]
        MarkerController m_MarkerController;

        void Start()
        {
            m_MarkerController.BarcodeDataParser = this;
        }

        public bool TryParse(string inputData, out IMarker marker)
        {
            try
            {
                Uri uri = new Uri(inputData);
                Dictionary<string, string> queryArgs = GetQueryArgs(uri);

                var key = m_MarkerController.ProjectLinkSource.Key;

                if (queryArgs.TryGetValue(key, out string value))
                {
                    var markerSuccess = m_MarkerController.MarkerStorage.Get(value);
                    if (markerSuccess != null)
                    {
                        marker = markerSuccess.Value;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }


            marker = null;
            return false;
        }

        public string Generate(IMarker marker, UnityProject project)
        {
            return GetMarkerURI(marker.Id.ToString(), m_MarkerController.ProjectLinkSource.BaseURI);
        }


        public string GetMarkerURI(string markerId, Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new Exception("No BaseURI available");
            }

            var queryArgs = GetQueryArgs(baseUri);
            queryArgs[m_MarkerController.ProjectLinkSource.Key] = markerId;

            var response = new UriBuilder(baseUri);
            response.Query = ToQueryString(queryArgs);
            return response.Uri.ToString();
        }



        public static Dictionary<string, string> GetQueryArgs(Uri uri)
        {
            var queryArgs = new Dictionary<string, string>();
            if (uri.Query.Length > 1)
            {
                var keyValuePairs = uri.Query.Substring(1).Split('&');
                foreach (var keyValuePair in keyValuePairs)
                {
                    var splitStr = keyValuePair.Split('=');
                    queryArgs.Add(splitStr[0], splitStr.Length > 1 ? splitStr[1]: string.Empty);
                }
            }

            return queryArgs;
        }


        public static string ToQueryString(Dictionary<string, string> args)
        {
            List<string> items = new List<string>();
            foreach (var item in args)
            {
                items.Add(
                    Uri.EscapeDataString(item.Key)
                    + "=" +
                    Uri.EscapeDataString(item.Value)
                );
            }

            string response = "";
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                    response += "&";
                response += items[i];
            }

            return response;
        }

    }
}
