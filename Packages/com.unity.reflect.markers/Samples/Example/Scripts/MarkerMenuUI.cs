using Unity.Reflect.Markers.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.UI
{
    public class MarkerMenuUI : MonoBehaviour
    {
        [SerializeField] private Button quickScanButton = null;
        [SerializeField] private QuickScanController _quickScanController = null;

        [SerializeField] private Button createMarkerButton = null;

        [SerializeField] private Button selectMarkerButton = null;
        
        // Start is called before the first frame update
        void Start()
        {   
            quickScanButton.onClick.AddListener(_quickScanController.Run);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
