using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using Unity.Reflect;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine.InputSystem;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.Reflect.Viewer
{
    [RequireComponent(typeof(XRController))]
    public class VRSelector: VRPointer
    {
#pragma warning disable 0649
        [SerializeField]
        InputActionAsset m_InputActionAsset;
#pragma warning restore 0649

        InputAction m_SelectAction;
        string m_CurrentUserId;

        ObjectSelectionInfo m_ObjectSelectionInfo;
        GameObject m_PreviousGameObject;
        bool m_CanSelect;
        List<IDisposable> m_Disposables = new List<IDisposable>();
        IUISelector<NetworkUserData> m_LocalUserGetter;
        IUISelector<NetworkUserData> m_UserGetter;

        void Awake()
        {
            m_SelectionTarget.gameObject.SetActive(false);
            UIStateContext.current.stateChanged += StateChange;


            m_Disposables.Add(UISelectorFactory.createSelector<IPicker>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectPicker), OnObjectSelectorChanged));
            m_Disposables.Add(UISelectorFactory.createSelector<UnityUser>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.user), OnUserChanged));
            m_Disposables.Add(m_LocalUserGetter = UISelectorFactory.createSelector<NetworkUserData>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.localUser), OnLocalUserChanged));
            m_Disposables.Add(m_UserGetter = UISelectorFactory.createSelector<NetworkUserData>(RoomConnectionContext.current, nameof(IRoomConnectionDataProvider<NetworkUserData>.users)));
            m_Disposables.Add(UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool),
                type => m_CanSelect = m_ShowPointer = type == SetActiveToolAction.ToolType.SelectTool));

            m_SelectAction = m_InputActionAsset["VR/Select"];
            m_SelectAction.Enable();
            m_SelectAction.performed += OnSelectTrigger;
        }

        void OnUserChanged(UnityUser newData)
        {
            m_CurrentUserId = newData.UserId;
        }

        protected override void OnDestroy()
        {
            UIStateContext.current.stateChanged -= StateChange;
            m_Disposables.ForEach(x => x.Dispose());
            base.OnDestroy();
        }

        void OnSelectTrigger(InputAction.CallbackContext obj)
        {
            if (!m_CanSelect)
                return;

            m_ObjectSelectionInfo.selectedObjects = m_Results.Select(x => x.Item1).Where(x => x != null).ToList();

            if (m_ObjectSelectionInfo.selectedObjects.Count == 0)
                return;

            if (m_ObjectSelectionInfo.selectedObjects[0] == m_PreviousGameObject)
            {
                m_ObjectSelectionInfo.currentIndex = (m_ObjectSelectionInfo.currentIndex + 1) % m_ObjectSelectionInfo.selectedObjects.Count;
            }
            else if (m_ObjectSelectionInfo.selectedObjects.Count > 1)
            {
                m_ObjectSelectionInfo.selectedObjects = m_ObjectSelectionInfo.selectedObjects.GetRange(0, 1);
                m_ObjectSelectionInfo.currentIndex = 0;
            }

            m_ObjectSelectionInfo.userId = m_CurrentUserId;
            m_ObjectSelectionInfo.colorId = 0;

            Dispatcher.Dispatch(SelectObjectAction.From(m_ObjectSelectionInfo));

            m_PreviousGameObject = m_ObjectSelectionInfo.selectedObjects[0];
        }

        void OnLocalUserChanged(NetworkUserData localUser)
        {
            var matchmakerId = localUser.matchmakerId;
            if (!string.IsNullOrEmpty(matchmakerId) && m_CurrentUserId != matchmakerId)
            {
                m_CurrentUserId = matchmakerId;
            }
        }
    }
}
