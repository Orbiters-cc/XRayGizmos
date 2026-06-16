using System;
using System.Collections.Generic;
using Orbiters.XRayGizmos;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Orbiters.XRayGizmos.Editor
{
    public class XRayGizmosWindow : EditorWindow
    {
        private ScrollView content;

        [MenuItem("Tools/Orbiters/XRay Gizmos")]
        public static void Open()
        {
            var window = GetWindow<XRayGizmosWindow>();
            window.titleContent = new GUIContent("XRay Gizmos");
            window.minSize = new Vector2(520f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            XRayGizmoService.Changed += Render;
            XRayBonePickingService.Changed += Render;
            XRayWeightPaintService.Changed += Render;
            Selection.selectionChanged += Render;
        }

        private void OnDisable()
        {
            XRayGizmoService.Changed -= Render;
            XRayBonePickingService.Changed -= Render;
            XRayWeightPaintService.Changed -= Render;
            Selection.selectionChanged -= Render;
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                XRayGizmosPackage.BasePath + "/Editor/UI/xraygizmos.uss");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            root.AddToClassList("xray-root");

            var header = new VisualElement();
            header.AddToClassList("xray-header");
            header.Add(XRayGizmosLogo.Create(1.05f));
            var title = new Label("XRay Gizmos");
            title.AddToClassList("xray-title");
            header.Add(title);
            root.Add(header);

            var body = new VisualElement();
            body.AddToClassList("xray-body");
            body.style.flexGrow = 1;
            root.Add(body);

            content = new ScrollView(ScrollViewMode.Vertical);
            content.AddToClassList("xray-scroll");
            body.Add(content);

            Render();
        }

        private void Render()
        {
            if (content == null)
            {
                return;
            }

            content.Clear();

            Question("Armature display");

            var enabled = new Toggle("Show XRay armatures") { value = XRayGizmoService.Enabled };
            enabled.AddToClassList("xray-field");
            enabled.RegisterValueChangedCallback(evt => XRayGizmoService.SetEnabled(evt.newValue));
            content.Add(enabled);

            var scopes = Cards();
            scopes.Add(Card(
                "Selected object",
                DescribeSelectedTarget(),
                XRayGizmoService.TargetMode == XRayGizmoTargetMode.SelectedObject,
                () => XRayGizmoService.SetTargetMode(XRayGizmoTargetMode.SelectedObject),
                "xray-card--small"));
            scopes.Add(Card(
                "Pinned object",
                XRayGizmoService.PinnedObject != null ? XRayGizmoService.PinnedObject.name : "None",
                XRayGizmoService.TargetMode == XRayGizmoTargetMode.PinnedObject,
                () => XRayGizmoService.SetTargetMode(XRayGizmoTargetMode.PinnedObject),
                "xray-card--small"));
            scopes.Add(Card(
                "Whole scene",
                "Loaded armatures",
                XRayGizmoService.TargetMode == XRayGizmoTargetMode.WholeScene,
                () => XRayGizmoService.SetTargetMode(XRayGizmoTargetMode.WholeScene),
                "xray-card--small"));

            var pinned = new ObjectField("Object") { objectType = typeof(GameObject), allowSceneObjects = true, value = XRayGizmoService.PinnedObject };
            pinned.AddToClassList("xray-field");
            pinned.SetEnabled(XRayGizmoService.TargetMode == XRayGizmoTargetMode.PinnedObject);
            pinned.RegisterValueChangedCallback(evt => XRayGizmoService.SetPinnedObject(evt.newValue as GameObject));
            content.Add(pinned);

            Section("Appearance");

            var meshType = new EnumField("Shape", XRayGizmoService.MeshType);
            meshType.AddToClassList("xray-field");
            meshType.RegisterValueChangedCallback(evt =>
                XRayGizmoService.SetMeshType((XRayArmatureMeshType)evt.newValue));
            content.Add(meshType);

            var thickness = new Slider("Thickness", 0.005f, 0.1f)
            {
                value = XRayGizmoService.Thickness,
                showInputField = true
            };
            thickness.AddToClassList("xray-field");
            thickness.RegisterValueChangedCallback(evt => XRayGizmoService.SetThickness(evt.newValue));
            content.Add(thickness);

            var color = new ColorField("Color") { value = XRayGizmoService.GizmoColor };
            color.AddToClassList("xray-field");
            color.RegisterValueChangedCallback(evt => XRayGizmoService.SetColor(evt.newValue));
            content.Add(color);

            var clickableBones = new Toggle("Clickable scene bones") { value = XRayBonePickingService.Enabled };
            clickableBones.AddToClassList("xray-field");
            clickableBones.RegisterValueChangedCallback(evt => XRayBonePickingService.SetEnabled(evt.newValue));
            content.Add(clickableBones);

            if (XRayBonePickingService.Enabled)
            {
                var hovered = XRayBonePickingService.HoveredBone;
                SummaryRow("Hover", hovered != null ? hovered.name : "-");
                Help("Hover xray bones in the Scene view for a white highlight; click to select the matching bone transform.");
            }

            Section("Weight paint");

            var weightPaint = new Toggle("Show weight paint") { value = XRayWeightPaintService.Enabled };
            weightPaint.AddToClassList("xray-field");
            weightPaint.RegisterValueChangedCallback(evt => XRayWeightPaintService.SetEnabled(evt.newValue));
            content.Add(weightPaint);

            var weightAlpha = new Slider("Weight alpha", 0.1f, 1f)
            {
                value = XRayWeightPaintService.OverlayAlpha,
                showInputField = true
            };
            weightAlpha.AddToClassList("xray-field");
            weightAlpha.RegisterValueChangedCallback(evt => XRayWeightPaintService.SetOverlayAlpha(evt.newValue));
            content.Add(weightAlpha);

            if (XRayWeightPaintService.Enabled)
            {
                SummaryRow("Meshes", FormatRenderers(XRayWeightPaintService.ActiveRenderers));
                SummaryRow("Bones", FormatTransforms(XRayWeightPaintService.ActiveBones));
                Help(XRayWeightPaintService.LastStatus ?? "No weight paint overlay is currently visible.");
            }

            var controls = new VisualElement();
            controls.AddToClassList("xray-row");
            var refresh = new Button(() =>
            {
                XRayGizmoService.RebuildAll();
                XRayWeightPaintService.RebuildAll();
            })
            { text = "Refresh" };
            refresh.AddToClassList("xray-back");
            controls.Add(refresh);
            var clear = new Button(() =>
            {
                XRayGizmoService.SetEnabled(false);
                XRayWeightPaintService.SetEnabled(false);
            })
            { text = "Clear" };
            clear.AddToClassList("xray-back");
            controls.Add(clear);
            content.Add(controls);

            Section("Visible");
            var targets = XRayGizmoService.ActiveTargets;
            if (targets.Count == 0)
            {
                Help(XRayGizmoService.LastStatus ?? "No armature is currently visible.");
            }
            else
            {
                foreach (var target in targets)
                {
                    AddTargetRow(target);
                }
            }
        }

        private void AddTargetRow(GameObject target)
        {
            var row = new VisualElement();
            row.AddToClassList("xray-summary-row");

            var name = new Label(target != null ? target.name : "-");
            name.AddToClassList("xray-summary-value");
            name.style.flexGrow = 1;
            row.Add(name);

            var select = new Button(() =>
            {
                if (target == null)
                {
                    return;
                }

                Selection.activeGameObject = target;
                EditorGUIUtility.PingObject(target);
            })
            { text = "Select" };
            select.AddToClassList("xray-back");
            row.Add(select);

            content.Add(row);
        }

        private void SummaryRow(string key, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("xray-summary-row");

            var k = new Label(key);
            k.AddToClassList("xray-summary-key");
            row.Add(k);

            var v = new Label(value);
            v.AddToClassList("xray-summary-value");
            row.Add(v);

            content.Add(row);
        }

        private string DescribeSelectedTarget()
        {
            return XRayGizmoTargetFinder.TryResolveTarget(Selection.activeGameObject, out var selected)
                ? selected.DisplayName
                : "None";
        }

        private static string FormatRenderers(IReadOnlyList<SkinnedMeshRenderer> renderers)
        {
            if (renderers == null || renderers.Count == 0)
            {
                return "-";
            }

            return FormatNames(renderers, renderer => renderer != null ? renderer.name : null);
        }

        private static string FormatTransforms(IReadOnlyList<Transform> transforms)
        {
            if (transforms == null || transforms.Count == 0)
            {
                return "-";
            }

            return FormatNames(transforms, transform => transform != null ? transform.name : null);
        }

        private static string FormatNames<T>(IReadOnlyList<T> items, Func<T, string> getName)
        {
            var names = new List<string>();
            for (int i = 0; i < items.Count && names.Count < 4; i++)
            {
                string name = getName(items[i]);
                if (!string.IsNullOrEmpty(name))
                {
                    names.Add(name);
                }
            }

            if (items.Count > names.Count)
            {
                names.Add("+" + (items.Count - names.Count));
            }

            return names.Count > 0 ? string.Join(", ", names) : "-";
        }

        private void Question(string text)
        {
            var label = new Label(text);
            label.AddToClassList("xray-question");
            content.Add(label);
        }

        private void Help(string text)
        {
            var label = new Label(text);
            label.AddToClassList("xray-help");
            content.Add(label);
        }

        private void Section(string text)
        {
            var label = new Label(text);
            label.AddToClassList("xray-section");
            content.Add(label);
        }

        private VisualElement Cards()
        {
            var cards = new VisualElement();
            cards.AddToClassList("xray-cards");
            content.Add(cards);
            return cards;
        }

        private VisualElement Card(string label, string sublabel, bool active, Action onClick, string extraClass = null)
        {
            var card = new VisualElement();
            card.AddToClassList("xray-card");
            if (!string.IsNullOrEmpty(extraClass))
            {
                card.AddToClassList(extraClass);
            }

            if (active)
            {
                card.AddToClassList("xray-card--active");
            }

            var main = new Label(label);
            main.AddToClassList("xray-card-label");
            card.Add(main);

            if (!string.IsNullOrEmpty(sublabel))
            {
                var sub = new Label(sublabel);
                sub.AddToClassList("xray-card-sublabel");
                card.Add(sub);
            }

            card.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                onClick();
                evt.StopPropagation();
            });
            card.RegisterCallback<ClickEvent>(_ => onClick());
            return card;
        }
    }
}
