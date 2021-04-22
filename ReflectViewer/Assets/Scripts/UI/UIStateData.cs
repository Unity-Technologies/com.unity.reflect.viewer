using System;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
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
        ARCardSelection = 16,
        GizmoMode = 17,
        InfoSelect = 18,
        DebugOptions = 19,
        CollaborationMenu = 20,
        CollaborationUserList = 21,
        CollaborationUserInfo = 22,
        LoginScreen = 23,
        LinkSharing = 24,
    }
    /// <summary>
    /// Defines a global mode for dialog buttons. For example Help Mode, which makes clicking any dialog button open a help dialog.
    /// </summary>
    public enum DialogMode
    {
        Normal,
        Help,
        //Options,
    }

    /// <summary>
    /// Defines a global mode for HelpMode Buttons that don't have dialog/subdialog.
    /// </summary>
    public enum HelpModeEntryID
    {
        None,
        Sync,
        HomeReset,
        // Right Toolbar
        OrbitSelect,
        LookAround,
        SunStudyDial,
        // AR Sidebars
        Back,
        Scale,
        Target, // Not used yet
        Ok,
        Cancel,
        ARSelect, // Select button without opening subdialog
        MeasureTool,
        Microphone,
        LinkSharing
    }

    public enum ToolbarType
    {
        OrbitSidebar = 0,
        FlySidebar,
        WalkSidebar,
        ARSidebar,
        ARModelAlignSidebar,
        ARInstructionSidebar,
        ARScaleDial,
        TimeOfDayYearDial,
        AltitudeAzimuthDial,
        VRSidebar,
        NoSidebar
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
        SunstudyTool = 6
    }

    public enum StatusMessageType
    {
        Debug,
        Info,
        Instruction,
        Warning
    }

    public struct StatusMessageData
    {
        public StatusMessageType type;
        public string text;
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
        VRCapability = 0x02,
        SupportsAsyncGPUReadback = 0x04,
    }

    [Serializable]
    public struct UIStateData : IEquatable<UIStateData>
    {
        public bool toolbarsEnabled;
        public SettingsToolStateData settingsToolStateData;
        public bool syncEnabled;
        public bool operationCancelled;
        public ToolState toolState;
        public DialogType activeDialog;
        public DialogType activeSubDialog;
        public DialogMode dialogMode;
        public HelpModeEntryID helpModeEntryId;
        public ToolbarType activeToolbar;
        public OptionDialogType activeOptionDialog;
        public SettingsDialogState settingsDialogState;
        public NavigationState navigationState;
        public CameraOptionData cameraOptionData;
        public SceneOptionData sceneOptionData;
        [NonSerialized]
        public Project selectedProjectOption;
        public int projectOptionIndex;
        public SunStudyData sunStudyData;
        public ProgressData progressData;
        public string bimGroup;
        public string filterGroup;
        public ProjectListFilterData landingScreenFilterData;
        public ArchitectureScale modelScale;
        public DeviceCapability deviceCapability;
        public DisplayData display;
        public string themeName;
        public bool VREnable;
        public Color[] colorPalette;
        public UserInfoDialogData SelectedUserData;

        public override string ToString()
        {
            return ToString("( ToolbarEnabled {0}, Sync Enabled {1}, OperationCancelled {2}, StatusMessage {3}, " +
                "StatusMessageLevel {4}, ToolState {5}, ActiveDialog {6}, ActiveSubDialog {7}, DialogMode {8}, " +
                "HelpModeEntryId {9}, ActiveToolbar {10} , ActiveOptionDialog {11}, SettingsDialogState {12}, " +
                "NavigationState {13}, CameraOptionData {14}, SceneOptionData {15}, ProjectOptionIndex {16}, " +
                "SunStudyData {17}, ProgressData {18}, BimGroup {19}, FilterGroup {20}, LandingScreenFilterData {21}, ModelScale {22}, DeviceCapability {23}, " +
                "ThemeName {24}, MeasureToolStateData {25}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                toolbarsEnabled,
                syncEnabled,
                operationCancelled,
                "",
                "",
                toolState,
                activeDialog,
                activeSubDialog,
                dialogMode,
                helpModeEntryId,
                activeToolbar,
                activeOptionDialog,
                settingsDialogState,
                navigationState,
                cameraOptionData,
                sceneOptionData,
                projectOptionIndex,
                sunStudyData,
                progressData,
                bimGroup,
                filterGroup,
                landingScreenFilterData,
                modelScale,
                deviceCapability,
                themeName,
                VREnable);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = toolbarsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ settingsToolStateData.GetHashCode();
                hashCode = (hashCode * 397) ^ syncEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ operationCancelled.GetHashCode();
                hashCode = (hashCode * 397) ^ toolState.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) activeDialog;
                hashCode = (hashCode * 397) ^ (int) activeSubDialog;
                hashCode = (hashCode * 397) ^ (int) dialogMode;
                hashCode = (hashCode * 397) ^ (int) helpModeEntryId;
                hashCode = (hashCode * 397) ^ (int) activeToolbar;
                hashCode = (hashCode * 397) ^ (int) activeOptionDialog;
                hashCode = (hashCode * 397) ^ settingsDialogState.GetHashCode();
                hashCode = (hashCode * 397) ^ navigationState.GetHashCode();
                hashCode = (hashCode * 397) ^ cameraOptionData.GetHashCode();
                hashCode = (hashCode * 397) ^ sceneOptionData.GetHashCode();
                hashCode = (hashCode * 397) ^ (selectedProjectOption != null ? selectedProjectOption.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ projectOptionIndex;
                hashCode = (hashCode * 397) ^ sunStudyData.GetHashCode();
                hashCode = (hashCode * 397) ^ progressData.GetHashCode();
                hashCode = (hashCode * 397) ^ bimGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ filterGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ landingScreenFilterData.GetHashCode();
                hashCode = (hashCode * 397) ^ modelScale.GetHashCode();
                hashCode = (hashCode * 397) ^ deviceCapability.GetHashCode();
                hashCode = (hashCode * 397) ^ display.GetHashCode();
                hashCode = (hashCode * 397) ^ themeName.GetHashCode();
                hashCode = (hashCode * 397) ^ VREnable.GetHashCode();
                hashCode = (hashCode * 397) ^ colorPalette.GetHashCode();
                hashCode = (hashCode * 397) ^ SelectedUserData.GetHashCode();

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
                settingsToolStateData == other.settingsToolStateData &&
                syncEnabled == other.syncEnabled &&
                operationCancelled == other.operationCancelled &&
                toolState.Equals(other.toolState) &&
                activeDialog == other.activeDialog &&
                activeSubDialog == other.activeSubDialog &&
                dialogMode == other.dialogMode &&
                helpModeEntryId == other.helpModeEntryId &&
                activeToolbar == other.activeToolbar &&
                activeOptionDialog == other.activeOptionDialog &&
                settingsDialogState.Equals(other.settingsDialogState) &&
                navigationState.Equals(other.navigationState) &&
                cameraOptionData.Equals(other.cameraOptionData) &&
                sceneOptionData.Equals(other.sceneOptionData) &&
                Equals(selectedProjectOption, other.selectedProjectOption) &&
                projectOptionIndex == other.projectOptionIndex &&
                sunStudyData.Equals(other.sunStudyData) &&
                progressData.Equals(other.progressData) &&
                bimGroup == other.bimGroup &&
                filterGroup == other.filterGroup &&
                landingScreenFilterData == other.landingScreenFilterData &&
                modelScale == other.modelScale &&
                deviceCapability == other.deviceCapability &&
                display == other.display &&
                VREnable == other.VREnable &&
                themeName == other.themeName &&
                colorPalette == other.colorPalette &&
                SelectedUserData == other.SelectedUserData;
        }

        public static bool operator ==(UIStateData a, UIStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIStateData a, UIStateData b)
        {
            return !(a == b);
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
