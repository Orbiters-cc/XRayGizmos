using Orbiters.XRayGizmos;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Orbiters.XRayGizmos.Editor
{
    [Overlay(
        typeof(SceneView),
        "Orbiters/XRayGizmos/SceneToolbar",
        "XRay Gizmos",
        true,
        defaultDockPosition = DockPosition.Top,
        defaultDockZone = DockZone.TopToolbar)]
    [Icon(XRayGizmosSceneToolbarIcons.BonesIconPath)]
    internal sealed class XRayGizmosSceneToolbarOverlay : ToolbarOverlay
    {
        public XRayGizmosSceneToolbarOverlay()
            : base(
                XRayBonesToolbarToggle.Id,
                XRayWeightPaintToolbarToggle.Id,
                XRayMeshEdgesToolbarToggle.Id)
        {
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    internal sealed class XRayBonesToolbarToggle : EditorToolbarDropdownToggle
    {
        public const string Id = "Orbiters/XRayGizmos/Bones";

        private bool isSyncing;
        private bool isListening;

        public XRayBonesToolbarToggle()
        {
            icon = XRayGizmosSceneToolbarIcons.LoadBonesIcon();
            this.RegisterValueChangedCallback(OnValueChanged);
            RegisterCallback<AttachToPanelEvent>(_ => Attach());
            RegisterCallback<DetachFromPanelEvent>(_ => Detach());
            dropdownClicked += ShowModeMenu;
            Attach();
            SyncFromServices();
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (isSyncing)
            {
                return;
            }

            if (evt.newValue)
            {
                EnableBones(XRayGizmoTargetMode.WholeScene);
            }
            else
            {
                DisableBones();
            }
        }

        private void ShowModeMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Whole scene"),
                XRayGizmoService.Enabled && XRayGizmoService.TargetMode == XRayGizmoTargetMode.WholeScene,
                () => EnableBones(XRayGizmoTargetMode.WholeScene));
            menu.AddItem(
                new GUIContent("Selected object"),
                XRayGizmoService.Enabled && XRayGizmoService.TargetMode == XRayGizmoTargetMode.SelectedObject,
                () => EnableBones(XRayGizmoTargetMode.SelectedObject));
            menu.AddSeparator(string.Empty);

            if (XRayGizmoService.Enabled)
            {
                menu.AddItem(new GUIContent("Disable"), false, DisableBones);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Disable"));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Open XRay Gizmos Window"), false, XRayGizmosWindow.Open);
            menu.DropDown(worldBound);
        }

        private static void EnableBones(XRayGizmoTargetMode mode)
        {
            XRayGizmoService.SetTargetMode(mode);
            XRayBonePickingService.SetEnabled(true);
            XRayGizmoService.SetEnabled(true);
        }

        private static void DisableBones()
        {
            XRayBonePickingService.SetEnabled(false);
            XRayGizmoService.SetEnabled(false);
        }

        private void SyncFromServices()
        {
            isSyncing = true;
            SetValueWithoutNotify(XRayGizmoService.Enabled);
            isSyncing = false;
            tooltip = BuildTooltip();
        }

        private static string BuildTooltip()
        {
            string scope = XRayGizmoService.TargetMode == XRayGizmoTargetMode.SelectedObject
                ? "selected object"
                : "whole scene";
            string picking = XRayBonePickingService.Enabled ? "clickable" : "not clickable";
            return $"XRay bones ({scope}, {picking}). Click to show whole-scene clickable bones.";
        }

        private void Attach()
        {
            if (isListening)
            {
                return;
            }

            isListening = true;
            XRayGizmoService.Changed += SyncFromServices;
            XRayBonePickingService.Changed += SyncFromServices;
        }

        private void Detach()
        {
            if (!isListening)
            {
                return;
            }

            isListening = false;
            XRayGizmoService.Changed -= SyncFromServices;
            XRayBonePickingService.Changed -= SyncFromServices;
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    internal sealed class XRayWeightPaintToolbarToggle : EditorToolbarToggle
    {
        public const string Id = "Orbiters/XRayGizmos/WeightPaint";

        private bool isSyncing;
        private bool isListening;

        public XRayWeightPaintToolbarToggle()
        {
            icon = XRayGizmosSceneToolbarIcons.LoadWeightPaintIcon();
            this.RegisterValueChangedCallback(OnValueChanged);
            RegisterCallback<AttachToPanelEvent>(_ => Attach());
            RegisterCallback<DetachFromPanelEvent>(_ => Detach());
            Attach();
            SyncFromServices();
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (isSyncing)
            {
                return;
            }

            XRayWeightPaintService.SetEnabled(evt.newValue);
        }

        private void SyncFromServices()
        {
            isSyncing = true;
            SetValueWithoutNotify(XRayWeightPaintService.Enabled);
            isSyncing = false;
            tooltip = XRayWeightPaintService.Enabled
                ? XRayWeightPaintService.LastStatus ?? "Hide weight paint overlay."
                : "Show weight paint overlay for the selected bone or armature.";
        }

        private void Attach()
        {
            if (isListening)
            {
                return;
            }

            isListening = true;
            XRayWeightPaintService.Changed += SyncFromServices;
        }

        private void Detach()
        {
            if (!isListening)
            {
                return;
            }

            isListening = false;
            XRayWeightPaintService.Changed -= SyncFromServices;
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    internal sealed class XRayMeshEdgesToolbarToggle : EditorToolbarDropdownToggle
    {
        public const string Id = "Orbiters/XRayGizmos/MeshEdges";

        private bool isSyncing;
        private bool isListening;

        public XRayMeshEdgesToolbarToggle()
        {
            icon = XRayGizmosSceneToolbarIcons.LoadMeshEdgesIcon();
            this.RegisterValueChangedCallback(OnValueChanged);
            RegisterCallback<AttachToPanelEvent>(_ => Attach());
            RegisterCallback<DetachFromPanelEvent>(_ => Detach());
            dropdownClicked += ShowSettings;
            Attach();
            SyncFromServices();
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (isSyncing)
            {
                return;
            }

            XRayMeshEdgeService.SetEnabled(evt.newValue);
        }

        private void ShowSettings()
        {
            UnityEditor.PopupWindow.Show(worldBound, new XRayMeshEdgesPopupContent());
        }

        private void SyncFromServices()
        {
            isSyncing = true;
            SetValueWithoutNotify(XRayMeshEdgeService.Enabled);
            isSyncing = false;
            tooltip = XRayMeshEdgeService.Enabled
                ? XRayMeshEdgeService.LastStatus ?? "Hide mesh polygon edges."
                : "Show mesh polygon edges for selected skinned meshes.";
        }

        private void Attach()
        {
            if (isListening)
            {
                return;
            }

            isListening = true;
            XRayMeshEdgeService.Changed += SyncFromServices;
        }

        private void Detach()
        {
            if (!isListening)
            {
                return;
            }

            isListening = false;
            XRayMeshEdgeService.Changed -= SyncFromServices;
        }
    }

    internal sealed class XRayMeshEdgesPopupContent : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            return new Vector2(250f, 118f);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUILayout.Toggle("Show edges", XRayMeshEdgeService.Enabled);
            if (EditorGUI.EndChangeCheck())
            {
                XRayMeshEdgeService.SetEnabled(enabled);
            }

            EditorGUI.BeginChangeCheck();
            float alpha = EditorGUILayout.Slider("Opacity", XRayMeshEdgeService.EdgeAlpha, 0.05f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                XRayMeshEdgeService.SetEdgeAlpha(alpha);
            }

            EditorGUI.BeginChangeCheck();
            var color = EditorGUILayout.ColorField("Color", XRayMeshEdgeService.EdgeColor);
            if (EditorGUI.EndChangeCheck())
            {
                XRayMeshEdgeService.SetEdgeColor(color);
            }

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh"))
                {
                    XRayMeshEdgeService.RebuildAll();
                }

                if (GUILayout.Button("Open Window"))
                {
                    XRayGizmosWindow.Open();
                }
            }
        }
    }

    internal static class XRayGizmosSceneToolbarIcons
    {
        public const string BonesIconPath = XRayGizmosPackage.BasePath + "/Editor/UI/Icons/xray-bones.png";
        private const string WeightPaintIconPath = XRayGizmosPackage.BasePath + "/Editor/UI/Icons/xray-weight-paint.png";
        private const string MeshEdgesIconPath = XRayGizmosPackage.BasePath + "/Editor/UI/Icons/xray-mesh-edges.png";

        private static Texture2D bonesIcon;
        private static Texture2D weightPaintIcon;
        private static Texture2D meshEdgesIcon;

        public static Texture2D LoadBonesIcon()
        {
            return bonesIcon != null
                ? bonesIcon
                : bonesIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(BonesIconPath);
        }

        public static Texture2D LoadWeightPaintIcon()
        {
            return weightPaintIcon != null
                ? weightPaintIcon
                : weightPaintIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(WeightPaintIconPath);
        }

        public static Texture2D LoadMeshEdgesIcon()
        {
            return meshEdgesIcon != null
                ? meshEdgesIcon
                : meshEdgesIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(MeshEdgesIconPath);
        }
    }
}
