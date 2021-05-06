using UnityEngine;
using UnityEngine.UIElements;

public class ValidateSelectionsScript : MonoBehaviour
{
    private Button validateButton;

    /*
    void OnEnable()
    {
        //Register the action on button click
        var DBScript = GameObject.Find("FirstPersonController").GetComponent<DBInteractions>();
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        validateButton = rootVisualElement.Q<Button>("ok-button");
        validateButton.RegisterCallback<ClickEvent>(ev => DBScript.produceAvenant());
    }
    */
}
