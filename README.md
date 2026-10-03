# XRay Gizmos

## 0.2.7 — 2026-10-03

- Enable symmetry controls during Play Mode using Toolkit mirror overrides that survive the animation update.
- Keep editor posing changes separate from the uploaded avatar runtime.

XRay Gizmos is a Unity editor tool for showing armatures as xray overlay
meshes in the Scene view.

Open it from:

```text
Tools > Orbiters > XRay Gizmos
```

The window can display armatures for the active selection, a pinned object, or
every skinned armature found in loaded scenes. Generated gizmo objects are hidden,
editor-only, and not saved into the scene.

Clickable scene bones can be enabled from the window. When enabled, hovering an
xray bone in the Scene view draws a white highlight, and clicking selects the
matching bone transform in the hierarchy. Selected bones are also highlighted in
white while the xray armature display is active.

Bone picking participates in Unity's normal handle arbitration and yields to active
drags, the View tool and camera navigation. Only the segment in front of the camera's
near plane is pickable. Shift-click adds a bone; Ctrl/Cmd-click toggles it. Hover is
kept separately for each Scene view, and empty-space clicks remain available to Unity.

## Mirror posing

Click **Mirror** in the Scene-view toolbar or the window. Mirror is a mode: it stays
on while nothing can be mirrored and mirrors the rig of the avatar or bone you select
next. Rotation and movement of one side are mirrored to its partner across the
avatar root's local X plane. The window shows the active rig and selected partner.
It works independently of bone visibility and uses Toolkit's shared editor posing
service; XRay Gizmos supplies the rig when the selection is outside any humanoid.

Humanoid mappings and left/right bone names are supported. Mesh bind poses account
for differing local bone axes; pairs without bind data use their enable-time pose,
reported in the status. Undo includes both sides. Center bones and scale edits are
not mirrored. Both-side selections preserve explicit edits on each side. Mirror
pauses during animation preview and play mode, stays on through script reloads and
binds again after bone hierarchy changes. Pairs under an unevenly scaled bone are
skipped.

## Clothing preview

With the VRChat avatar SDK in the project, **Clothing** (Scene-view toolbar and window) does
what My Avatar's Posing › Clothing does: clothing and accessories follow the pose of the selected
avatar (or the scene's only avatar) as they will once built, through VRCFury Armature Links,
My Avatar attachments and matching bone names, and go back where they were when it is switched
off. It lives in an optional assembly that compiles only with the SDK; without it nothing shows.

Requires **Orbiters Toolkit 0.3.7 or newer within 0.3.x**, declared in `vpmDependencies`. MCP for Unity
is optional and is not needed for posing. When installing without VPM, install
Toolkit alongside XRayGizmos.

Weight paint mode overlays Blender-style weight colors on selected skinned meshes
for the selected bone transforms. Selecting only a bone searches loaded scenes for
skinned meshes with positive weight on that bone. Selecting an `Armature` object
or a renderer's root bone displays the combined weight of all bones under that
armature/root.

Mesh edge mode overlays polygon edges on selected skinned meshes. If only a bone
or armature root is selected, it searches loaded scenes for skinned meshes using
that bone or armature. The edge overlay uses optimized editor-only skinned edge
meshes with configurable color and opacity.

ReFit debug labels are drawn automatically while ReFit debug mode is enabled.
Each ReFit debug snapshot gets a readable Scene view label with a connector line
to the snapshot, and nearby labels are separated to reduce overlap.

## Notes

- The package has no dependency on MCB. The VRChat SDK is optional (Clothing preview only).
- Armature detection is based on usable `SkinnedMeshRenderer` bone arrays.
- Multiple meshes pointing to the same resolved armature are displayed once.
- Armature geometry is generated from the selected renderer's bone array and
  follows the live bones through a `SkinnedMeshRenderer`.
- Scene bone picking uses the displayed xray armature segments and selects the
  matching source bone transform, not the hidden gizmo object.
- Weight paint uses a blue, cyan, green, yellow, red ramp over cloned editor-only
  skinned meshes, leaving source mesh assets untouched.
- Mesh edges are generated from source mesh vertices, active blendshape
  deformation, and bone weights, leaving source mesh assets untouched.
- Scene labels are editor-only GUI overlays; ReFit debug labels are detected from
  ReFit's debug snapshot naming and editor preference without a package
  dependency.
