using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Orbiters.XRayGizmos.Editor
{
    internal static class XRayGizmoTargetFinder
    {
        public static List<XRayArmatureTarget> FindSceneTargets()
        {
            var targetsByArmature = new Dictionary<int, XRayArmatureTarget>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (var root in scene.GetRootGameObjects())
                {
                    AddTargets(root, targetsByArmature);
                }
            }

            var targets = new List<XRayArmatureTarget>(targetsByArmature.Values);
            targets.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));
            return targets;
        }

        public static bool TryResolveTarget(GameObject selected, out XRayArmatureTarget target)
        {
            target = default;
            if (selected == null)
            {
                return false;
            }

            var current = selected.transform;
            while (current != null)
            {
                if (TryFindBestTarget(current.gameObject, out target))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        public static bool TryBuildTarget(SkinnedMeshRenderer renderer, out XRayArmatureTarget target)
        {
            target = default;
            if (!XRayArmatureMeshGenerator.IsUsableRenderer(renderer))
            {
                return false;
            }

            var armatureRoot = ResolveArmatureRoot(renderer);
            if (armatureRoot == null)
            {
                return false;
            }

            var owner = ResolveOwner(renderer, armatureRoot);
            if (owner == null)
            {
                return false;
            }

            int boneCount = renderer.bones != null ? renderer.bones.Length : 0;
            int vertexCount = renderer.sharedMesh != null ? renderer.sharedMesh.vertexCount : 0;
            int score = boneCount * 1000000 + vertexCount;
            target = new XRayArmatureTarget(owner, renderer, armatureRoot, score);
            return true;
        }

        private static void AddTargets(GameObject root, Dictionary<int, XRayArmatureTarget> targetsByArmature)
        {
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!TryBuildTarget(renderer, out var target))
                {
                    continue;
                }

                if (!targetsByArmature.TryGetValue(target.ArmatureKey, out var existing) ||
                    target.Score > existing.Score)
                {
                    targetsByArmature[target.ArmatureKey] = target;
                }
            }
        }

        private static bool TryFindBestTarget(GameObject root, out XRayArmatureTarget target)
        {
            target = default;
            int bestScore = -1;

            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!TryBuildTarget(renderer, out var candidate))
                {
                    continue;
                }

                if (candidate.Score > bestScore)
                {
                    bestScore = candidate.Score;
                    target = candidate;
                }
            }

            return bestScore >= 0;
        }

        private static Transform ResolveArmatureRoot(SkinnedMeshRenderer renderer)
        {
            if (renderer == null || renderer.rootBone == null)
            {
                return null;
            }

            var current = renderer.rootBone;
            while (current != null)
            {
                if (string.Equals(current.name, "Armature", System.StringComparison.OrdinalIgnoreCase))
                {
                    return current;
                }

                current = current.parent;
            }

            return renderer.rootBone;
        }

        private static GameObject ResolveOwner(SkinnedMeshRenderer renderer, Transform armatureRoot)
        {
            if (armatureRoot != null && armatureRoot.parent != null)
            {
                return armatureRoot.parent.gameObject;
            }

            return renderer != null ? renderer.gameObject : null;
        }
    }

    internal readonly struct XRayArmatureTarget
    {
        public readonly GameObject Owner;
        public readonly SkinnedMeshRenderer Renderer;
        public readonly Transform ArmatureRoot;
        public readonly int ArmatureKey;
        public readonly int Score;

        public XRayArmatureTarget(GameObject owner, SkinnedMeshRenderer renderer, Transform armatureRoot, int score)
        {
            Owner = owner;
            Renderer = renderer;
            ArmatureRoot = armatureRoot;
            ArmatureKey = armatureRoot != null ? armatureRoot.GetInstanceID() : 0;
            Score = score;
        }

        public string DisplayName => Owner != null ? Owner.name : "Armature";

        public bool IsValid => Owner != null &&
                               Renderer != null &&
                               ArmatureRoot != null &&
                               ArmatureKey != 0;
    }
}
