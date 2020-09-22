using System;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    public enum DialogType
    {
        None = 0,
        Account = 1,
        Projects = 2,
        Filters = 3,
        MeasureTool = 4,
        ClippingTool = 5,
        OrbitSelect = 6,
        CameraOptions = 7,
        SceneOptions = 8,
        Sequence = 9,
        SelectTool = 10,
        SunStudy = 11,
        BimInfo = 12,
        LandingScreen = 13,
        NavigationMode = 14,
        StatsInfo = 15,
    }

    public enum ToolbarType
    {
        OrbitSidebar = 0,
        FlySidebar = 1,
        WalkSidebar = 2,
        ARSidebar = 3,
        ARInstructionSidebar = 4,
        TimeOfDayYearDial = 5,
        AltitudeAzimuthDial = 6,
        VRSidebar = 7,
        ARScaleDial = 8,
    }

    public enum OptionDialogType
    {
        None = 0,
        ProjectOptions = 1,
    }

    public enum ToolType
    {
        None = 0,
        SelectTool = 1,
        OrbitTool = 2,
        ZoomTool = 3,
        PanTool = 4,
        ClippingTool = 5,
        SunstudyTool = 6,
        MeasureTool = 6
    }

    public enum StatusMessageLevel
    {
        Debug = 0,
        Info = 1,
        Instruction = 2,
    }

    public struct StatusMessageData
    {
        public string text;
        public StatusMessageLevel level;
    }

    public enum ArchitectureScale
    {
        OneToFiveThousand = 5000,
        OneToOneThousand = 1000,
        OneToFiveHundred = 500,
        OneToFourHundred = 400,
        OneToThreeHundred = 300,
        OneToTwoHundred = 200,
        OneToOneHundred = 100,
        OneToFifty = 50,
        OneToTwenty = 20,
        OneToTen = 10,
        OneToFive = 5,
        OneToOne = 1,
    }

    public enum UniversalRenderer
    {
        DefaultForwardRenderer = 0,
        VRRenderer = 1,
        ARRenderer = 2
    }

    [System.Flags]
    public enum DeviceCapability
    {
        None,
        ARCapability = 0x01,
        VRCapability = 0x02
    }

    [Serializable]
    public struct UIStateData : IEquatable<UIStateData>
    {
        public bool toolbarsEnabled;
        public bool syncEnabled;
        public bool operationCancelled;
        public string statusMessage;
        public StatusMessageLevel statusMessageLevel;
        public ToolState toolState;
        public DialogType activeDialog;
        public DialogType activeSubDialog;
        public ToolbarType activeToolbar;
        public OptionDialogType activeOptionDialog;
        public SettingsDialogState settingsDialogState;
        public ScreenOrientation screenOrientation;
        public NavigationState navigationState;
        public CameraOptionData cameraOptionData;
        public SceneOptionData sceneOptionData;
        [NonSerialized]
        public Project selectedProjectOption;
        public int projectOptionIndex;
        public SunStudyData sunStudyData;
        public ProgressData progressData;
        public string bimGroup;
        public ProjectListFilterData landingScreenFilterData;
        public StatsInfoData statsInfoData;
        public ArchitectureScale modelScale;
        public DeviceCapability deviceCapability;

        public override string ToString()
        {
            return ToString("( ToolbarEnabled{0}, Sync Enabled {1}, OperationCancelled {2}, StatusMessage {3} , ActiveTool {4}, ActiveDialog {5}, ActiveToolbar {6}, ActiveOptionDialog {7}" +
                "ScreenOrientation {8},  NavigationState {9}, SessionState {10}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                toolbarsEnabled,
                syncEnabled,
                operationCancelled,
                statusMessage,
                (object)this.toolState,
                (object)this.activeDialog,
                (object)this.activeToolbar,
                (object)this.activeOptionDialog,
                (object)this.screenOrientation,
                (object)this.navigationState,
                (object)this.cameraOptionData,
                (object)this.sceneOptionData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = toolbarsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ syncEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ operationCancelled.GetHashCode();
                hashCode = (hashCode * 397) ^ (statusMessage != null ? statusMessage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ toolState.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) activeDialog;
                hashCode = (hashCode * 397) ^ (int) activeSubDialog;
                hashCode = (hashCode * 397) ^ (int) activeToolbar;
                hashCode = (hashCode * 397) ^ (int) activeOptionDialog;
                hashCode = (hashCode * 397) ^ settingsDialogState.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) screenOrientation;
                hashCode = (hashCode * 397) ^ navigationState.GetHashCode();
                hashCode = (hashCode * 397) ^ cameraOptionData.GetHashCode();
                hashCode = (hashCode * 397) ^ sceneOptionData.GetHashCode();
                hashCode = (hashCode * 397) ^ (selectedProjectOption != null ? selectedProjectOption.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ projectOptionIndex;
                hashCode = (hashCode * 397) ^ sunStudyData.GetHashCode();
                hashCode = (hashCode * 397) ^ progressData.GetHashCode();
                hashCode = (hashCode * 397) ^ bimGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ landingScreenFilterData.GetHashCode();
                hashCode = (hashCode * 397) ^ modelScale.GetHashCode();
                hashCode = (hashCode * 397) ^ deviceCapability.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is UIStateData other && Equals(other);
        }

        public bool Equals(UIStateData other)
        {
            return toolbarsEnabled == other.toolbarsEnabled &&
                syncEnabled == other.syncEnabled &&
                operationCancelled == other.operationCancelled &&
                statusMessage == other.statusMessage &&
                toolState.Equals(other.toolState) &&
                activeDialog == other.activeDialog &&
                activeSubDialog == other.activeSubDialog &&
                activeToolbar == other.activeToolbar &&
                activeOptionDialog == other.activeOptionDialog &&
                settingsDialogState.Equals(other.settingsDialogState) &&
                screenOrientation == other.screenOrientation &&
                navigationState.Equals(other.navigationState) &&
                cameraOptionData.Equals(other.cameraOptionData) &&
                sceneOptionData.Equals(other.sceneOptionData) &&
                Equals(selectedProjectOption, other.selectedProjectOption) &&
                projectOptionIndex == other.projectOptionIndex &&
                sunStudyData.Equals(other.sunStudyData) &&
                progressData.Equals(other.progressData) &&
                bimGroup == other.bimGroup &&
                landingScreenFilterData == other.landingScreenFilterData &&
                modelScale == other.modelScale &&
                deviceCapability == other.deviceCapability;
        }

        public static bool operator ==(UIStateData a, UIStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIStateData a, UIStateData b)
        {
            return !(a == b);
        }

        public void LogStatusMessage(string message, StatusMessageLevel level = StatusMessageLevel.Info)
        {
            if (level >= statusMessageLevel)
            {
                statusMessage = message;
            }
        }
    }

    public enum SettingsTab
    {
        Connection,
        Controls,
        Options
    }

    [Serializable]
    public struct SettingsDialogState
    {
        public SettingsTab activeTab;

        public override string ToString()
        {
            return ToString("(SettingsTab {0}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object)this.activeTab);
        }

        public override int GetHashCode()
        {
            return (int)activeTab;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SettingsDialogState))
                return false;
            return this.Equals((SettingsDialogState)obj);
        }

        public bool Equals(SettingsDialogState other)
        {
            return this.activeTab == other.activeTab;
        }

        public static bool operator ==(SettingsDialogState a, SettingsDialogState b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SettingsDialogState a, SettingsDialogState b)
        {
            return !(a == b);
        }
    }
}
