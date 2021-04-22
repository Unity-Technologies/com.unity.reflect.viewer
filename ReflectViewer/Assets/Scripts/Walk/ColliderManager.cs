using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace UnityEngine.Reflect.Viewer
{
    public class ColliderManager : MonoBehaviour
    {
        public float detectionRange = 2;

        List<GameObject> m_ObjectsToAdd = new List<GameObject>();
        List<GameObject> m_ObjectsToRemove = new List<GameObject>();
        List<MeshRenderer> m_MeshRenderers = new List<MeshRenderer>();
        Dictionary<GameObject, List<MeshCollider>> m_ColliderCache = new Dictionary<GameObject, List<MeshCollider>>();
        Dictionary<GameObject, Metadata> m_MetadataCache = new Dictionary<GameObject, Metadata>();
        SpatialSelector m_SpatialSelector;
        List<Tuple<GameObject, RaycastHit>> m_SpatialObjects = new List<Tuple<GameObject, RaycastHit>>();

        const string k_Category = "Category";
        const string k_Doors = "Doors";

        void OnEnable()
        {
            m_SpatialSelector = UIStateManager.current.projectStateData.teleportPicker as SpatialSelector;
        }

        void FixedUpdate()
        {
            if (m_SpatialSelector == null)
                return;

            GetSurroundingCollider();
            RemoveCollider();
            AddCollider();
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
        }

        void AddCollider()
        {
            // Add the new element close to the player to the cash and add the collider except for the door
            foreach (var go in m_ObjectsToAdd)
            {
                // Check if the gameobject already have a collider
                if (go == null || m_ColliderCache.ContainsKey(go))
                    continue;

                // Get the metada to know which type of object it is
                if (!m_MetadataCache.TryGetValue(go, out var metadata))
                {
                    metadata = go.GetComponent<Metadata>();
                    m_MetadataCache.Add(go, metadata);
                }

                // Check if we have it's a door
                if (metadata.GetParameters().TryGetValue(k_Category, out var parameter) && parameter.value.Contains(k_Doors))
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

        void GetSurroundingCollider()
        {
            m_SpatialObjects.Clear();
            m_SpatialSelector.Pick(detectionRange, m_SpatialObjects, transform);

            m_ObjectsToAdd.Clear();
            foreach (var go in m_SpatialObjects)
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

            m_ColliderCache.Clear();
            m_MetadataCache.Clear();
        }
    }
}
