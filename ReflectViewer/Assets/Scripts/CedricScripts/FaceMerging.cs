using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Reflect;
using System.IO;

public class FaceMerging : MonoBehaviour
{
    public GameObject selectedObject; //Leave empty as usual
    List<Material> matPoss;
    List<string> namePoss;
    public Text textCosts; //Empty text object
    int curScenario1 = 1;
    double curCost1 = 0.00;
    double totArea1 = 0;
    GameObject root;
    ChangeMaterial changeMatScript;
    public Material defMat; //default material that gets used when adding/removing objects from merged list

    public List<GameObject> listCustom; //To be left empty
    public List<List<GameObject>> listOfListCustom; //The list of all merged lists, starts empty but is built upon automatically, start empty
    public int curList; //The index of the current list in listOfListCustom (yes, kinda confusing...), start empty too or at 0

    // Start is called before the first frame update
    void Start()
    {
        root = GameObject.Find("Root");
        changeMatScript = root.GetComponent<ChangeMaterial>();

        listCustom = new List<GameObject>();
        listOfListCustom = new List<List<GameObject>>();
        listOfListCustom.Add(listCustom);
        curList = 0;

    }

    // Update is called once per frame
    void Update()
    {
        selectedObject = changeMatScript.selectedObject;
        var test = selectedObject.GetComponent<Metadata>().GetParameter("Area");
        double curArrea = 0.0;
        if (test.Split()[0].Length > 0)
        {
            curArrea = double.Parse(test.Split()[0], System.Globalization.CultureInfo.InvariantCulture);
        }
        namePoss = new List<string>();
        curCost1 = 0.0;
        totArea1 = 0.0;
        if (listCustom.Count >= 1) //If the current merged list is not empty
        {
            foreach (GameObject go in listCustom)
            {
                matPoss = changeMatScript.CreateUINew(go, 0); //Get the possible materials from changeMatScript
                if (matPoss.Count >= 1)
                {
                    foreach (Material mat in matPoss)
                    {
                        namePoss.Add(mat.name);
                        namePoss.Add(mat.name + " (Instance)");
                    }
                    if (namePoss.Contains(go.GetComponent<MeshRenderer>().sharedMaterial.name))
                    { 
                        curCost1 += double.Parse(go.GetComponent<Metadata>().GetParameter("Area").Split()[0], System.Globalization.CultureInfo.InvariantCulture) * 20.0;
                    }
                }
                test = go.GetComponent<Metadata>().GetParameter("Area");
                totArea1 += double.Parse(test.Split()[0], System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        textCosts.text = "Area is " + curArrea.ToString() + "\nThe price of scenario " + curScenario1 + " is " + curCost1.ToString() + "\nSelected area is " + totArea1;// + "\nThe price of scenario " + curScenario2 + " is " + curCost2.ToString() + "\nSelected area is " + totArea2 + "\nTotal area: " + (totArea1 + totArea2) +"\nTotal cost: " + (curCost1 + curCost2).ToString();
        //Generates script of costs
        if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)) //right click and ctrl and alt; ADD NEW CUSTOMLIST!!!
        {
            Debug.Log("ctrl alt");
            listOfListCustom.Add(new List<GameObject>());
            curList = listOfListCustom.Count-1;
            listOfListCustom[curList].Add(selectedObject);
        }
        else if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftControl)) //right click and ctrl, ADD TO OR REMOVE FROM CUSTOMLIST
        {
            if (!listOfListCustom[curList].Contains(selectedObject)) //If current list doesn't dontain the object, add it
            {
                listOfListCustom[curList].Add(selectedObject);
                foreach (GameObject go in listOfListCustom[curList])
                {
                    changeMatScript.ChangeMaterialClick(defMat, go);
                }
            }
            for (int i = 0; i < listOfListCustom.Count; i++) //Remove it in any other list
            {
                if (listOfListCustom[i].Contains(selectedObject))
                {
                    listOfListCustom[i].Remove(selectedObject);
                    foreach (GameObject go in listOfListCustom[i])
                    {
                        changeMatScript.ChangeMaterialClick(defMat, go);
                    }
                }
            }
        }   
        
        if(changeMatScript.functionReplaceCalled == true) //When the function to replace a material is called anywhere, check if the object is part of any merge list and if so change materials on all of them
        {
            for (int i = 0; i < listOfListCustom.Count; i++)
            {
                if (listOfListCustom[i].Contains(selectedObject))
                {
                    foreach (GameObject go in listOfListCustom[i])
                    {
                        changeMatScript.ChangeMaterialClick(selectedObject.GetComponent<Renderer>().material, go);
                    }
                }
            }
            changeMatScript.functionReplaceCalled = false;
        }
    }

    void OnApplicationQuit() //When Unity halts, create a new CSV file
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        CreateCSV("Test");
    }
    void CreateCSV(string fileName) //Create CSV file: path, attributes. Reset file and fill it
    {

        string path = "C:/Users/cdri/Documents" + "/" + fileName + ".csv";
        File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);  // to put in try/catch if file doesn't exist
        if (File.Exists(path))
        {
            File.WriteAllText(path, String.Empty);
            //File.Delete(path);
        }

        var sr = File.CreateText(path);

        string data = textCosts.text;

        sr.WriteLine(data);

        FileInfo fInfo = new FileInfo(path);
        fInfo.IsReadOnly = true;

        sr.Close();

        //Application.OpenURL(path);
    }

}
