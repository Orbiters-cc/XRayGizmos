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
                XRayWeightPaintToolbarToggle.Id)
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

    internal static class XRayGizmosSceneToolbarIcons
    {
        public const string BonesIconPath = XRayGizmosPackage.BasePath + "/Editor/UI/Icons/xray-bones.png";
        private const string WeightPaintIconPath = XRayGizmosPackage.BasePath + "/Editor/UI/Icons/xray-weight-paint.png";

        private static Texture2D bonesIcon;
        private static Texture2D weightPaintIcon;

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
    }
}
