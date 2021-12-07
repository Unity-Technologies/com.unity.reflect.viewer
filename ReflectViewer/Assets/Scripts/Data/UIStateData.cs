using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{


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

    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UIStateData : IEquatable<UIStateData>, IDialogDataProvider, IToolBarDataProvider, IHelpModeDataProvider, ISyncModeDataProvider, IUIStateDataProvider, IUIStateDisplayProvider<DisplayData>
    {
        public ProjectListFilterData landingScreenFilterData;
        public SettingsToolStateData settingsToolStateData;
        public ToolStateData toolState;
        public NavigationStateData navigationStateData;
        public CameraOptionData cameraOptionData;
        public ProgressData progressData;

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool toolbarsEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool syncEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool operationCancelled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public OpenDialogAction.DialogType activeDialog { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public OpenDialogAction.DialogType activeSubDialog { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetDialogModeAction.DialogMode dialogMode { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetHelpModeIDAction.HelpModeEntryID helpModeEntryId { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetActiveToolBarAction.ToolbarType activeToolbar { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public CloseAllDialogsAction.OptionDialogType activeOptionDialog { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SettingsDialogState settingsDialogState { get; set; }

        [CreateProperty]
        [field: NonSerialized, DontCreateProperty]
        public Project selectedProjectOption { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int projectOptionIndex { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string bimGroup { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public DisplayData display { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string themeName { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Color[] colorPalette { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IUserInfoDialogDataProvider SelectedUserData { get; set; }

        public override string ToString()
        {
            return ToString("( ToolbarEnabled {0}, Sync Enabled {1}, OperationCancelled {2}, StatusMessage {3}, " +
                "StatusMessageLevel {4}, ToolState {5}, ActiveDialog {6}, ActiveSubDialog {7}, DialogMode {8}, " +
                "HelpModeEntryId {9}, ActiveToolbar {10} , ActiveOptionDialog {11}, SettingsDialogState {12}, " +
                "NavigationState {13}, CameraOptionData {14}, SceneOptionData {15}, ProjectOptionIndex {16}, " +
                "SunStudyData {17}, ProgressData {18}, BimGroup {19}, LandingScreenFilterData {20}, " +
                "ThemeName {21}, MeasureToolStateData {22}");
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
                navigationStateData,
                cameraOptionData,
                projectOptionIndex,
                progressData,
                bimGroup,
                landingScreenFilterData,
                themeName);
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
                hashCode = (hashCode * 397) ^ (int)activeDialog;
                hashCode = (hashCode * 397) ^ (int)activeSubDialog;
                hashCode = (hashCode * 397) ^ (int)dialogMode;
                hashCode = (hashCode * 397) ^ (int)helpModeEntryId;
                hashCode = (hashCode * 397) ^ (int)activeToolbar;
                hashCode = (hashCode * 397) ^ (int)activeOptionDialog;
                hashCode = (hashCode * 397) ^ settingsDialogState.GetHashCode();
                hashCode = (hashCode * 397) ^ navigationStateData.GetHashCode();
                hashCode = (hashCode * 397) ^ cameraOptionData.GetHashCode();
                hashCode = (hashCode * 397) ^ (selectedProjectOption != null ? selectedProjectOption.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ projectOptionIndex;
                hashCode = (hashCode * 397) ^ progressData.GetHashCode();
                hashCode = (hashCode * 397) ^ bimGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ landingScreenFilterData.GetHashCode();
                hashCode = (hashCode * 397) ^ display.GetHashCode();
                hashCode = (hashCode * 397) ^ themeName.GetHashCode();
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
                navigationStateData.Equals(other.navigationStateData) &&
                cameraOptionData.Equals(other.cameraOptionData) &&
                Equals(selectedProjectOption, other.selectedProjectOption) &&
                projectOptionIndex == other.projectOptionIndex &&
                progressData.Equals(other.progressData) &&
                bimGroup == other.bimGroup &&
                landingScreenFilterData == other.landingScreenFilterData &&
                display == other.display &&
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
