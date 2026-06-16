using System;
using Orbiters.XRayGizmos;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Orbiters.XRayGizmos.Editor
{
    [InitializeOnLoad]
    public static class XRayBonePickingService
    {
        private const string EnabledKey = "Orbiters.XRayGizmos.BonePicking.Enabled";
        private const float PickRadiusPixels = 12f;

        private static XRayBoneSegment hoveredSegment;
        private static double lastRepaintTime;

        public static event Action Changed;

        static XRayBonePickingService()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            XRayGizmoService.Changed += RepaintSceneViews;
            AssemblyReloadEvents.beforeAssemblyReload += ClearHover;
            EditorApplication.quitting += ClearHover;
        }

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, false);
            private set => EditorPrefs.SetBool(EnabledKey, value);
        }

        public static Transform HoveredBone => hoveredSegment.IsValid ? hoveredSegment.Bone : null;

        public static void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
            {
                return;
            }

            Enabled = enabled;
            if (!enabled)
            {
                ClearHover();
            }

            RepaintSceneViews();
            Changed?.Invoke();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (sceneView != null)
            {
                sceneView.wantsMouseMove = Enabled;
            }

            if (!Enabled || !XRayGizmoService.Enabled)
            {
                if (hoveredSegment.IsValid)
                {
                    ClearHover();
                }
                return;
            }

            var evt = Event.current;
            if (evt == null)
            {
                return;
            }

            UpdateHover(evt.mousePosition);

            if (evt.type == EventType.MouseDown &&
                evt.button == 0 &&
                !evt.alt &&
                hoveredSegment.IsValid)
            {
                SelectHoveredBone();
                evt.Use();
                return;
            }

            if (evt.type == EventType.Repaint && hoveredSegment.IsValid)
            {
                DrawHoverHighlight();
            }
        }

        private static void UpdateHover(Vector2 mousePosition)
        {
            var next = FindHoveredSegment(mousePosition);
            if (SameSegment(hoveredSegment, next))
            {
                return;
            }

            hoveredSegment = next;
            RepaintSceneViewsThrottled();
            Changed?.Invoke();
        }

        private static XRayBoneSegment FindHoveredSegment(Vector2 mousePosition)
        {
            XRayBoneSegment best = default;
            float bestDistance = PickRadiusPixels;

            foreach (var segment in XRayGizmoService.ActiveBoneSegments)
            {
                if (!segment.IsValid)
                {
                    continue;
                }

                if (!TryGetSegmentGuiPoints(segment, out var a, out var b))
                {
                    continue;
                }

                float distance = DistanceToSegment(mousePosition, a, b);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = segment;
                }
            }

            return best;
        }

        private static bool TryGetSegmentGuiPoints(XRayBoneSegment segment, out Vector2 a, out Vector2 b)
        {
            a = default;
            b = default;

            var camera = Camera.current;
            if (camera == null)
            {
                return false;
            }

            Vector3 head = segment.Bone.position;
            Vector3 tail = segment.Child.position;
            if ((tail - head).sqrMagnitude < 0.000001f)
            {
                return false;
            }

            if (camera.WorldToViewportPoint(head).z <= 0f &&
                camera.WorldToViewportPoint(tail).z <= 0f)
            {
                return false;
            }

            a = HandleUtility.WorldToGUIPoint(head);
            b = HandleUtility.WorldToGUIPoint(tail);
            return true;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.0001f)
            {
                return Vector2.Distance(point, a);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq);
            return Vector2.Distance(point, a + ab * t);
        }

        private static void DrawHoverHighlight()
        {
            if (!hoveredSegment.IsValid)
            {
                return;
            }

            var previousZTest = Handles.zTest;
            try
            {
                using (new Handles.DrawingScope(Color.white))
                {
                    Handles.zTest = CompareFunction.Always;
                    Handles.DrawAAPolyLine(8f, hoveredSegment.Bone.position, hoveredSegment.Child.position);

                    float handleSize = HandleUtility.GetHandleSize(hoveredSegment.Bone.position) * 0.025f;
                    Handles.SphereHandleCap(
                        0,
                        hoveredSegment.Bone.position,
                        Quaternion.identity,
                        handleSize,
                        EventType.Repaint);
                }
            }
            finally
            {
                Handles.zTest = previousZTest;
            }
        }

        private static void SelectHoveredBone()
        {
            if (!hoveredSegment.IsValid)
            {
                return;
            }

            Selection.activeTransform = hoveredSegment.Bone;
            EditorGUIUtility.PingObject(hoveredSegment.Bone);
            SceneView.RepaintAll();
        }

        private static bool SameSegment(XRayBoneSegment a, XRayBoneSegment b)
        {
            return a.Bone == b.Bone && a.Child == b.Child;
        }

        private static void ClearHover()
        {
            hoveredSegment = default;
            RepaintSceneViews();
            Changed?.Invoke();
        }

        private static void RepaintSceneViews()
        {
            SceneView.RepaintAll();
        }

        private static void RepaintSceneViewsThrottled()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - lastRepaintTime < 0.025d)
            {
                return;
            }

            lastRepaintTime = now;
            SceneView.RepaintAll();
        }
    }
}
