using SharpFlux.Dispatching;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldBlockMove : MonoBehaviour
    {
        TMP_InputField m_InputField;

        void Awake()
        {
            m_InputField = GetComponent<TMP_InputField>();
            m_InputField.onSelect.AddListener(OnSelect);
            m_InputField.onDeselect.AddListener(OnDeselect);
            m_InputField.onEndEdit.AddListener(OnEndEdit);
        }

        void OnEndEdit(string text)
        {
            var eventSystem = EventSystem.current;
            if (!eventSystem.alreadySelecting)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        void OnSelect(string text)
        {
            SetNavigationMoveEnabled(false);
        }

        void OnDeselect(string text)
        {
            SetNavigationMoveEnabled(true);
        }

        void SetNavigationMoveEnabled(bool enable)
        {
            Dispatcher.Dispatch(SetMoveEnabledAction.From(enable));
        }
    }
}
