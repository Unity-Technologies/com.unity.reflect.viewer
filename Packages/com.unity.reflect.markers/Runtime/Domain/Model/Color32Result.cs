using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Color32Result
{
    private Color32[] colorArr;
    private Vector2 size;

    public Color32[] ColorArr => colorArr;
    public Vector2 Size => size;
    
    public Color32Result(Color32[] arr, float w, float h)
    {
        colorArr = arr;
        size = new Vector2(w,h);
    }

}
