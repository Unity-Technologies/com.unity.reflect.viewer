using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GUIZTestAttribute : MonoBehaviour
{
    public CompareFunction CompareFunction = CompareFunction.Equal;

    void Start()
    {
        var graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            var material = graphic.materialForRendering;
            if (material != null)
            {
                var materialCopy = new Material(material);
                materialCopy.SetInt("unity_GUIZTestMode", (int) CompareFunction);
                graphic.material = materialCopy;
            }
        }
    }
}
