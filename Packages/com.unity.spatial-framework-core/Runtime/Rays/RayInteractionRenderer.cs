using System;
using Unity.SpatialFramework.Rendering;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// A base class for controlling visuals for an XR ray interactor.
    /// </summary>
    public abstract class RayInteractionRenderer : MonoBehaviour, IUsesViewerScale
    {
        /// <summary>
        /// State of visibility, including transition states.
        /// </summary>
        public enum Visibility
        {
            Invisible,
            FadingIn,
            Visible,
            FadingOut,
        }

        /// <summary>
        /// Serializable class for a Unity boolean event
        /// </summary>
        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        [SerializeField, Tooltip("The Ray Interactor to render")]
        protected XRRayInteractor m_RayInteractor;

        [SerializeField, Tooltip("Whether visuals should be hidden while the target interactor is not hovering over anything.")]
        bool m_HideIfNotHovering;

        [SerializeField, Tooltip("Whether visuals are shown by fading in.")]
        bool m_FadeInOnShow;

        [SerializeField, Tooltip("The length of time that fading in will take.")]
        float m_FadeInDuration = 0.2f;

        [SerializeField, Tooltip("Whether visuals are hidden by fading out.")]
        bool m_FadeOutOnHide;

        [SerializeField, Tooltip("The length of time that fading out will take.")]
        float m_FadeOutDuration = 0.2f;

        [SerializeField, Tooltip("The default length of the ray interaction line.")]
        protected float m_DefaultLineLength = 5f;

        /// <summary>
        /// An event that is fired when the renderer changes visibility. The boolean value is true when shown, and false when hidden.
        /// </summary>
        public BoolEvent onShow = new BoolEvent();

        float m_CurrentRayLength;
        FadeHighlight m_FadeHighlight;
        FadeUIHighlight m_FadeUIHighlight;
        Transform m_SelectedObjectTransform;
        Vector3 m_CurrentHitOrSelectPoint;
        Vector3 m_CurrentHitNormal;
        bool m_Hovering;
        Vector3 m_ObjectLocalSelectPoint;
        Transform m_MainCameraTransform;

        /// <summary>
        /// The ray interactor that is used when updating visuals.
        /// </summary>
        public XRRayInteractor rayInteractor
        {
            get => m_RayInteractor;
            set
            {
                if (value != m_RayInteractor)
                {
                    if (m_RayInteractor != null)
                        UnbindToInteractor(m_RayInteractor);
                    m_RayInteractor = value;
                    BindToInteractor(m_RayInteractor);
                }
            }
        }

        /// <summary>
        /// Length (in meters) of the current target ray interactor. This is equivalent to the hit distance of the interactor's
        /// current raycast result if there was a game object hit, or the interactor's maximum raycast distance if there
        /// was not a game object hit.
        /// </summary>
        public float currentRayLength
        {
            get => m_CurrentRayLength;
            private set => m_CurrentRayLength = Mathf.Max(0f, value);
        }

        /// <summary>
        /// If the interactor is currently selecting, then this is the point on the selected object where the ray end point was when the selection started. Otherwise this is the current end point of the ray.
        /// </summary>
        public Vector3 CurrentHitOrSelectPoint => m_CurrentHitOrSelectPoint;

        /// <summary>
        /// The normal of the surface at the point where the ray is currently hitting.
        /// </summary>
        public Vector3 CurrentHitNormal => m_CurrentHitNormal;

        /// <summary>
        /// The current state of visibility.
        /// </summary>
        public Visibility visibility { get; private set; }

        /// <summary>
        /// Whether visuals should be hidden while the target interactor is not hovering over anything.
        /// </summary>
        public bool hideIfNotHovering
        {
            get => m_HideIfNotHovering;
            set
            {
                m_HideIfNotHovering = value;
                if (!m_HideIfNotHovering)
                    Show(true);
                else if (!m_Hovering)
                    Show(false);
            }
        }

        /// <summary>
        /// Whether visuals are shown by fading in.
        /// </summary>
        public bool fadeInOnShow
        {
            get => m_FadeInOnShow;
            set => m_FadeInOnShow = value;
        }

        /// <summary>
        /// The length of time that fading in will take.
        /// </summary>
        public float fadeInDuration
        {
            get => m_FadeInDuration;
            set
            {
                m_FadeInDuration = value;
                if (m_FadeHighlight != null)
                    m_FadeHighlight.fadeInDuration = m_FadeInDuration;
                if (m_FadeUIHighlight != null)
                    m_FadeUIHighlight.fadeInDuration = m_FadeInDuration;
            }
        }

        /// <summary>
        /// Whether visuals are shown by fading out.
        /// </summary>
        public bool fadeOutOnHide
        {
            get => m_FadeOutOnHide;
            set => m_FadeOutOnHide = value;
        }

        /// <summary>
        /// The length of time that fading out will take.
        /// </summary>
        public float fadeOutDuration
        {
            get => m_FadeOutDuration;
            set
            {
                m_FadeOutDuration = value;
                if (m_FadeHighlight != null)
                    m_FadeHighlight.fadeOutDuration = m_FadeOutDuration;
                if (m_FadeUIHighlight != null)
                    m_FadeUIHighlight.fadeOutDuration = m_FadeOutDuration;
            }
        }

        /// <summary>
        /// Reference to the current selected object transform
        /// </summary>
        public Transform selectedObjectTransform => m_SelectedObjectTransform;

        /// <summary>
        /// Sets an override ray length of the ray renderer that can be less than the actual current length
        /// </summary>
        public float? overrideRayLength { get; set; }

        IProvidesViewerScale IFunctionalitySubscriber<IProvidesViewerScale>.provider { get; set; }

        protected virtual void Awake()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
                m_MainCameraTransform = mainCamera.transform;

            var hasRenderer = GetComponentInChildren<Renderer>() != null;
            var hasCanvasRenderer = GetComponentInChildren<CanvasRenderer>() != null;
            if (!hasRenderer && !hasCanvasRenderer)
            {
                Debug.LogError("No renderer found in hierarchy");
                enabled = false;
                return;
            }

            if (hasRenderer)
            {
                m_FadeHighlight = ComponentUtils.GetOrAddIf<FadeHighlight>(gameObject, true);
                m_FadeHighlight.rendererCaptureDepth = RendererCaptureDepth.AllChildRenderers;
                m_FadeHighlight.fadeAmount = 0f;
                m_FadeHighlight.fadeInDuration = fadeInDuration;
                m_FadeHighlight.fadeOutDuration = fadeOutDuration;
                m_FadeHighlight.onFadeInFinished.AddListener(OnFadeInFinished);
                m_FadeHighlight.onFadeOutFinished.AddListener(OnFadeOutFinished);
            }

            if (hasCanvasRenderer)
            {
                m_FadeUIHighlight = ComponentUtils.GetOrAddIf<FadeUIHighlight>(gameObject, true);
                m_FadeUIHighlight.rendererCaptureDepth = RendererCaptureDepth.AllChildRenderers;
                m_FadeUIHighlight.fadeAmount = 0f;
                m_FadeUIHighlight.fadeInDuration = fadeInDuration;
                m_FadeUIHighlight.fadeOutDuration = fadeOutDuration;
                m_FadeUIHighlight.onFadeInFinished.AddListener(OnFadeInFinished);
                m_FadeUIHighlight.onFadeOutFinished.AddListener(OnFadeOutFinished);
            }
        }

        protected virtual void OnEnable()
        {
            if (hideIfNotHovering && !m_Hovering)
            {
                if (m_FadeHighlight != null)
                    m_FadeHighlight.Activate(true);
                if (m_FadeUIHighlight != null)
                    m_FadeUIHighlight.Activate(true);
            }
            else
                Show(true);

            if (m_RayInteractor != null)
                BindToInteractor(m_RayInteractor);
        }

        void BindToInteractor(XRBaseInteractor interactor)
        {
            interactor.selectEntered.AddListener(OnSelectEntered);
            interactor.selectExited.AddListener(OnSelectExited);
        }

        void UnbindToInteractor(XRBaseInteractor interactor)
        {
            interactor.selectEntered.RemoveListener(OnSelectEntered);
            interactor.selectExited.RemoveListener(OnSelectExited);
        }

        protected virtual void OnDisable()
        {
            m_Hovering = false;
            if (m_RayInteractor != null)
                UnbindToInteractor(m_RayInteractor);
        }

        protected virtual void LateUpdate()
        {
            // Update visuals in LateUpdate so they are not a frame behind.
            if (rayInteractor != null)
            {
                var linePoints = new Vector3[] { };
                ((ILineRenderable)rayInteractor).GetLinePoints(ref linePoints, out _);
                var rayOrigin = rayInteractor.attachTransform;
                var startPoint = rayOrigin.position;

                if (m_SelectedObjectTransform != null)
                {
                    m_CurrentHitOrSelectPoint = m_SelectedObjectTransform.TransformPoint(m_ObjectLocalSelectPoint);
                    currentRayLength = Vector3.Distance(m_CurrentHitOrSelectPoint, startPoint);
                }
                else
                {
                    var isValidTarget = UpdateCurrentHitInfo(rayOrigin);
                    m_Hovering = isValidTarget;

                    if (hideIfNotHovering)
                        Show(m_Hovering);
                }

                UpdateVisuals();
            }
        }

        bool UpdateCurrentHitInfo(Transform rayOrigin)
        {
            var startPoint = rayOrigin.position;
            if (rayInteractor.TryGetHitInfo(out m_CurrentHitOrSelectPoint, out m_CurrentHitNormal, out _, out var isValidTarget))
            {
                currentRayLength = Vector3.Distance(m_CurrentHitOrSelectPoint, startPoint);
            }
            else
            {
                var defaultLineLength = m_DefaultLineLength * this.TryGetViewerScale(m_MainCameraTransform);
                currentRayLength = overrideRayLength ?? defaultLineLength;
                var rayDirection = rayOrigin.forward;
                m_CurrentHitNormal = -rayDirection;
                m_CurrentHitOrSelectPoint = startPoint + rayDirection * currentRayLength;
            }

            return isValidTarget;
        }

        /// <summary>
        /// Updates visuals based on the state of the target ray interactor.
        /// Called every frame in which the target ray interactor is non-null.
        /// </summary>
        protected abstract void UpdateVisuals();

        protected virtual void OnSelectEntered(SelectEnterEventArgs args)
        {
            m_SelectedObjectTransform = args.interactable.transform;
            var rayOrigin = args.interactor.attachTransform;
            UpdateCurrentHitInfo(rayOrigin); // Update the hit info so that capture point is not behind by 1 frame
            m_ObjectLocalSelectPoint = m_SelectedObjectTransform.InverseTransformPoint(m_CurrentHitOrSelectPoint);
        }

        protected virtual void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactable == null || args.interactable.transform == m_SelectedObjectTransform)
            {
                m_SelectedObjectTransform = null;
            }
        }

        void Show(bool show)
        {
            if (show && visibility != Visibility.Visible && visibility != Visibility.FadingIn)
            {
                visibility = fadeInOnShow ? Visibility.FadingIn : Visibility.Visible;
                if (m_FadeHighlight != null)
                    m_FadeHighlight.Deactivate(!fadeInOnShow);
                if (m_FadeUIHighlight != null)
                    m_FadeUIHighlight.Deactivate(!fadeInOnShow);
            }
            else if (!show && visibility != Visibility.Invisible && visibility != Visibility.FadingOut)
            {
                visibility = fadeOutOnHide ? Visibility.FadingOut : Visibility.Invisible;
                if (m_FadeHighlight != null)
                    m_FadeHighlight.Activate(!fadeOutOnHide);
                if (m_FadeUIHighlight != null)
                    m_FadeUIHighlight.Activate(!fadeOutOnHide);
            }

            onShow?.Invoke(show);
        }

        void OnFadeInFinished()
        {
            visibility = Visibility.Visible;
        }

        void OnFadeOutFinished()
        {
            visibility = Visibility.Invisible;
        }
    }
}
