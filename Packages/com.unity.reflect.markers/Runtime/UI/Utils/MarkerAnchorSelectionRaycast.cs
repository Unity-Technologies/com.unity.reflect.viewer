using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Placement;
using UnityEngine;

namespace Unity.Reflect.Markers.UI
{
    public static class MarkerAnchorSelectionRaycast
    {
        public static void PostRaycast(List<Tuple<GameObject, RaycastHit>> results)
        {
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
                var point = hit.point;
                var normal = hit.normal;
                // Vertex snapping requires a readable mesh
                if (mesh.isReadable)
                {
                    int[] triangles = mesh.triangles;
                    Vector3[] normals = mesh.normals;
                    Vector3[] vertices = mesh.vertices;
                    List<Vector3> triangleVertices = new List<Vector3> { vertices[triangles[hit.triangleIndex * 3 + 0]], vertices[triangles[hit.triangleIndex * 3 + 1]], vertices[triangles[hit.triangleIndex * 3 + 2]] };

                    Vector3 N0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                    Vector3 N1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                    Vector3 N2 = normals[triangles[hit.triangleIndex * 3 + 2]];
                    normal = meshCollider.transform.rotation * ((N0 + N1 + N2) / 3.0f );

                    point = DetectBestPointSelection(hit.point, triangleVertices);
                }

                SelectObjectDragToolAction.IAnchor anchor = new MarkerAnchor(tuple.Item1.GetInstanceID(), point, normal);

                //TODO This will keep adding AnchorSelectionContext on every selected object, need to remove them at some point
                var selectionContext = tuple.Item1.GetComponent<MarkerAnchorSelectionContext>();
                if (selectionContext == null)
                    selectionContext = tuple.Item1.AddComponent<MarkerAnchorSelectionContext>();

                //TODO This will keep adding SelectionContext every time there is a selection, the list needs to be cleared up at some point or moved somewhere else
                selectionContext.SelectionContextList.Add(new MarkerAnchorSelectionContext.SelectionContext { selectedAnchor = anchor });
            }

            foreach (var tuple in resultsToRemove)
                results.Remove(tuple);

            results.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));
        }

        // Basic Vertices Snapping
        static Vector3 DetectBestPointSelection(Vector3 hitPoint, List<Vector3> triangleVertices)
        {
            triangleVertices.Sort((x, y) => { return (hitPoint - x).sqrMagnitude.CompareTo((hitPoint - y).sqrMagnitude); });

            if (Vector3.Distance(triangleVertices[0], hitPoint) < 0.1f)
            {
                return triangleVertices[0];
            }

            return hitPoint;
        }
    }
}
