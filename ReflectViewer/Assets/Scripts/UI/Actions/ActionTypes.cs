using System;

namespace Unity.Reflect.Viewer.UI
{
    public enum ActionTypes
    {
        None = 0,

        Login,
        Logout,
        SelectTool,
        SetToolState,
        OpenDialog,
        CloseAllDialogs,
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
        SetActiveToolbar,
        SelectObjects,
        SetBimGroup,
        SetLandingScreenFilter,
        SelectOrbitType,
        OpenSubDialog,
        SetProgressIndicator,
        SetSync,
        Teleport,

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
        SetNavigationMode,
        LoadScene,
        UnloadScene,

        SetScreenOrientation,
        ResetHomeView,
        SetStatsInfo,

        // AR
        SetInstructionUI,
        ShowModel,
        ShowBoundingBoxModel,
        Cancel,
        SetModelScale,
        SetModelRotation,
        SetPlacementRules,

        Failure = Int32.MaxValue,
    }
}
