using System;

namespace Unity.Reflect.Viewer.UI
{
    public enum ActionTypes
    {
        None = 0,

        Login,
        Logout,
        OpenURL,
        SelectTool,
        SetToolState,
        SetSettingsToolState,
        OpenDialog,
        CloseAllDialogs,
        SetDialogMode,
        SetHelpModeID,
        SetStatus,
        SetStatusWithLevel,
        SetStatusLevel,
        ClearStatus,
        RefreshProjectList,
        OpenProject,
        CloseProject,
        DownloadProject,
        RemoveProject,
        OpenOptionDialog,
        SetOptionProject,
        SetProjectSortMethod,
        SetActiveToolbar,
        SetObjectPicker,
        SelectObjects,
        SetBimGroup,
        SetLandingScreenFilter,
        SelectOrbitType,
        OpenSubDialog,
        SetProgressIndicator,
        SetSync,
        Teleport,
        Cancel,

        SetVisibleFilter,
        SetHighlightFilter,
        SetFilterGroup,
        SetViewOption,
        SetSkybox,
        SetClimateOption,
        SetSunStudy,
        SetSunStudyMode,

        SetCameraOption,
        SetJoystickOption,
        SetNavigationOption,
        SetNavigationState,
        LoadScene,
        UnloadScene,

        ResetHomeView,
        SetStatsInfo,
        SetDebugOptions,

        SetTheme,
        SetQuality,

        // AR
        SetARMode,
        EnableAR,
        SetInstructionUIState,
        SetInstructionUI,
        ShowModel,
        ShowBoundingBoxModel,
        SetModelScale,
        SetModelRotation,
        SetPlacementRules,
        EnablePlacement,
        SetARToolState,
        SetPlacementState,

        Failure = Int32.MaxValue,
    }
}
