using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MARS.Data;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace UnityEngine.Reflect.Viewer
{
    public class SpatialOrientedPlaneSelector : SpatialSelector
    {
        public MarsPlaneAlignment Orientation { get; set; }

        public SpatialOrientedPlaneSelector()
        {
            Orientation = MarsPlaneAlignment.Vertical;
        }

        protected override void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
            // remove results of faces that are not vertical
            var resultsToRemove = new List<Tuple<GameObject, RaycastHit>>();
            foreach (var tuple in results)
            {
                var hit = tuple.Item2;
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                {
                    resultsToRemove.Add(tuple);
                    continue;
                }
                Mesh mesh = meshCollider.sharedMesh;
                int[] triangles = mesh.triangles;
                Vector3[] normals = mesh.normals;
                Vector3[] vertices = mesh.vertices;
                Vector3 N0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                Vector3 N1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                Vector3 N2 = normals[triangles[hit.triangleIndex * 3 + 2]];
                Vector3 normal = (N0 + N1 + N2) / 3.0f;
                var angle = Vector3.Dot(normal, Vector3.up);
                switch (Orientation)
                {
                    case MarsPlaneAlignment.Vertical:
                    {
                        if (!Mathf.Approximately(angle, 0.0f))
                        {
                            resultsToRemove.Add(tuple);
                            continue;
                        }
                        break;
                    }

                    case MarsPlaneAlignment.HorizontalUp:
                    {
                        if (!Mathf.Approximately(angle, 1.0f))
                        {
                            resultsToRemove.Add(tuple);
                            continue;
                        }
                        break;
                    }

                    default:
                    {
                        throw new NotImplementedException();
                    }
                }

                // keeper, add PlaneSelectionContext
                Plane wall = new Plane();
                var worldTransform = new GameObject().transform;
                worldTransform.position = tuple.Item1.gameObject.transform.position;
                worldTransform.rotation = tuple.Item1.gameObject.transform.rotation;
                worldTransform.localScale = tuple.Item1.gameObject.transform.lossyScale;

                Vector3 P0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                var worldNormal = worldTransform.TransformDirection(normal);
                var worldPoint = worldTransform.TransformPoint(P0);
                // dispose of unused gameobject
                Object.Destroy(worldTransform.gameObject);
                wall.SetNormalAndPosition(worldNormal, worldPoint);

                //TODO This will keep adding PlaneSelectionContext on every selected object, need to remove them at some point
                var selectionContext =  tuple.Item1.GetComponent<PlaneSelectionContext>();
                if (selectionContext == null)
                    selectionContext = tuple.Item1.AddComponent<PlaneSelectionContext>();
                //TODO This will keep adding SelectionContext every time there is a selection, the list needs to be cleared up at some point or moved somewhere else
                selectionContext.SelectionContextList.Add(new PlaneSelectionContext.SelectionContext { SelectedPlane = wall, HitPoint = hit.point });
            }

            // Remove any non vertical object from results
            foreach (var tuple in resultsToRemove)
                results.Remove(tuple);

            results.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));

            foreach (var c in m_ColliderCache.Values)
                Object.Destroy(c);

            m_ColliderCache.Clear();
        }
    }
}
