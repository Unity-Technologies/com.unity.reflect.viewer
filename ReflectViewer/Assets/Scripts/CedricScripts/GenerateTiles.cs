using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateTiles : MonoBehaviour
{
    Material compMat = null;
    public Slider sliderWidth1; //These are all sliders of type float or double, mortar size in cm(?)
    public Slider sliderHeight1;
    public Slider sliderWidth2;
    public Slider sliderHeight2;
    public int resolutionTexture; //1024 is good as a standard
    public Color tileColor; //Color of tile
    public Color mortarColor; //Color of mortar
    Texture2D tileTexture;
    Texture2D tileTextureNormal;
    public Text textW1; //These texts are to be made empty, only used for output
    public Text textH1;
    public Text textW2;
    public Dropdown matDropdown; //Dropdown with two options, for now 'Tiles' and 'Bricks' for a normal tiled or brick tiled pattern. These options can be changed but also have to be changed in the GenerateTiles() function

    public void GenerateTilesFunction() //Generates a material of tiles according to sizes given and mortar size as well, including normal texture
    {
        float tileWidth = sliderWidth1.value; //These are the dimensions from the sliders
        float tileHeight = sliderHeight1.value;
        float mortarWidth = sliderWidth2.value;
        float mortarHeight = sliderHeight2.value;
        float totalWidth = tileWidth + mortarWidth;
        float totalHeight = tileHeight + mortarHeight;
        int totalWidthInt = resolutionTexture;
        int totalHeightInt = (int)Mathf.Floor(totalHeight / totalWidth * resolutionTexture);
        int mortarWidthInt = (int)Mathf.Floor(mortarWidth / totalWidth * resolutionTexture);
        int mortarHeightInt = (int)Mathf.Floor(mortarHeight / totalHeight * resolutionTexture);
        tileTexture = new Texture2D(totalWidthInt, totalHeightInt, TextureFormat.ARGB32, false); //Creates a new texture according to the sizes
        tileTextureNormal = new Texture2D(totalWidthInt, totalHeightInt, TextureFormat.ARGB32, false); //Creates new normal text
        float metallicness = 0;

        Debug.Log(totalWidthInt.ToString() + ", " + totalHeightInt.ToString());

        if (matDropdown.options[matDropdown.value].text.Equals("Tiles")) //Generate a normal tile material, not a bricked one
        {
            for (int i = 0; i < totalWidthInt; i++)
            {
                for (int j = 0; j < totalHeightInt; j++)
                {
                    if (i < (int)Mathf.Round(mortarWidthInt / 2f) || j < (int)Mathf.Round(mortarHeightInt / 2f) || i >= totalWidthInt - (int)Mathf.Round(mortarWidthInt / 2f) || j >= totalHeightInt - (int)Mathf.Round(mortarHeightInt / 2f))
                    //if (i < mortarWidthInt || j < mortarHeightInt)
                    {
                        tileTexture.SetPixel(i, j, mortarColor);
                        //tileTextureNormal.SetPixel(i, j, Color.black);
                    }
                    else
                    {
                        tileTexture.SetPixel(i, j, tileColor);
                        //tileTextureNormal.SetPixel(i, j, Color.white);
                    }
                }
            }
            metallicness = 0.85f;
            tileTextureNormal = Resources.Load<Texture2D>("Materials/normalMap_Tiles");//pregenerated normal map
        }
        else if (matDropdown.options[matDropdown.value].text.Equals("Bricks")) //Generate a brick material type
        {
            for (int i = 0; i < totalWidthInt; i++)
            {
                for (int j = 0; j < totalHeightInt; j++)
                {
                    //Outer borders
                    if ((i < (int)Mathf.Round(mortarWidthInt / 2f) && j <= totalHeightInt/2f) || j < (int)Mathf.Round(mortarHeightInt / 2f) || (i >= totalWidthInt - (int)Mathf.Round(mortarWidthInt / 2f) && j <= totalHeightInt / 2f) || j >= totalHeightInt - (int)Mathf.Round(mortarHeightInt / 2f))
                    {
                        tileTexture.SetPixel(i, j, mortarColor);
                        //tileTextureNormal.SetPixel(i, j, Color.black);
                    }
                    else if(j < totalHeightInt / 2f + (int)Mathf.Round(mortarHeightInt / 2f) && j > totalHeightInt / 2f - (int)Mathf.Round(mortarHeightInt / 2f)) //middle horizontal
                    {
                        tileTexture.SetPixel(i, j, mortarColor);
                    }
                    else if(j > totalHeightInt / 2f && i < totalWidthInt / 2f + (int)Mathf.Round(mortarWidthInt / 2f) && i > totalWidthInt / 2f - (int)Mathf.Round(mortarWidthInt / 2f))
                    {
                        tileTexture.SetPixel(i, j, mortarColor);
                    }
                    else
                    {
                        tileTexture.SetPixel(i, j, tileColor);
                        //tileTextureNormal.SetPixel(i, j, Color.white);
                    }
                }
            }
            metallicness = 0.0f;
            tileTextureNormal = Resources.Load<Texture2D>("Materials/normalMap_Bricks"); //pregenerated normal map
        }
        tileTexture.Apply();
        //tileTextureNormal.Apply();

        textW1.text = "Tile width: " + string.Format("{0:N3}", tileWidth) + "m";
        textH1.text = "Tile height: " + string.Format("{0:N3}", tileHeight) + "m";
        textW2.text = "Mortar width: " + string.Format("{0:N3}", mortarWidth) + "m";

        compMat.mainTexture = tileTexture;
        compMat.EnableKeyword("_NORMALMAP");
        compMat.SetTexture("_BumpMap", tileTextureNormal);
        compMat.mainTextureScale = new Vector2(1/totalWidth, 1/totalHeight);
        compMat.SetFloat("_Metallic", metallicness);
        //compMat.shader = Shader.Find("Diffuse");
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateTilesFunction(); //Generate it at the start of the program, to have one available
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
