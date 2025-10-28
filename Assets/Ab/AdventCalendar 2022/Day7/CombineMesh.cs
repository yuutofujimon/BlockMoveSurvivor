using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CombineMesh
{
    [MenuItem("GameObject/メッシュ結合", false, 40)]
    public static void Build(MenuCommand command)
    {
        if (command.context != Selection.activeObject) return;
        var mesh = new Mesh();
        mesh.name = "CombinedMesh";

        List<Vector3> verts = new List<Vector3>();
        List<List<int>> triangles = new List<List<int>>();
        List<Color> colors = new List<Color>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<BoneWeight> weights = new List<BoneWeight>();
        List<Transform> bones = new List<Transform>();
        var go = new GameObject("Combined GameObject");
        var root = go.transform;

        var materials = new List<Material>();
        foreach (var group in Selection.objects.OfType<GameObject>()
                     .GroupBy(g => g.GetComponent<MeshRenderer>().sharedMaterial))
        {
            materials.Add(group.Key);
            var tri = new List<int>();
            foreach (var oo in group)
            {
                AddMesh(verts, tri, colors, normals, uvs, weights, oo, bones.Count);

                var g = new GameObject();
                g.name = "[Bone]" + oo.name;
                g.transform.SetParent(root, false);
                g.transform.localPosition = oo.transform.localPosition;
                g.transform.localScale = oo.transform.localScale;
                g.transform.localRotation = oo.transform.localRotation;
                bones.Add(g.transform);
            }

            triangles.Add(tri);
        }

        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.subMeshCount = triangles.Count;
        for (var i = 0; i < triangles.Count; i++)
        {
            mesh.SetTriangles(triangles[i], i);
        }

        mesh.SetColors(colors);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.boneWeights = weights.ToArray();
        mesh.UploadMeshData(false);
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.bindposes = bones.Select((bone) => bone.worldToLocalMatrix * root.localToWorldMatrix).ToArray();
        var meshRenderer = go.AddComponent<SkinnedMeshRenderer>();
        meshRenderer.bones = bones.ToArray();
        meshRenderer.rootBone = root;
        meshRenderer.sharedMesh = mesh;
        meshRenderer.sharedMaterials = materials.ToArray();

        var assetPath = EditorUtility.SaveFilePanelInProject("Save", "Combined.prefab", "prefab", "save");
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, assetPath, InteractionMode.UserAction);
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (!AssetDatabase.IsMainAsset(o))
            {
                if (o is Mesh m && m.name == "CombinedMesh")
                {
                    AssetDatabase.RemoveObjectFromAsset(o);
                }
            }
        }

        AssetDatabase.AddObjectToAsset(mesh, assetPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, assetPath, InteractionMode.UserAction);

        foreach (var g in Selection.objects.OfType<GameObject>())
        {
            g.SetActive(false);
        }
    }

    private static void AddMesh(
        List<Vector3> verts, List<int> triangles, List<Color> colors, List<Vector3> normals, List<Vector2> uvs,
        List<BoneWeight> weights, GameObject go, int n)
    {
        var meshFilter = go.GetComponent<MeshFilter>();
        var sm = meshFilter.sharedMesh;
        var originalVerts = sm.vertices;
        var originalColors = sm.colors;
        var originalNormals = sm.normals;
        var originalTriangles = sm.triangles;
        var originalUvs = sm.uv;
        var startIndex = verts.Count;
        for (var i = 0; i < originalVerts.Length; i++)
        {
            verts.Add(go.transform.localRotation * Vector3.Scale(originalVerts[i], go.transform.localScale) +
                      go.transform.localPosition);
            weights.Add(new BoneWeight() { weight0 = 1, boneIndex0 = n });
        }

        for (var i = 0; i < originalColors.Length; i++)
        {
            colors.Add(originalColors[i]);
        }

        for (var i = 0; i < originalNormals.Length; i++)
        {
            normals.Add(go.transform.localRotation * originalNormals[i]);
        }

        for (var i = 0; i < originalTriangles.Length; i++)
        {
            triangles.Add(startIndex + originalTriangles[i]);
        }

        for (var i = 0; i < originalUvs.Length; i++)
        {
            uvs.Add(originalUvs[i]);
        }
    }
}