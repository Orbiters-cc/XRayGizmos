# XRay Gizmos

XRay Gizmos is a Unity editor tool for showing armatures as xray overlay
meshes in the Scene view.

Open it from:

```text
Tools > Orbiters > XRay Gizmos
```

The window can display armatures for the active selection, a pinned object, or
every skinned armature found in loaded scenes. Generated gizmo objects are hidden,
editor-only, and not saved into the scene.

Weight paint mode overlays Blender-style weight colors on selected skinned meshes
for the selected bone transforms. Selecting only a bone searches loaded scenes for
skinned meshes with positive weight on that bone. Selecting an `Armature` object
or a renderer's root bone displays the combined weight of all bones under that
armature/root.

## Notes

- The package has no dependency on MCB or the VRChat SDK.
- Armature detection is based on usable `SkinnedMeshRenderer` bone arrays.
- Multiple meshes pointing to the same resolved armature are displayed once.
- Armature geometry is generated from the selected renderer's bone array and
  follows the live bones through a `SkinnedMeshRenderer`.
- Weight paint uses a blue, cyan, green, yellow, red ramp over cloned editor-only
  skinned meshes, leaving source mesh assets untouched.
