namespace Unity.Reflect.Viewer.Builder
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class BuilderConstants
    {
        public const string ANDROID_BUILD_TARGET = "android";
        public const string IOS_BUILD_TARGET = "ios";
        public const string OSX_BUILD_TARGET = "osxuniversal";
        public const string WIN_BUILD_TARGET = "win64";

        public const string VIEWER_VERSION = "-viewerVersion";
        public const string BUILD_TARGET = "-buildTarget";
        public const string OUTPUT_PATH = "-outputPath";

        public const string DELTA_DNA_BASE_URL = "-deltaDNABase";
        public const string DELTA_DNA_LIVE_URL = "-deltaDNALive";
        public const string DELTA_DNA_DEV_URL = "-deltaDNADev";

        public const string DEFAULT_BUILD_DIRECTORY = "Builds";

    }
}
