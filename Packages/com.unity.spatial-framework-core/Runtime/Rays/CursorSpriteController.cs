using UnityEngine;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Controls a ray cursor's sprite based on the ray interactor state.
    /// </summary>
    public class CursorSpriteController : MonoBehaviour
    {
        [SerializeField, Tooltip("The ray interactor cursor")]
        RayInteractionCursor m_RayInteractorRenderer;

        [SerializeField, Tooltip("The sprite renderer to control")]
        SpriteRenderer m_CursorSprite;

        [SerializeField, Tooltip("The sprite to show when the interactor is selecting something")]
        Sprite m_SelectingSprite;

        [SerializeField, Tooltip("The sprite to show when the interactor is hovering something")]
        Sprite m_HoveringSprite;

        /// <summary>
        /// The ray interactor cursor
        /// </summary>
        public RayInteractionCursor rayInteractorRenderer
        {
            get => m_RayInteractorRenderer;
            set => m_RayInteractorRenderer = value;
        }

        /// <summary>
        /// The sprite renderer to control
        /// </summary>
        public SpriteRenderer cursorSprite
        {
            get => m_CursorSprite;
            set => m_CursorSprite = value;
        }

        /// <summary>
        /// The sprite to show when the interactor is selecting something
        /// </summary>
        public Sprite selectingSprite
        {
            get => m_SelectingSprite;
            set => m_SelectingSprite = value;
        }

        /// <summary>
        /// The sprite to show when the interactor is hovering something
        /// </summary>
        public Sprite hoveringSprite
        {
            get => m_HoveringSprite;
            set => m_HoveringSprite = value;
        }

        void LateUpdate()
        {
            var uiSelect = false;
            if (m_RayInteractorRenderer.rayInteractor != null)
            {
                if (m_RayInteractorRenderer.rayInteractor.TryGetUIModel(out var model))
                    uiSelect = model.select;

                var selecting = m_RayInteractorRenderer.rayInteractor.selectTarget != null || uiSelect;

                //TODO implement animate/tween between cursors (scale in//out probably)
                m_CursorSprite.sprite = selecting ? m_SelectingSprite : m_HoveringSprite;

                //TODO implement object-custom cursors and other cursor states
            }
        }
    }
}
