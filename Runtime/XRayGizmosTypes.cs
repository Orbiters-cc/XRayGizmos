namespace Orbiters.XRayGizmos
{
    public enum XRayArmatureMeshType
    {
        Pyramid,
        Tapered,
        Box
    }

    public enum XRayGizmoTargetMode
    {
        SelectedObject,
        PinnedObject,
        WholeScene
    }

    public static class XRayGizmosPackage
    {
        public const string Name = "orbiters.xraygizmos";
        public const string DisplayName = "XRay Gizmos";
        public const string BasePath = "Packages/orbiters.xraygizmos";
    }
}
