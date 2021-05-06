using UnityEngine.UIElements;
using UnityEngine;

public class CommentMenuScript : MonoBehaviour
{
    private Button validateButton;
    private TextField txtField;
    private GameObject target;

    /*
    void OnEnable()
    {
        //Register the action on button click
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<MenusHandler>().hitSurface;
        
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        validateButton = rootVisualElement.Q<Button>("ok-button");
        txtField = rootVisualElement.Q<TextField>("txtField");
        txtField.label = target.name;
        validateButton.RegisterCallback<ClickEvent>(ev => saveComment(target));

        //TODO : recuperate the comment if one already exists
    }

    void saveComment(GameObject target)
    {
        string comment = txtField.text;
        var DBScript = GameObject.Find("FirstPersonController").GetComponent<DBInteractions>();
        DBScript.saveComment(comment, target);
        GameObject.Find("CommentMenu").SetActive(false);
    }
    */
}
