
namespace Unity.Properties.UI
{
    /// <summary>
    /// Defines the different mode of display for the <see cref="System.Version"/> type.
    /// </summary>
    public enum SystemVersionUsage
    {
        MajorMinor = 0,
        MajorMinorBuild = 1,
        MajorMinorBuildRevision = 2
    }
    
    /// <summary>
    /// Use this attribute on fields and properties of type <see cref="System.Version"/> to indicate which properties
    /// should be displayed.
    /// </summary>
    public class SystemVersionUsageAttribute : InspectorAttribute
    {
        /// <summary>
        /// Returns the information about how a <see cref="System.Version"/> should be displayed. 
        /// </summary>
        public SystemVersionUsage Usage { get; }

        /// <summary>
        /// Return <see langword="true"/> if the <see cref="System.Version.Build"/> property should be displayed.
        /// </summary>
        public bool IncludeBuild =>
            Usage == SystemVersionUsage.MajorMinorBuild || Usage == SystemVersionUsage.MajorMinorBuildRevision;

        /// <summary>
        /// Return <see langword="true"/> if the <see cref="System.Version.Revision"/> property should be displayed. 
        /// </summary>
        public bool IncludeRevision =>
            Usage == SystemVersionUsage.MajorMinorBuildRevision;

        /// <summary>
        /// Constructs a new instance of <see cref="SystemVersionUsageAttribute"/> with the provided usage.
        /// </summary>
        /// <param name="usage">The indented usage of the <see cref="System.Version"/> type.</param>
        public SystemVersionUsageAttribute(SystemVersionUsage usage = SystemVersionUsage.MajorMinorBuildRevision)
        {
            Usage = usage;
        }
    }
}