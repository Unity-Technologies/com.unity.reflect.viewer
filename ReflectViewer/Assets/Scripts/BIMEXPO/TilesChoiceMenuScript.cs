using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TilesChoiceMenuScript : MonoBehaviour
{
    public string chosenMaterial;
    public GameObject target;
    private Button okButton;

    /*
    void OnEnable()
    {
        //Create the menu
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        okButton = rootVisualElement.Q<Button>("ok-button");
        okButton.RegisterCallback<ClickEvent>(ev => ApplyMaterial());
        okButton.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
        okButton.AddToClassList("ok-but");

        VisualElement imgContainer = rootVisualElement.Q<VisualElement>("img-container");
        imgContainer.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
        imgContainer.AddToClassList("medaillon-container");

        Label menuTitle = rootVisualElement.Q<Label>("menu-title");
        menuTitle.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
        menuTitle.AddToClassList("title-menu");

        PopulateMenu();
    }

    /// <summary>
    /// Changes the visual aspect of the tile that is selected in the menu.
    /// </summary>
    /// <param name="name"></param>
    public void SelectMaterial(string name)
    {
        chosenMaterial = name;

        //Change visual aspect of the chosen material
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        VisualElement myBox = rootVisualElement.Q<VisualElement>("img-container");
        IEnumerable<VisualElement> tiles = myBox.Children();
        foreach (VisualElement tile in tiles)
        {
            if (tile.name == name)
            {
                tile.RemoveFromClassList("medaillon");
                tile.AddToClassList("selected-medaillon");
            }
            else
            {
                tile.RemoveFromClassList("selected-medaillon");
                tile.AddToClassList("medaillon");
            }
        }
    }

    /// <summary>
    /// Validates the choice of a tile on a given surface. This will save the choice in the DB, and close the menu until another surface is right-clicked by the user.
    /// </summary>
    void ApplyMaterial()
    {
        var loadTxtScript = GameObject.Find("FirstPersonController").GetComponent<DBInteractions>();
        string path = @"c:\users\aca\Documents\Projects\BIMEXPO\pictures_carrelages\" + loadTxtScript.GetTexturePathFromName(chosenMaterial);
        loadTxtScript.changeMaterial(target, path);
        //Get the ID of the surface from Metadata script
        string surfaceId = target.GetComponent<dummyMetadataScript>().ID.ToString();
        //Get the tile ID from its name
        string tileId = loadTxtScript.GetTileIdFromName(chosenMaterial);
        //Save the user choice to DB
        loadTxtScript.SaveUserChoiceToDB(tileId, surfaceId);

        //Close menu
        GameObject.Find("TileChoiceMenu").SetActive(false);

        //Reactivate player camera rotation
        GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = true;
    }

    void PopulateMenu()
    {
        //Recuperate the list of selected tiles - from DB
        List<string> selectedTiles = new List<string>();
        selectedTiles = GameObject.FindGameObjectWithTag("Player").GetComponent<DBInteractions>().RetrievePreselectedTiles();

        //Populate the menu with this selection
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        VisualElement myBox = rootVisualElement.Q<VisualElement>("img-container");
        Button okButton = rootVisualElement.Q<Button>("ok-button");

        //The menu should only suggest tiles for walls or for ground, depending on what was hit
        List<string> filteredList = new List<string>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<MenusHandler>().hitSurface;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (target.layer == 6)
            filteredList = player.GetComponent<DBInteractions>().FilterWallsOnlyFromTileList(selectedTiles);
        else
            filteredList = player.GetComponent<DBInteractions>().FilterSlabsOnlyFromTileList(selectedTiles);

        string texturePath;
        foreach (string tileName in filteredList)
        {
            texturePath = player.GetComponent<DBInteractions>().GetTexturePathFromName(tileName);
            var myVE = new VisualElement();
            myVE.styleSheets.Add(Resources.Load<StyleSheet>("USS/testVE"));
            myVE.AddToClassList("medaillon");
            myVE.style.backgroundImage = player.GetComponent<DBInteractions>().LoadTextureFromDisk(@"c:\users\aca\Documents\Projects\BIMEXPO\pictures_carrelages\" + texturePath);
            myVE.name = tileName;
            myBox.Add(myVE);
            myVE.RegisterCallback<ClickEvent>(ev => SelectMaterial(tileName));
        }
    }
    */
}
