using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace Unity.Reflect.Viewer.UI
{
    public abstract class UISelectionController : MonoBehaviour
    {
        protected struct SelectionData
        {
            public string userId;
            public int colorId;
            public GameObject selectedObject;
        }

#pragma warning disable CS0649
        [SerializeField]
        float m_Tolerance;

        [SerializeField]
        Color m_DefaultColor = new Color(255, 102, 0);
#pragma warning restore CS0649

        readonly List<Tuple<GameObject, RaycastHit>> m_Results = new List<Tuple<GameObject, RaycastHit>>();

        HighlightFilterInfo m_CurrentHighlightFilter;
        ObjectSelectionInfo m_CurrentObjectSelectionInfo;
        GameObject m_CurrentSelectedGameObject;

        Vector2? m_PreviousScreenPoint;
        bool m_SelectMode;
        bool m_Pressed;

        ToolState? m_CachedToolState;
        MeasureToolStateData? m_CachedMeasureToolStateData;
        DialogType m_CachedActiveDialog;
        Project m_CachedActiveProject;
        Color[] m_CachedColorPalette;

        ISpatialPicker<Tuple<GameObject, RaycastHit>> m_ObjectPicker;
        Camera m_Camera;

        string m_CurrentUserId;
        protected List<SelectionData> m_SelectedDatas;

        LoginState? m_LoginState;

        protected abstract void ChangePalette(Texture2D texture);

        protected virtual void Awake()
        {
            m_SelectedDatas = new List<SelectionData>();

            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateChanged;
            UIStateManager.roomConnectionStateChanged += OnRoomConnectionStateChanged;
            UIStateManager.externalToolChanged += OnExternalToolChanged;

            OrphanUIController.onPointerClick += OnPointerClick;
        }

        void OnRoomConnectionStateChanged(RoomConnectionStateData data)
        {
            // Check if current user had change Id
            var matchmakerId = UIStateManager.current.roomConnectionStateData.localUser.matchmakerId;
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
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, info));

                    m_CurrentUserId = matchmakerId;

                    //Reselect it with new id
                    info.userId = matchmakerId;
                    info.selectedObjects = new List<GameObject>() { selectedObject };
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, info));
                }
                else
                {
                    m_CurrentUserId = matchmakerId;
                }
            }


            foreach (var user in data.users)
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
                    m_SelectedDatas.Add(new SelectionData{
                        userId = user.matchmakerId,
                        selectedObject = user.selectedObject,
                        colorId = (userIdentity.colorIndex+1)
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

                if (!data.users.Any(u => u.matchmakerId == selectedData.userId))
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

        void OnSessionStateChanged(UISessionStateData data)
        {
            if (m_LoginState != data.sessionState.loggedState)
            {
                switch (data.sessionState.loggedState)
                {
                    case LoginState.LoggedIn:
                        var matchmakerId = UIStateManager.current.roomConnectionStateData.localUser.matchmakerId;
                        m_CurrentUserId = string.IsNullOrEmpty(matchmakerId)
                            ? data.sessionState.user?.UserId
                            : matchmakerId;
                        break;
                    case LoginState.LoggedOut:
                        m_CurrentUserId = String.Empty;
                        break;
                }
                m_LoginState = data.sessionState.loggedState;
            }
        }

        private void OnExternalToolChanged(ExternalToolStateData data)
        {
            if (m_CachedMeasureToolStateData == null || m_CachedMeasureToolStateData != data.measureToolStateData)
            {
                m_SelectMode = (m_CachedToolState != null && m_CachedToolState.Value.activeTool == ToolType.SelectTool && data.measureToolStateData.toolState == false);
                m_CachedMeasureToolStateData = data.measureToolStateData;
            }
        }

        void OnStateDataChanged(UIStateData data)
        {
            bool somethingChanged = false;

            if (m_CachedToolState != data.toolState)
            {
                m_SelectMode = (data.toolState.activeTool == ToolType.SelectTool && UIStateManager.current.externalToolStateData.measureToolStateData.toolState == false);
                m_CachedToolState = data.toolState;
                somethingChanged = true;
            }

            if (m_CachedActiveDialog != data.activeSubDialog)
            {
                somethingChanged = true;
                m_CachedActiveDialog = data.activeSubDialog;
            }

            if (m_CachedColorPalette != data.colorPalette)
            {
                m_CachedColorPalette = data.colorPalette;
                SetColorPalette(m_CachedColorPalette);
            }

            if (somethingChanged)
                UpdateMultiSelection();
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_ObjectPicker = data.objectPicker;
            if (data.objectSelectionInfo != m_CurrentObjectSelectionInfo)
            {
                m_CurrentObjectSelectionInfo = data.objectSelectionInfo;

                if (m_SelectedDatas.Any(s => s.userId == data.objectSelectionInfo.userId))
                {
                    var selectedData = m_SelectedDatas.First(s => s.userId == data.objectSelectionInfo.userId);
                    m_SelectedDatas.Remove(selectedData);
                }

                var selectedObject = data.objectSelectionInfo.CurrentSelectedObject();
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

                if (data.objectSelectionInfo.userId == m_CurrentUserId)
                {
                    m_CurrentSelectedGameObject = selectedObject;
                }

                if (data.objectSelectionInfo.CurrentSelectedObject() != null)
                {
                    m_SelectedDatas.Add(new SelectionData{
                        userId = data.objectSelectionInfo.userId,
                        selectedObject = selectedObject,
                        colorId = data.objectSelectionInfo.colorId
                    });
                }

                UpdateMultiSelection();
            }

            if (data.highlightFilter != m_CurrentHighlightFilter)
            {
                m_CurrentHighlightFilter = data.highlightFilter;

                StartCoroutine(WaitBeforeUpdateHighlight());
            }

            if (data.activeProject != m_CachedActiveProject)
            {
                m_CachedActiveProject = data.activeProject;
                m_SelectedDatas.Clear();
                UpdateMultiSelection();
            }
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
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, info));
                }
            }

            UpdateMultiSelection();
        }

        protected abstract void UpdateMultiSelection();

        void OnPointerClick(BaseEventData data)
        {
            if (!m_SelectMode)
                return;

            var screenPoint = data.currentInputModule.input.mousePosition;

            var info = new ObjectSelectionInfo();

            info.userId = m_CurrentUserId;
            if (m_PreviousScreenPoint.HasValue && (screenPoint - m_PreviousScreenPoint.Value).magnitude <= m_Tolerance)
            {
                info.selectedObjects = m_CurrentObjectSelectionInfo.selectedObjects;
                info.currentIndex = m_CurrentObjectSelectionInfo.selectedObjects.Count == 0
                    ? 0
                    : (m_CurrentObjectSelectionInfo.currentIndex + 1) %
                    m_CurrentObjectSelectionInfo.selectedObjects.Count;
            }
            else
            {
                if (m_Camera == null || !m_Camera.gameObject.activeInHierarchy)
                {
                    m_Camera = Camera.main;
                    if (m_Camera == null)
                    {
                        Debug.LogError($"[{nameof(UISelectionController)}] active main camera not found!");
                        return;
                    }
                }

                m_ObjectPicker.Pick(m_Camera.ScreenPointToRay(screenPoint), m_Results);
                // send a copy of the list to preserve previous selection info
                info.selectedObjects = m_Results.Select(x => x.Item1).Where(x => x.layer != MetadataFilter.k_OtherLayer).ToList();
                info.currentIndex = 0;
                info.colorId = 0;
            }

            m_PreviousScreenPoint = screenPoint;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, info));
        }

        public void SendSelection(ObjectSelectionInfo info)
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, info));
        }

        void SetColorPalette(Color[] palette)
        {
            var texture = new Texture2D(palette.Length+1, 1, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Point;

            texture.SetPixel(0,0, m_DefaultColor);

            for(int i=1; i<palette.Length+1; i++)
            {
                texture.SetPixel(i,0, palette[i-1]);
            }
            texture.Apply();

            ChangePalette(texture);
        }
    }
}
