using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Orbiters.XRayGizmos.Editor.Tests
{
    public static class XRayEdgeSkinningTests
    {
        public static void RunOrThrow()
        {
            var selection = Selection.objects;
            bool enabled = XRayMeshEdgeService.Enabled;
            var scene = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("Edge skinning fixture");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
            var mesh = new Mesh();
            var sourceBake = new Mesh();
            var edgeBake = new Mesh();
            try
            {
                var a = new GameObject("A").transform; a.SetParent(root.transform, false);
                var b = new GameObject("B").transform; b.SetParent(root.transform, false); b.localPosition = Vector3.right * 2;
                mesh.vertices = new[] { Vector3.zero, Vector3.up, Vector3.right };
                mesh.triangles = new[] { 0, 1, 2 };
                mesh.bindposes = new[] { Matrix4x4.identity, Matrix4x4.identity };
                mesh.boneWeights = Enumerable.Repeat(new BoneWeight { boneIndex0 = 0, weight0 = 1 }, 3).ToArray();
                var source = root.AddComponent<SkinnedMeshRenderer>();
                source.sharedMesh = mesh; source.bones = new[] { a, b }; source.rootBone = a;
                Selection.activeGameObject = root;
                XRayMeshEdgeService.SetEnabled(true); XRayMeshEdgeService.Refresh();
                var overlay = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r != source);
                var edgeMesh = overlay.sharedMesh;
                Action verify = () =>
                {
                    if (overlay == null || overlay.sharedMesh != edgeMesh) throw new InvalidOperationException("Skinning sync rebuilt edge geometry.");
                    if (!overlay.bones.SequenceEqual(source.bones) || overlay.rootBone != source.rootBone || overlay.quality != source.quality)
                        throw new InvalidOperationException($"Edge skinning is stale: bones={overlay.bones.SequenceEqual(source.bones)}, root={overlay.rootBone == source.rootBone}, quality={overlay.quality}/{source.quality}.");
                    source.BakeMesh(sourceBake); overlay.BakeMesh(edgeBake);
                    for (int i = 0; i < 3; i++)
                        if ((sourceBake.vertices[i] - edgeBake.vertices[i]).sqrMagnitude > 0.00000001f)
                            throw new InvalidOperationException("Baked edge vertices differ from source.");
                };
                verify();
                source.bones = new[] { b, a }; source.rootBone = b;
                XRayMeshEdgeService.Refresh(); verify();
                source.bones = new[] { a, a }; source.rootBone = a;
                source.quality = SkinQuality.Bone1;
                Sync(); verify();
                source.rootBone = b; Sync(); verify();
                a.localPosition = Vector3.up * 3; Sync(); verify();
            }
            finally
            {
                XRayMeshEdgeService.SetEnabled(false);
                Selection.objects = selection;
                Object.DestroyImmediate(root); Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(sourceBake); Object.DestroyImmediate(edgeBake);
                EditorSceneManager.ClosePreviewScene(scene);
                XRayMeshEdgeService.SetEnabled(enabled);
            }
        }

        private static void Sync()
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            typeof(XRayMeshEdgeService).GetField("nextBlendShapeSyncTime", flags).SetValue(null, 0d);
            typeof(XRayMeshEdgeService).GetMethod("SyncBlendShapeDrivenInstances", flags).Invoke(null, null);
        }
    }
}
