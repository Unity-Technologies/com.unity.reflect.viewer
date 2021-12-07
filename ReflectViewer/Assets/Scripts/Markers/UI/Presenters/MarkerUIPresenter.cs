using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Stores;
using Unity.Properties;
using Unity.Reflect.Markers;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Model;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Markers.UI;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class MarkerUIPresenter
        : MonoBehaviour, IStore<MarkerEditViewModel>, IStore<MarkerListViewModel>, IStore<MarkerDraggableEditorViewModel>, IContextPropertyProvider
    {
        [SerializeField, Tooltip("Marker editor state"), UIContextProperties("MarkerEditContext")]
        [ContextButton("Marker Edit changed","OnMarkerEditContextChanged")]
        MarkerEditViewModel m_MarkerEdit;
        MarkerEditViewModel IStore<MarkerEditViewModel>.Data => m_MarkerEdit;

        [SerializeField, Tooltip("Marker List state"), UIContextProperties("MarkerListContext")]
        [ContextButton("Marker List changed","OnMarkerListContextChanged")]
        MarkerListViewModel m_MarkerList;
        MarkerListViewModel IStore<MarkerListViewModel>.Data => m_MarkerList;

        [SerializeField, Tooltip("Marker Drag state"), UIContextProperties("MarkerDraggableEditorContext")]
        [ContextButton("Marker Drag changed","OnMarkerDraggableEditorContextChanged")]
        MarkerDraggableEditorViewModel m_MarkerDrag;
        MarkerDraggableEditorViewModel IStore<MarkerDraggableEditorViewModel>.Data => m_MarkerDrag;

        [SerializeField]
        DraggableMarkerPlacement m_DraggableMarkerPlacement = null;
        [SerializeField]
        MarkerGraphicManager m_MarkerGraphicManager;

        [SerializeField]
        MarkerDialogController m_MarkerDialogController;

        [SerializeField]
        Button m_NewMarkerButton;
        [SerializeField]
        ButtonControl m_SelectionModeButton;
        [SerializeField]
        ButtonControl m_ScanQRButton;
        [SerializeField]
        GameObject m_ARModePanel;
        [SerializeField]
        MarkerController m_MarkerController;

        NavigationModeUIController m_NavigationModeUIController;

        IContextTarget m_MarkerEditContextTarget;
        IContextTarget m_MarkerListContextTarget;
        IContextTarget m_MarkerDraggableEditorContextTarget;

        IDispatcher m_Dispatcher;

        Marker m_CachedMarker;
        ThrottleAction m_RefreshList = new ThrottleAction();
        ThrottleAction m_UpdateDragHandle = new ThrottleAction();
        IUISelector<SetARModeAction.ARMode> m_ARModeSelector;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;

        bool m_MarkerModeActive = true;
        readonly object m_SyncRoot = new object();

        public bool hasChanged;
        public string DispatchToken { get; set; }
        public bool HasChanged
        {
            get { return hasChanged; }
            set
            {
                if (!m_Dispatcher.IsDispatching)
                    throw new InvalidOperationException("Must be invoked while dispatching.");
                hasChanged = value;
            }
        }

        public void Setup(IDispatcher dispatcher)
        {
            m_Dispatcher = dispatcher;
            m_MarkerEditContextTarget = MarkerEditContext.BindTarget(m_MarkerEdit);
            m_MarkerListContextTarget = MarkerListContext.BindTarget(m_MarkerList);
            m_MarkerDraggableEditorContextTarget = MarkerDraggableEditorContext.BindTarget(m_MarkerDrag);

            DispatchToken = m_Dispatcher.Register<Payload<IViewerAction>>(InvokeOnDispatchMarkers);

            if (m_NavigationModeUIController == null)
                m_NavigationModeUIController = FindObjectOfType<NavigationModeUIController>();
            if (m_MarkerGraphicManager == null)
                m_MarkerGraphicManager = FindObjectOfType<MarkerGraphicManager>();

            // Configure actions in the controller and edit view
            LinkActions(m_MarkerController);
            LinkActions(ref m_MarkerEdit);
            LinkActions(ref m_MarkerList);

            m_NewMarkerButton.onClick.AddListener(OnCreateMarker);
            if (m_SelectionModeButton)
                m_SelectionModeButton.onControlUp.AddListener(ToggleSelectionMode);
            if(m_ScanQRButton)
                m_ScanQRButton.onControlUp.AddListener(OnScanMarkerButton);

            m_MarkerDialogController.OnEditToggled += HandleEditToggled;
            m_DraggableMarkerPlacement.OnValueUpdate += OnDragMarkerUpdate;
            m_ARModeSelector = UISelectorFactory.createSelector<SetARModeAction.ARMode>(ARContext.current, nameof(IARModeDataProvider.arMode), OnARModeChange);
            m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode));
        }

        void OnDestroy()
        {
            // Remove actions from this object
            UnlinkActions(ref m_MarkerEdit);
            UnlinkActions(ref m_MarkerList);
            UnlinkActions(m_MarkerController);
            m_DraggableMarkerPlacement.OnValueUpdate -= OnDragMarkerUpdate;
            m_MarkerDialogController.OnEditToggled -= HandleEditToggled;
            m_ARModeSelector?.Dispose();
            if (m_SelectionModeButton)
                m_SelectionModeButton.onControlUp.RemoveListener(ToggleSelectionMode);
            if(m_ScanQRButton)
                m_ScanQRButton.onControlUp.RemoveListener(OnScanMarkerButton);
            m_NewMarkerButton.onClick.RemoveListener(OnCreateMarker);

            m_NavigationModeSelector?.Dispose();
        }

        void LinkActions(ref MarkerEditViewModel markerEdit)
        {
            markerEdit.Actions = new MarkerActions
            {
                Save = OnEditSave,
                Delete = OnEditDelete,
                Export = OnEditExport
            };
        }

        void LinkActions(ref MarkerListViewModel markerList)
        {
            markerList.Actions = new MarkerListActions()
            {
                Delete = OnListDelete,
                Export = OnListExport
            };
        }

        void LinkActions(IMarkerController markerController)
        {
            markerController.OnMarkerUpdated += OnMarkerChanged;
            markerController.OnMarkerListUpdated += OnMarkerListUpdated;
        }

        void UnlinkActions(ref MarkerEditViewModel markerEdit)
        {

        }

        void UnlinkActions(ref MarkerListViewModel markerList)
        {

        }

        void UnlinkActions(IMarkerController markerController)
        {
            markerController.OnMarkerUpdated -= OnMarkerChanged;
            markerController.OnMarkerListUpdated -= OnMarkerListUpdated;
        }

        void OnARModeChange(SetARModeAction.ARMode mode)
        {
            if (mode == SetARModeAction.ARMode.MarkerBased && !m_MarkerModeActive)
            {
                // Turn on marker mode
                if (m_ARModePanel)
                    m_ARModePanel.SetActive(true);
                if (m_DraggableMarkerPlacement)
                    m_DraggableMarkerPlacement.Close();
                m_MarkerModeActive = true;
            }
            else if (mode != SetARModeAction.ARMode.MarkerBased && m_MarkerModeActive)
            {
                // Turn off marker mode
                if (m_ARModePanel)
                    m_ARModePanel.SetActive(false);
                m_MarkerModeActive = false;
            }
        }

        // Export the edited marker as a printable graphic
        void OnEditExport()
        {
            Dispatcher.Dispatch(SetStatusMessage.From("Exporting marker."));
            m_MarkerGraphicManager.PrintMarker(m_MarkerEdit.Id);
        }

        void OnListExport()
        {
            Dispatcher.Dispatch(SetStatusMessage.From($"Exporting {m_MarkerList.Markers.Selected.Count} markers."));
            m_MarkerGraphicManager.PrintMarkers(m_MarkerList.Markers.Selected);
        }

        void OnListDelete()
        {
            var data = UIStateManager.current.popUpManager.GetModalPopUpData();

            data.title = $"Delete {m_MarkerList.Markers.Selected.Count} Markers?";
            data.text = $"Are you sure you wish to delete {m_MarkerList.Markers.Selected.Count} markers? This cannot be undone.";
            data.negativeText = "Cancel";
            data.positiveText = "Delete";
            data.negativeCallback = () => { };
            data.positiveCallback = () =>
            {
                Dispatcher.Dispatch(SetStatusMessage.From($"Deleting {m_MarkerList.Markers.Selected.Count} markers."));
                m_MarkerController.MarkerStorage.Delete(m_MarkerList.Markers.Selected);
            };
            UIStateManager.current.popUpManager.DisplayModalPopUp(data);
        }

        // Update marker controller with the changed values.
        void OnEditSave()
        {
            if (m_DraggableMarkerPlacement.Active)
            {
                m_DraggableMarkerPlacement.Close();
            }
            Dispatcher.Dispatch(SetStatusMessage.From("Marker saved successfully."));
            m_CachedMarker = m_MarkerEdit.ToMarker();
            UpdateMarkerInList(m_CachedMarker);
            m_MarkerController.EditMarker(m_CachedMarker);
            EditClose();
        }

        void UpdateMarkerInList(Marker marker)
        {
            var markerList = m_MarkerList.Markers;
            for (int i = 0; i < markerList.Markers.Count; i++)
            {
                if (markerList.Markers[i].Id == marker.Id)
                {
                    markerList.Markers[i] = marker;
                    m_MarkerList.Markers = markerList;
                    m_MarkerListContextTarget.UpdateWith(ref m_MarkerList, UpdateNotification.ForceNotify);
                    return;
                }
            }
        }

        void EditClose()
        {
            m_MarkerDialogController.SetEditPanel(false);
        }


        void HandleEditToggled(bool newState)
        {
            if (!newState)
            {
                m_DraggableMarkerPlacement.Close();
                var crt = RunSoon(ClearSelection);
                StartCoroutine(crt);
            }
        }

        // Reset values to the cached value
        void OnEditReset()
        {
            m_MarkerController.Visualize(m_CachedMarker);
            SelectMarker(m_CachedMarker);
        }

        void OnEditDelete()
        {
            var data = UIStateManager.current.popUpManager.GetModalPopUpData();
            data.title = "Delete Marker?";
            data.text = $"Are you sure you wish to delete the marker {m_MarkerEdit.Name}?  This cannot be undone.";
            data.negativeText = "Cancel";
            data.positiveText = "Delete";
            data.negativeCallback = () => { };
            data.positiveCallback = () =>
            {
                m_MarkerController.DeleteMarker(m_MarkerEdit.ToMarker());
                EditClose();
            };
            UIStateManager.current.popUpManager.DisplayModalPopUp(data);
        }

        void OnScanMarkerButton(BaseEventData evt)
        {
            var navigationMode = m_NavigationModeSelector.GetValue();
            if (navigationMode == SetNavigationModeAction.NavigationMode.AR)
            {
                _ = RecycleAR(SetARModeAction.ARMode.MarkerBased);
            }
            else
            {
                Dispatcher.Dispatch(SetARModeAction.From(SetARModeAction.ARMode.MarkerBased));
            }
        }

        async Task RecycleAR(SetARModeAction.ARMode arMode)
        {
            m_NavigationModeUIController.StartOrbitMode();
            await Task.Delay(100);
            Dispatcher.Dispatch(SetARModeAction.From(arMode));
        }

        void PrintAllMarkers()
        {
            m_MarkerGraphicManager.PrintAll();
        }

        void ToggleSelectionMode(BaseEventData evt)
        {
            var listState = m_MarkerList.Markers;
            var actionState = m_MarkerList.Actions;

            if (listState.Mode == MarkerListProperty.SelectionMode.Single)
            {
                // Switch to multiselect
                listState.Mode = MarkerListProperty.SelectionMode.Multiple;
                // Close edit panel
                EditClose();
                // Enable bulk item options (Delete, Print)
                // Update Select button text
                m_SelectionModeButton.text = "Cancel";
                actionState.Active = true;

            }
            else
            {
                // switch to single select
                listState.Mode = MarkerListProperty.SelectionMode.Single;
                m_SelectionModeButton.text = "Select";
                actionState.Active = false;
                if (listState.Selected.Count > 0)
                {
                    listState.Selected.Clear();
                }
                actionState.MarkersSelected = listState.Selected.Count;
            }

            m_MarkerList.Markers = listState;
            m_MarkerList.Actions = actionState;
            m_MarkerListContextTarget.UpdateWith(ref m_MarkerList);
        }

        // Action for the CreateMarker button
        void OnCreateMarker()
        {
            m_DraggableMarkerPlacement.Open();

            // Create a new marker
            Marker newMarker = new Marker("New Marker");
            SelectMarker(newMarker);
        }

        void OnDragMarkerUpdate(Pose newMarkerPose)
        {
            if (m_MarkerController.AlignedObject == null)
                return;
            var local = Marker.InverseTransformPose(m_MarkerController.AlignedObject.Transform, newMarkerPose);
            var rotation = local.rotation.eulerAngles;
            var effectiveRotation = new Vector3(rotation.x, m_MarkerEdit.YAxis, rotation.z);

            // Only update the Y rotation if the marker is on a wall
            // We can determine if on a wall if the X is near zero
            int xmod = Mathf.RoundToInt((rotation.x * 4) / 360) % 4;
            if (xmod == 0)
                effectiveRotation.y = rotation.y;

            // Discard if similar to existing data.
            if (PivotTransform.Similar(local.position, new Vector3(m_MarkerEdit.X, m_MarkerEdit.Y, m_MarkerEdit.Z)) &&
                PivotTransform.Similar(effectiveRotation, new Vector3(m_MarkerEdit.XAxis, m_MarkerEdit.YAxis, m_MarkerEdit.ZAxis)))
                return;

            m_MarkerEdit.X = local.position.x;
            m_MarkerEdit.Y = local.position.y;
            m_MarkerEdit.Z = local.position.z;
            m_MarkerEdit.XAxis = effectiveRotation.x;
            m_MarkerEdit.YAxis = effectiveRotation.y;
            m_MarkerEdit.ZAxis = effectiveRotation.z;
            m_MarkerEditContextTarget.UpdateWith(ref m_MarkerEdit);

            m_MarkerController.Visualize(m_MarkerEdit.ToMarker());
        }

        [UsedImplicitly]
        void OnMarkerEditContextChanged()
        {
            m_MarkerEditContextTarget.UpdateWith(ref m_MarkerEdit);
            m_MarkerController.Visualize(m_MarkerEdit.ToMarker());
        }

        [UsedImplicitly]
        void OnMarkerListContextChanged()
        {
            m_MarkerListContextTarget.UpdateWith(ref m_MarkerList);
        }

        [UsedImplicitly]
        void OnMarkerDraggableEditorContextChanged()
        {
            m_MarkerDraggableEditorContextTarget.UpdateWith(ref m_MarkerDrag);
        }

        void OnMarkerChanged(IMarker updatedMarker)
        {
            var markerList = m_MarkerList.Markers;
            markerList.Active = updatedMarker.Id;
            markerList.Markers = m_MarkerController.MarkerStorage.Markers;
            m_MarkerList.Markers = markerList;
            m_RefreshList.Run(RefreshMarkerList, 0.1f);
        }
        void OnMarkerListUpdated()
        {
            RefreshMarkerList();
        }

        void RefreshMarkerList()
        {
            var markerListProperty = new MarkerListProperty();
            markerListProperty.Markers = m_MarkerController.MarkerStorage.Markers;
            markerListProperty.Selected = new List<SyncId>();
            markerListProperty.OnSelectionUpdated += HandleSelectionUpdate;
            var actions = m_MarkerList.Actions;
            actions.MarkersSelected = markerListProperty.Selected.Count;
            m_MarkerList.Markers = markerListProperty;
            m_MarkerList.Actions = actions;
            m_MarkerListContextTarget.UpdateWith(ref m_MarkerList);
        }

        void HandleSelectionUpdate()
        {
            if (m_MarkerList.Markers.Mode == MarkerListProperty.SelectionMode.Single)
            {
                if (m_MarkerList.Markers.Selected.Count > 0)
                {
                    var marker = GetSelectedMarker(m_MarkerList.Markers.Selected[0]);
                    if (marker != null)
                        SelectMarker(marker.Value);
                }
                else
                {
                    m_MarkerDialogController.SetEditPanel(false);
                }
            } else if (m_MarkerList.Markers.Mode == MarkerListProperty.SelectionMode.Multiple)
            {
                // Nothing currently
            }
            var listActions = m_MarkerList.Actions;
            listActions.MarkersSelected = m_MarkerList.Markers.Selected.Count;
            m_MarkerList.Actions = listActions;
            m_MarkerListContextTarget.UpdateWith(ref m_MarkerList);
        }

        Marker? GetSelectedMarker(SyncId id)
        {
            // Default to current stored marker
            var marker = m_MarkerController.MarkerStorage.Get(id.Value);
            if (marker != null)
                return marker;

            // Pull an unsaved marker if one exists
            foreach (var item in m_MarkerList.Markers.Markers)
            {
                if (item.Id == id)
                    return item;
            }

            return null;
        }

        public void SelectMarker(Marker selectedMarker)
        {
            // Open marker editor
            m_CachedMarker = selectedMarker;
            m_MarkerDialogController.SetEditPanel(true);
            m_MarkerEdit.Present(m_CachedMarker);
            m_MarkerEditContextTarget.UpdateWith(ref m_MarkerEdit);
            if (m_ARModeSelector.GetValue() == SetARModeAction.ARMode.None)
                OnMoveMarker();

            // Select marker in the list
            var list = m_MarkerList.Markers;
            if (!list.Markers.Contains(selectedMarker))
            {
                list.Markers.Add(selectedMarker);
            }
            list.Selected = new List<SyncId>{selectedMarker.Id};

            m_MarkerList.Markers = list;
            m_MarkerListContextTarget.UpdateWith(ref m_MarkerList, UpdateNotification.ForceNotify);
        }

        void ClearSelection()
        {
            var list = m_MarkerList.Markers;
            list.Selected = new List<SyncId>();
            var actions = m_MarkerList.Actions;
            actions.MarkersSelected = 0;

            m_MarkerList.Markers = list;
            m_MarkerList.Actions = actions;
            m_MarkerListContextTarget.UpdateWith(ref m_MarkerList);
        }

        public IEnumerable<IContextPropertyProvider.ContextPropertyData> GetProperties()
        {
            var markerEditBag = PropertyBag.GetPropertyBag(m_MarkerEdit.GetType()) as PropertyBag<MarkerEditViewModel>;
            if (markerEditBag != null)
            {
                foreach (var property in markerEditBag.GetProperties())
                {
                    var data = new IContextPropertyProvider.ContextPropertyData() { context = typeof(MarkerEditContext), property = property };
                    yield return data;
                }
            }

            var markerListBag = PropertyBag.GetPropertyBag(m_MarkerList.GetType()) as PropertyBag<MarkerListViewModel>;
            if (markerListBag != null)
            {
                foreach (var property in markerListBag.GetProperties())
                {
                    var data = new IContextPropertyProvider.ContextPropertyData() { context = typeof(MarkerListContext), property = property };
                    yield return data;
                }
            }

            var markerDragBag = PropertyBag.GetPropertyBag(m_MarkerDrag.GetType()) as PropertyBag<MarkerDraggableEditorViewModel>;
            if (markerDragBag != null)
            {
                foreach (var property in markerDragBag.GetProperties())
                {
                    var data = new IContextPropertyProvider.ContextPropertyData() { context =typeof(MarkerDraggableEditorContext), property = property };
                    yield return data;
                }
            }

            var markerARModeBag = PropertyBag.GetPropertyBag(m_MarkerDrag.GetType()) as PropertyBag<MarkerARModeViewModel>;
            if (markerARModeBag != null)
            {
                foreach (var property in markerARModeBag.GetProperties())
                {
                    var data = new IContextPropertyProvider.ContextPropertyData() { context = typeof(MarkerARModeContext), property = property };
                    yield return data;
                }
            }
        }

        private void InvokeOnDispatchMarkers(Payload<IViewerAction> viewerAction)
        {
            HasChanged = false;

            lock (m_SyncRoot)
            {
                if (viewerAction.ActionType.RequiresContext(MarkerEditContext.current, viewerAction.Data))
                    viewerAction.ActionType.ApplyPayload(viewerAction.Data, ref m_MarkerEdit, () =>
                    {
                        bool sendUpdate = false;

                        var editedMarker = m_MarkerEdit.ToMarker();
                        // Check if any of the fields are different from controller.
                        if (!PivotTransform.Similar(m_CachedMarker.RelativePosition, editedMarker.RelativePosition) ||
                            !PivotTransform.Similar(m_CachedMarker.RelativeRotationEuler, editedMarker.RelativeRotationEuler) ||
                            !PivotTransform.Similar(m_CachedMarker.ObjectScale, editedMarker.ObjectScale) ||
                            m_CachedMarker.Name != editedMarker.Name)
                        {
                            sendUpdate = true;
                        }

                        // Update drag handle position
                        if (sendUpdate)
                        {
                            m_MarkerController.Visualize(editedMarker);
                            m_UpdateDragHandle.Run(
                                ()=>m_DraggableMarkerPlacement.UpdatePose( editedMarker.GetWorldPose(m_MarkerController.AlignedObject.Get())),
                                0.1f);
                            m_CachedMarker = editedMarker;
                            m_MarkerEditContextTarget.UpdateWith(ref m_MarkerEdit);
                        }
                        HasChanged = true;
                    });
                if (viewerAction.ActionType.RequiresContext(MarkerListContext.current, viewerAction.Data))
                    viewerAction.ActionType.ApplyPayload(viewerAction.Data, ref m_MarkerList, () =>
                    {
                        m_MarkerListContextTarget.UpdateWith(ref m_MarkerList);
                        HasChanged = true;
                    });
            }
        }

        void OnMoveMarker()
        {
            if (m_MarkerController != null)
            {
                if (m_MarkerController.AlignedObject != null)
                {
                    m_DraggableMarkerPlacement.Open(
                        Marker.GetWorldPose(
                            m_MarkerController.AlignedObject.Get(),
                            new Vector3(m_MarkerEdit.X, m_MarkerEdit.Y, m_MarkerEdit.Z),
                            Quaternion.Euler(m_MarkerEdit.XAxis, m_MarkerEdit.YAxis, m_MarkerEdit.ZAxis)));
                }

            }
        }

        IEnumerator RunSoon(Action actionToRun)
        {
            yield return null;
            actionToRun?.Invoke();
        }
    }
}
