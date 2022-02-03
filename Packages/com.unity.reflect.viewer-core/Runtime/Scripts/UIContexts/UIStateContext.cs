using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IDialogDataProvider
    {
        public OpenDialogAction.DialogType activeDialog { get; set; }
        public OpenDialogAction.DialogType activeSubDialog { get; set; }
        public CloseAllDialogsAction.OptionDialogType activeOptionDialog { get; set; }
        public SetDialogModeAction.DialogMode dialogMode { get; set; }
    }

    public interface IToolBarDataProvider
    {
        public SetActiveToolBarAction.ToolbarType activeToolbar { get; set; }
        public bool toolbarsEnabled { get; set; }
    }

    public interface IHelpModeDataProvider
    {
        public SetHelpModeIDAction.HelpModeEntryID helpModeEntryId { get; set; }
    }

    public interface ISyncModeDataProvider
    {
        public bool syncEnabled { get; set; }
    }

    public interface IUIStateDataProvider
    {
        public IUserInfoDialogDataProvider SelectedUserData { get; set; }
        public string bimGroup { get; set; }
        public bool operationCancelled { get; set; }
        public string themeName { get; set; }
        public Color[] colorPalette { get; set; }
    }

    public interface IUIStateDisplayProvider<TDisplay>
    {
        public TDisplay display { get; set; }
    }

    public interface IDisplayDataProvider
    {
        public Vector2 screenSize { get; set; }
        public Vector2 scaledScreenSize { get; set; }
        public SetDisplayAction.ScreenSizeQualifier screenSizeQualifier { get; set; }
        public float targetDpi { get; set; }
        public float dpi { get; set; }
        public float scaleFactor { get; set; }
        public SetDisplayAction.DisplayType displayType { get; set; }
    }

    public interface IUserInfoDialogDataProvider
    {
        public string matchmakerId  { get; set; }
        public Vector3 dialogPosition  { get; set; }
    }

    public class UIStateContext : ContextBase<UIStateContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IDialogDataProvider), typeof(IToolBarDataProvider),
            typeof(IHelpModeDataProvider), typeof(ISyncModeDataProvider),
            typeof(IUIStateDisplayProvider<>), typeof(IUIStateDataProvider)};
    }
}
