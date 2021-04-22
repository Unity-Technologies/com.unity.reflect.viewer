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

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using System;
using System.Collections;
namespace VivoxUnity
{  
    public class VxUnityInterop : MonoBehaviour
    {
        private static object m_Lock = new object();
        private bool quitting = false;
        private static VxUnityInterop m_Instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static VxUnityInterop Instance
        {
            get
            {
                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        // Search for existing instance.
                        m_Instance = FindObjectOfType<VxUnityInterop>();

                        // Create new instance if one doesn't already exist.
                        if (m_Instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            m_Instance = singletonObject.AddComponent<VxUnityInterop>();
                            singletonObject.name = typeof(VxUnityInterop).ToString() + " (Singleton)";
                        }
                    }
                    // Make instance persistent even if its already in the scene
                    DontDestroyOnLoad(m_Instance.gameObject);
                    return m_Instance;
                }
            }
           
        }

        void OnApplicationQuit()
        {
            quitting = true;
        }

        // Setting up Unity Coroutine to run on the main thread 
        public virtual void StartVivoxUnity()
        {
            StartCoroutine(VivoxUnityRun());
        }

        private IEnumerator VivoxUnityRun()
        {
            while (VxClient.Instance.Started)
            {
                try
                {
                    Client.RunOnce();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error: " + e.Message);
                }
                yield return new WaitForSeconds(0.01f);
            }
        }
        void OnDestroy()
        {
            if (!quitting)
            {
                var classType = GetType();
                Debug.LogError(classType.Namespace + " requrires " + classType.Name + " to communicate messages to and from "
                    + classType.Namespace + " Core. Deleting this object will prevent the " + classType.Namespace +" SDK from working.  " +
                    "If you would like to change it's implementation please override StartVivoxUnity method in "
                    + classType.Name);
            }
        }
    }
}
#endif