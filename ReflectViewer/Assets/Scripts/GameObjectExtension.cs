using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;

namespace UnityEngine.Reflect
{
    public static class GameObjectExtension
    {
        const float s_Tolerance = 1e-5f;

        public static Bounds? CalculateBoundsInChildren(this GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            Bounds? b = null;
            if (renderers != null)
            {
                b = renderers[0].bounds;
                if (renderers.Length > 1)
                {
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        b.Value.Encapsulate(renderers[i].bounds);
                    }
                }
            }
            return b;
        }


        public static GameObject CloneMeshObject(this GameObject obj, string name, Material defaultMaterial = null)
        {
            var meshObject = new GameObject(name);
            meshObject.transform.position = obj.transform.position;
            meshObject.transform.rotation = obj.transform.rotation;
            meshObject.transform.localScale = obj.transform.localScale;
            var meshFilter = meshObject.AddComponent<MeshFilter>();

            var orgMeshRenderer = obj.GetComponent<MeshRenderer>();
            var meshRenderer = meshObject.AddComponent<MeshRenderer>();
            if (orgMeshRenderer.materials.Length > 1)
            {
                var materials = new Material[orgMeshRenderer.materials.Length];
                for (int i = 0; i < orgMeshRenderer.materials.Length; i++)
                {
                    if (defaultMaterial != null)
                    {
                        materials[i] = defaultMaterial;
                    }
                    else
                    {
                        materials[i] = orgMeshRenderer.materials[i];
                    }
                }
                meshRenderer.materials = materials;
                meshRenderer.sharedMaterials = materials;
            }
            else
            {
                meshRenderer.material = defaultMaterial;
                meshRenderer.sharedMaterial = defaultMaterial;
            }
            meshFilter.sharedMesh = GameObject.Instantiate(obj.GetComponent<MeshFilter>().sharedMesh);
            meshFilter.sharedMesh.RecalculateBounds();

            return meshObject;
        }

        public static GameObject ClonePlaneSurface(this GameObject obj, string name, Material defaultMaterial = null)
        {
            PlaneSelectionContext planeSelectionContext = obj.GetComponent<PlaneSelectionContext>();
            var context = planeSelectionContext.LastContext;
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null | planeSelectionContext == null)
            {
                return null;
            }
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            var newMesh = GameObject.Instantiate(mesh);
            var wall = context.SelectedPlane;
            for (var subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                var subMeshTriangles = mesh.GetTriangles(subMeshIndex);
                var newTriangles = new List<int>();
                for (var index = 0; index < subMeshTriangles.Length/3; index++)
                {
                    var coplanar = true;
                    for (var pointIndex = 0; pointIndex < 3; pointIndex++)
                    {
                        Vector3 point = vertices[subMeshTriangles[index * 3 + pointIndex]];
                        point = obj.transform.TransformPoint(point);
                        if (Mathf.Abs(wall.GetDistanceToPoint(point)) > s_Tolerance)
                        {
                            coplanar = false;
                            break;
                        }
                    }

                    if (coplanar)
                    {
                        for (var pointIndex = 0; pointIndex < 3; pointIndex++)
                        {
                            newTriangles.Add(subMeshTriangles[index * 3 + pointIndex]);
                        }
                    }
                }
                newMesh.SetTriangles(newTriangles.ToArray(), subMeshIndex);
            }

            // clone mesh object
            meshFilter.sharedMesh = newMesh;
            meshFilter.sharedMesh.RecalculateBounds();
            var clone = obj.CloneMeshObject(name, defaultMaterial);
            // add a planeSelectionContext
            var newPlaneContext = clone.AddComponent<PlaneSelectionContext>();
            newPlaneContext.SelectionContextList.Add( new PlaneSelectionContext.SelectionContext
            {
                HitPoint = context.HitPoint,
                SelectedPlane = context.SelectedPlane
            });
            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.RecalculateBounds();

            return clone;
        }
    }
}
