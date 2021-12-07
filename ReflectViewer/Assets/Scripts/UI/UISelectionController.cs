using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SharpFlux.Dispatching;
using Unity.Reflect.Actors;
using Unity.Reflect.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace Unity.Reflect.Viewer.UI
{
    public abstract class UISelectionController: MonoBehaviour
    {
        protected struct SelectionData
        {
            public string userId;
            public int colorId;
            public GameObject selectedObject;
        }

        private const string k_HlodPickFailMessage = "Failed to select a distant object, please move closer to the object";

#pragma warning disable CS0649
        [SerializeField]
        ViewerReflectBootstrapper m_Reflect;

        [SerializeField]
        float m_Tolerance;

        [SerializeField]
        Color m_DefaultColor = new Color(255, 102, 0);
#pragma warning restore CS0649

        GameObject m_CurrentSelectedGameObject;
        string m_CurrentPersistentKeyName;
        EntryStableGuid m_CurrentSelectedId;
        EntryStableGuid m_PreviousSelectedId;
        bool m_IsWaitingEnabling;

        Vector2? m_PreviousScreenPoint;
        bool m_SelectMode;
        bool m_Pressed;

        Camera m_Camera;

        string m_CurrentUserId;
        protected List<SelectionData> m_SelectedDatas;

        LoginState? m_LoginState;
        IUISelector<bool> m_MeasureToolStateSelector;
        IUISelector<bool> m_HOLDFilterSelector;
        IUISelector<IPicker> m_ObjectPickerSelector;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoSelector;
        IUISelector<UnityUser> m_UnityUserSelector;
        IUISelector<NetworkUserData> m_LocalUserSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        Vector3 m_LastCameraPosition;
        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolSelector;
        readonly string k_SelectionQueryKey = "sel";

        protected abstract void ChangePalette(Texture2D texture);

        protected virtual void Awake()
        {
            m_SelectedDatas = new List<SelectionData>();

            m_DisposeOnDestroy.Add(m_MeasureToolStateSelector = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnMeasureToolStateDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<List<NetworkUserData>>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.users), OnUsersChanged));
            m_DisposeOnDestroy.Add(m_LocalUserSelector = UISelectorFactory.createSelector<NetworkUserData>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser), OnLocalUserChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnable));
            m_DisposeOnDestroy.Add(m_ObjectPickerSelector = UISelectorFactory.createSelector<IPicker>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectPicker)));
            m_DisposeOnDestroy.Add(m_ObjectSelectionInfoSelector = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo), OnObjectSelectionInfoChanged));
            m_DisposeOnDestroy.Add(m_HOLDFilterSelector = UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.filterHlods)));
            m_DisposeOnDestroy.Add(m_UnityUserSelector = UISelectorFactory.createSelector<UnityUser>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.user)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), OnLoggedStateChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<HighlightFilterInfo>(ProjectContext.current, nameof(IProjectSortDataProvider.highlightFilter), info =>
            {
                StartCoroutine(WaitBeforeUpdateHighlight());
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), project =>
            {
                m_SelectedDatas.Clear();
                UpdateMultiSelection();
            }));
            m_DisposeOnDestroy.Add(m_ActiveToolSelector = UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool),
                type =>
                {
                    if (type != SetActiveToolAction.ToolType.SelectTool)
                        StartCoroutine(UnselectObject());

                    m_SelectMode = type == SetActiveToolAction.ToolType.SelectTool && m_MeasureToolStateSelector.GetValue() == false;
                    UpdateMultiSelection();
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog),
                type =>
                {
                    UpdateMultiSelection();
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Color[]>(UIStateContext.current, nameof(IUIStateDataProvider.colorPalette),
                palette =>
                {
                    SetColorPalette(palette);
                }));

            OrphanUIController.onPointerClick += OnPointerClick;

            m_Reflect.StreamingStarting += bridge =>
            {
                m_Reflect.ViewerBridge.GameObjectCreating += OnGameObjectCreating;
                m_Reflect.ViewerBridge.GameObjectDestroying += OnGameObjectDestroying;
                m_Reflect.ViewerBridge.GameObjectEnabling += OnGameObjectEnabling;
            };

            QueryArgHandler.Register(this, k_SelectionQueryKey, SelectionFromQueryValue, SelectionToQueryValue);
        }

        protected virtual void OnDestroy()
        {
            QueryArgHandler.Unregister(this);
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void OnLocalUserChanged(NetworkUserData localUser)
        {
            // Check if current user had change Id
            var matchmakerId = localUser.matchmakerId;
            if (!string.IsNullOrEmpty(matchmakerId) && m_CurrentUserId != matchmakerId)
            {
                if (m_SelectedDatas.Any(s => s.userId == m_CurrentUserId))
                {
                    var selectedData = m_SelectedDatas.First(s => s.userId == m_CurrentUserId);
                    var selectedObject = selectedData.selectedObject;

                    //Unselect current local selection
                    var info = new ObjectSelectionInfo();
                    info.currentIndex = 0;
                    info.selectedObjects = new List<GameObject>();
                    info.userId = m_CurrentUserId;
                    info.colorId = 0;
                    DispatchSelection(info);

                    m_CurrentUserId = matchmakerId;

                    //Reselect it with new id
                    info.userId = matchmakerId;
                    info.selectedObjects = new List<GameObject>() { selectedObject };
                    DispatchSelection(info);
                }
                else
                {
                    m_CurrentUserId = matchmakerId;
                }

                UpdateMultiSelection();
            }
        }

        void OnUsersChanged(List<NetworkUserData> users)
        {
            foreach (var user in users)
            {
                // Check if a user had already a selected object
                if (m_SelectedDatas.Any(s => s.userId == user.matchmakerId))
                {
                    var selectedData = m_SelectedDatas.First(s => s.userId == user.matchmakerId);
                    if (selectedData.selectedObject != user.selectedObject)
                    {
                        m_SelectedDatas.Remove(selectedData);
                    }
                    else
                    {
                        continue;
                    }
                }

                if (user.selectedObject != null)
                {
                    var userIdentity = UIStateManager.current.GetUserIdentityFromSession(user.matchmakerId);
                    m_SelectedDatas.Add(new SelectionData
                    {
                        userId = user.matchmakerId,
                        selectedObject = user.selectedObject,
                        colorId = (userIdentity.colorIndex + 1)
                    });
                }
            }

            // Check for disconnected users with ongoing selection
            List<SelectionData> missingUserDatas = new List<SelectionData>();
            foreach (var selectedData in m_SelectedDatas)
            {
                if (selectedData.userId == m_CurrentUserId)
                {
                    continue;
                }

                if (!users.Any(u => u.matchmakerId == selectedData.userId))
                {
                    missingUserDatas.Add(selectedData);
                }
            }

            foreach (var missingUserData in missingUserDatas)
            {
                m_SelectedDatas.Remove(missingUserData);
            }

            UpdateMultiSelection();
        }

        void OnVREnable(bool isEnable)
        {
            if (isEnable)
            {
                OrphanUIController.onPointerClick -= OnPointerClick;
            }
            else
            {
                OrphanUIController.onPointerClick += OnPointerClick;
            }
        }

        void OnLoggedStateChanged(LoginState data)
        {
            if (m_LoginState != data)
            {
                switch (data)
                {
                    case LoginState.LoggedIn:
                        var matchmakerId = m_LocalUserSelector.GetValue().matchmakerId;
                        m_CurrentUserId = string.IsNullOrEmpty(matchmakerId)
                            ? m_UnityUserSelector.GetValue()?.UserId
                            : matchmakerId;
                        break;
                    case LoginState.LoggedOut:
                        m_CurrentUserId = String.Empty;
                        break;
                }

                m_LoginState = data;
            }
        }

        void OnMeasureToolStateDataChanged(bool newData)
        {
            m_SelectMode = m_ActiveToolSelector?.GetValue() == SetActiveToolAction.ToolType.SelectTool && newData == false;
            if (newData)
            {
                StartCoroutine(UnselectObject());
            }
        }

        void SelectionFromQueryValue(string keyName)
        {
            m_CurrentPersistentKeyName = WebUtility.UrlDecode(keyName);
        }

        string SelectionToQueryValue()
        {
            return m_CurrentSelectedGameObject == null ? "" : WebUtility.UrlEncode(m_CurrentSelectedGameObject.GetComponent<SyncObjectBinding>().streamKey.key.Name);
        }

        void OnObjectSelectionInfoChanged(SelectObjectAction.IObjectSelectionInfo newData)
        {
            if (newData == null)
                return;
            
            var objectSelectionInfo = (ObjectSelectionInfo) newData;

            if (m_SelectedDatas.Any(s => s.userId == objectSelectionInfo.userId))
            {
                var selectedData = m_SelectedDatas.First(s => s.userId == objectSelectionInfo.userId);
                m_SelectedDatas.Remove(selectedData);
            }

            var selectedObject = objectSelectionInfo.CurrentSelectedObject();
            if (selectedObject != null)
            {
                var metadata = selectedObject.GetComponentInParent<Metadata>();
                if (metadata != null)
                {
                    selectedObject = metadata.gameObject;
                }
                else
                {
                    selectedObject = null;
                }
            }

            if (objectSelectionInfo.userId == m_CurrentUserId)
            {
                m_CurrentSelectedGameObject = selectedObject;
                SyncObjectBinding sync = null;
                if (selectedObject != null)
                    sync = selectedObject.GetComponent<SyncObjectBinding>();
                m_PreviousSelectedId = m_CurrentSelectedId;
                m_CurrentSelectedId = sync != null ? sync.stableId : default;
            }

            if (objectSelectionInfo.CurrentSelectedObject() != null)
            {
                m_SelectedDatas.Add(new SelectionData
                {
                    userId = objectSelectionInfo.userId,
                    selectedObject = selectedObject,
                    colorId = objectSelectionInfo.colorId
                });
            }

            UpdateMultiSelection();
        }

        IEnumerator UnselectObject()
        {
            yield return null;

            var info = new ObjectSelectionInfo();
            info.currentIndex = 0;
            info.selectedObjects = new List<GameObject>();
            info.userId = m_CurrentUserId;
            info.colorId = 0;
            DispatchSelection(info);
        }

        IEnumerator WaitBeforeUpdateHighlight()
        {
            //Wait one frame to be sure the highlight layer is updated
            yield return null;

            if (m_CurrentSelectedGameObject != null)
            {
                if (m_CurrentSelectedGameObject.layer == MetadataFilter.k_OtherLayer)
                {
                    //Force unselect the selected item because it's not on a highlighted layer
                    var info = new ObjectSelectionInfo();
                    info.currentIndex = 0;
                    info.selectedObjects = new List<GameObject>();
                    info.userId = m_CurrentUserId;
                    info.colorId = 0;
                    DispatchSelection(info);
                }
            }

            UpdateMultiSelection();
        }

        protected abstract void UpdateMultiSelection();

        void OnPointerClick(BaseEventData data)
        {
            var extendedPointerEventData = (ExtendedPointerEventData) data;
            if (extendedPointerEventData.dragging || extendedPointerEventData.button != PointerEventData.InputButton.Left)
                return;

            if (!m_SelectMode)
                return;

            if (m_Camera == null || !m_Camera.gameObject.activeInHierarchy)
            {
                m_Camera = Camera.main;
                if (m_Camera == null)
                {
                    Debug.LogError($"[{nameof(UISelectionController)}] active main camera not found!");
                    return;
                }
            }

            var screenPoint = data.currentInputModule.input.mousePosition;

            var info = new ObjectSelectionInfo();

            info.userId = m_CurrentUserId;
            if (m_LastCameraPosition == m_Camera.transform.position &&
                m_PreviousScreenPoint.HasValue && (screenPoint - m_PreviousScreenPoint.Value).magnitude <= m_Tolerance)
            {
                var currentInfo = (ObjectSelectionInfo) m_ObjectSelectionInfoSelector.GetValue();
                info.selectedObjects = currentInfo.selectedObjects;
                info.currentIndex = currentInfo.selectedObjects.Count == 0
                    ? 0
                    : (currentInfo.currentIndex + 1) %
                      currentInfo.selectedObjects.Count;
                DispatchSelection(info);
            }
            else
            {
                var flags = m_HOLDFilterSelector.GetValue() ? new [] {  SpatialActor.k_IsHlodFlag, SpatialActor.k_IsDisabledFlag } : new [] {  SpatialActor.k_IsDisabledFlag };
                var picker = (ISpatialPickerAsync<Tuple<GameObject, RaycastHit>>) m_ObjectPickerSelector.GetValue();

                picker.Pick(m_Camera.ScreenPointToRay(screenPoint), result =>
                {
                    m_LastCameraPosition = m_Camera.transform.position;
                    var selected = result.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer).ToList();
                    if(selected.Count > 0 && selected[0] != null)
                    {
                        var binding = selected[0].GetComponent<SyncObjectBinding>();
                        // If we clicked an hlod
                        if (binding != null && binding.streamKey.source == "hlod")
                        {
                            // If we are using Zenject this can be injected later
                            var popUpManager = UIStateManager.current.popUpManager;

                            // Get custom struct from manager
                            var data = popUpManager.GetNotificationData();

                            // The Notification has an icon and a different default position
                            data.icon = null;
                            data.text = k_HlodPickFailMessage;

                            // Send back the modified struct
                            popUpManager.DisplayNotification(data);

                            // Keep old selection data
                            return;
                        }
                    }

                    info.selectedObjects = selected;
                    info.currentIndex = 0;
                    info.colorId = 0;
                    DispatchSelection(info);

                }, flags);
            }

            m_PreviousScreenPoint = screenPoint;
        }

        void OnGameObjectCreating(GameObjectCreating data)
        {
            foreach (var go in data.GameObjectIds)
            {
                var persistentKey = go.GameObject.GetComponent<SyncObjectBinding>().streamKey.key.Name;
                if (persistentKey.Equals(m_CurrentPersistentKeyName) ||
                    (m_CurrentSelectedGameObject == null &&
                     (go.StableId == m_CurrentSelectedId ||
                      m_CurrentSelectedId == default &&
                      go.StableId == m_PreviousSelectedId)))
                {
                    m_IsWaitingEnabling = true;
                    break;
                }
            }
        }

        void OnGameObjectDestroying(GameObjectDestroying data)
        {
            foreach (var go in data.GameObjectIds)
            {
                if (go.StableId != m_CurrentSelectedId)
                    continue;

                var e = UnselectObject();
                while (e.MoveNext())
                { }
            }
        }

        void OnGameObjectEnabling(GameObjectEnabling data)
        {
            if (!m_IsWaitingEnabling)
                return;

            foreach (var go in data.GameObjectIds)
            {
                var persistentKey = go.GameObject.GetComponent<SyncObjectBinding>().streamKey.key.Name;
                if (go.StableId == m_PreviousSelectedId)
                {
                    var info = (ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue();
                    info.selectedObjects = new List<GameObject> { go.GameObject };
                    m_PreviousScreenPoint = null;

                    DispatchSelection(info);
                }
                else if (persistentKey.Equals(m_CurrentPersistentKeyName))
                {
                    m_CurrentPersistentKeyName = string.Empty;
                    // Revive selection from query value
                    var info = new ObjectSelectionInfo();
                    info.selectedObjects = new List<GameObject> { go.GameObject };
                    info.userId = m_CurrentUserId;
                    info.colorId = 0;

                    DispatchSelection(info);

                    UpdateMultiSelection();
                }
            }
        }

        void DispatchSelection(ObjectSelectionInfo info)
        {
            m_IsWaitingEnabling = false;
            Dispatcher.Dispatch(SelectObjectAction.From(info));
        }

        void SetColorPalette(Color[] palette)
        {
            var texture = new Texture2D(palette.Length + 1, 1, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Point;

            texture.SetPixel(0, 0, m_DefaultColor);

            for (int i = 1; i < palette.Length + 1; i++)
            {
                texture.SetPixel(i, 0, palette[i - 1]);
            }

            texture.Apply();

            ChangePalette(texture);
        }
    }
}
