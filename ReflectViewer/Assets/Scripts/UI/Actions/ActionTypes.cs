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
        ResetToolbars,
        SetDialogMode,
        SetHelpModeID,
        SetStatusMessage,
        SetStatusWithType,
        SetStatusInstructionMode,
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
        SetBimSearch,
        SetLandingScreenFilter,
        SelectOrbitType,
        OpenSubDialog,
        SetProgressIndicator,
        SetSync,
        Teleport,
        FinishTeleport,
        Cancel,
        EnableWalk,
        BeginDrag,
        OnDrag,
        EndDrag,

        SetVisibleFilter,
        SetHighlightFilter,
        SetFilterGroup,
        SetFilterSearch,
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
        SetDisplay,
        SetQuality,

        SetSpatialPriorityWeights,
        SetDebugBoundingBoxMaterials,
        SetCulling,
        SetStaticBatching,

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
        SetARToolState,
        SetPlacementState,

        //Multiplayer
        SetPrivateMode,
        FollowUser,
        EnableVR,
        SetUserInfo,
        ToggleUserMicrophone,

        // External Tools
        ResetExternalTools,
        SetMeasureToolOptions,

        SetLoginSetting,
        DeleteCloudEnvironmentSetting,

        // Link Share
        SetLinkSharePermission,

        Failure = Int32.MaxValue,
    }
}
