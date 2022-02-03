#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using Unity.Serialization.Json;
using UnityEngine;

namespace Unity.Properties
{
    [ExecuteInEditMode]
    public class PropertyBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector, DontCreateProperty]
        string m_Data;

        void OnEnable()
        {
            Load();
        }

        public void Save()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, GetType().Name);
#endif
            m_Data = JsonSerialization.ToJson(this, new JsonSerializationParameters {DisableRootAdapters = true});
        }

        public void Load()
        {
            if (string.IsNullOrEmpty(m_Data))
                return;

            var self = this;

            if (!JsonSerialization.TryFromJsonOverride(m_Data, ref self, out var events,
                new JsonSerializationParameters {DisableRootAdapters = true}))
            {
                foreach (var exception in events.Exceptions)
                {
                    Debug.LogException((Exception) exception.Payload);
                }

                foreach (var warnings in events.Warnings)
                {
                    Debug.LogWarning(warnings.Payload);
                }

                foreach (var logs in events.Logs)
                {
                    Debug.Log(logs.Payload);
                }
            }
        }
    }
}
