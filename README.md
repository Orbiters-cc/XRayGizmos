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

## Notes

- The package has no dependency on MCB or the VRChat SDK.
- Armature detection is based on usable `SkinnedMeshRenderer` bone arrays.
- Multiple meshes pointing to the same resolved armature are displayed once.
- Armature geometry is generated from the selected renderer's bone array and
  follows the live bones through a `SkinnedMeshRenderer`.
