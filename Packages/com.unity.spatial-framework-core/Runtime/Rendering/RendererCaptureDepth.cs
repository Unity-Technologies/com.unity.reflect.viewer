namespace Unity.SpatialFramework.Rendering
{
    public enum RendererCaptureDepth
    {
        /// <summary>Get all active renders on an object, its children and manually set renderers.</summary>
        AllChildRenderers,
        /// <summary>Get all active renders on an object and manually set renderers. Ignores children.</summary>
        CurrentRenderer,
        /// <summary>Only uses manually set renderers.</summary>
        ManualOnly,
    }
}
