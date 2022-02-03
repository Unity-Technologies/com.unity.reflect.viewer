using UnityEngine;
using UnityEngine.UI;

namespace Unity.VisuaLive.Markers.UI
{
    public class PopoutToggleUI : MonoBehaviour
    {
        [SerializeField] private Toggle toggle = null;
        [SerializeField] private Transform popoutArrowIcon = null;
        [SerializeField] private PopoutUI popoutUi = null;

        private Vector3 initialScale = Vector3.one;
        
        // Start is called before the first frame update
        void OnEnable()
        {
            initialScale = popoutArrowIcon.localScale;
            toggle.onValueChanged.AddListener(HandleOnValueChanged);
            popoutUi.OnToggled += UpdateGraphic;
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(HandleOnValueChanged);
            popoutUi.OnToggled -= UpdateGraphic;
        }

        void HandleOnValueChanged(bool value)
        {
            if (value)
                popoutUi.Open();
            else
                popoutUi.Close();
        }

        void UpdateGraphic(bool value)
        {
            Debug.Log("Graphic Updated");
            popoutArrowIcon.localScale = new Vector3(
                initialScale.x, 
                (value)?initialScale.y * -1 : initialScale.y,
                initialScale.z);
        }
    }
}
