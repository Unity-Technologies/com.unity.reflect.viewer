using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{
    public class ColliderManager: MonoBehaviour
    {
        [SerializeField]
        float m_DetectionRange = 4;
        [SerializeField]
        float m_FrenquencyFloorCheck = 0.03f;
        float m_Time;
        List<GameObject> m_ObjectsToAdd = new List<GameObject>();
        List<GameObject> m_ObjectsToRemove = new List<GameObject>();

        List<MeshRenderer> m_MeshRenderers = new List<MeshRenderer>();

        Dictionary<GameObject, List<MeshCollider>> m_ColliderCache = new Dictionary<GameObject, List<MeshCollider>>();
        Dictionary<GameObject, Metadata> m_MetadataCache = new Dictionary<GameObject, Metadata>();
        List<Tuple<GameObject, RaycastHit>> m_SpatialObjects = new List<Tuple<GameObject, RaycastHit>>();

        const string k_Category = "Category";
        const string k_Doors = "Doors";
        Rigidbody m_Rigidbody;
        IUISelector<SpatialSelector> m_TeleportPickerSelector;
        IUISelector<SetInstructionUIStateAction.InstructionUIState> m_WalkInstructionStateGetter;

        void Awake()
        {
            m_Rigidbody = GetComponentInParent<Rigidbody>();
            m_TeleportPickerSelector = UISelectorFactory.createSelector<SpatialSelector>(ProjectContext.current, nameof(ITeleportDataProvider.teleportPicker));
            m_WalkInstructionStateGetter = UISelectorFactory.createSelector<SetInstructionUIStateAction.InstructionUIState>(WalkModeContext.current, nameof(IWalkModeDataProvider.instructionUIState));
        }

        void OnDestroy()
        {
            m_TeleportPickerSelector?.Dispose();
        }

        void OnEnable()
        {
            m_Rigidbody.useGravity = false;
        }

        void Update()
        {
            if (m_TeleportPickerSelector.GetValue() == null)
                return;

            GetSurroundingAsyncCollider();
            RemoveCollider();
            AddCollider();

            if (m_Time < m_FrenquencyFloorCheck)
            {
                m_Time += Time.deltaTime;
            }
            else
            {
                GetFloorAsyncCollider();
                m_Time = 0;
            }
        }

        void RemoveCollider()
        {
            if (m_ColliderCache.Count == 0)
                return;

            // Remove the old collider from the gameobject that are far from the character
            m_ObjectsToRemove.Clear();
            m_ObjectsToRemove.AddRange(m_ColliderCache.Keys.Except(m_ObjectsToAdd));
            foreach (var go in m_ObjectsToRemove)
            {
                if (!go)
                    continue;

                foreach (var meshCollider in m_ColliderCache[go])
                    Destroy(meshCollider);

                m_ColliderCache.Remove(go);
            }

            AddCollider();
        }

        void AddCollider()
        {
            // Add the new element close to the player to the cash and add the collider except for the door
            foreach (var go in m_ObjectsToAdd)
            {
                // Check if the gameobject already have a collider
                if (go == null || m_ColliderCache.ContainsKey(go))
                    continue;

                // Get the metadata to know which type of object it is
                if (!m_MetadataCache.TryGetValue(go, out var metadata))
                {
                    metadata = go.GetComponent<Metadata>();
                    m_MetadataCache.Add(go, metadata);
                }

                // Check if we have a door
                if (metadata != null && metadata.GetParameters().TryGetValue(k_Category, out var parameter) && parameter.value.Contains(k_Doors))
                    continue;

                // add new list to cache since we know it doesn't already exist due to previous check
                // even if we don't add any colliders this will prevent the obj from being processed next time
                var colliderList = new List<MeshCollider>();
                m_ColliderCache.Add(go, colliderList);

                m_MeshRenderers.Clear();
                go.GetComponentsInChildren(m_MeshRenderers);
                foreach (var meshRenderer in m_MeshRenderers)
                {
                    if (meshRenderer.gameObject.GetComponent<Collider>() != null)
                        continue;

                    colliderList.Add(meshRenderer.gameObject.AddComponent<MeshCollider>());
                }
            }
        }

        void GetSurroundingAsyncCollider()
        {
            m_TeleportPickerSelector.GetValue().Pick(transform.position, m_DetectionRange, ProcessSpatialPickerResult);
        }

        void GetFloorAsyncCollider()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            m_TeleportPickerSelector.GetValue().FloorPick(ray, result =>
            {
                m_Rigidbody.useGravity = result.Count > 0;
            });
        }

        void ProcessSpatialPickerResult(List<Tuple<GameObject, RaycastHit>> list)
        {
            foreach (var go in list)
                m_ObjectsToAdd.Add(go.Item1);
        }

        void OnDisable()
        {
            Clean();
        }

        void Clean()
        {
            foreach (var go in m_ColliderCache)
            {
                if (!go.Key)
                    continue;

                foreach (var c in go.Value)
                    Destroy(c);
            }

            m_TeleportPickerSelector.GetValue().CleanCache();
            m_ColliderCache.Clear();
            m_MetadataCache.Clear();
        }
    }
}
