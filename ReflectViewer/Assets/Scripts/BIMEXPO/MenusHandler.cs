using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;

public class MenusHandler : MonoBehaviour
{
    public GameObject hitSurface = null;    // The surface clicked by the user.
    UnityEvent m_MyEvent = new UnityEvent();
    //private bool listenerSet = false;

    /*
    /// <summary>
    /// Sets the Preselection Menu active, so that it appears on screen.
    /// </summary>
    public void ActivatePreselectionMenu()
    {
        try
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<DBInteractions>().Connect_DB();
        }
        catch
        {
            return;
        }
            
        //Show preselection menu
        GameObject[] allGO = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allGO)
        {
            if (go.name == "PreselectionMenu")
                go.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit; // Infos about the hit

            //Filter walls and slabs only
            int layerMask = LayerMask.GetMask("Walls", "Slabs");

            //Shoot the ray towards the mouse position
            Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayToMouse, out hit, Mathf.Infinity, layerMask))
            {
                hitSurface = hit.collider.gameObject;
                HighlightSurface(hitSurface);
                ActivateTilesChoiceMenu();

                //Adding event listener so that if user clicks middle button when surface is selected, comment box appears
                m_MyEvent.AddListener(ActivateCommentMenu);
                //listenerSet = true;
            }
            else
            {
                Debug.Log("No hit");
                m_MyEvent.RemoveAllListeners();
            }
        }
        if (Input.GetMouseButtonDown(2) && m_MyEvent != null)
        {
            //Begin the action
            m_MyEvent.Invoke();
        }
    }

    /// <summary>
    /// Sets the Tile choice menu active, so that it appears on screen. This also freezes the player camera so that as long as this menu is up, moving the mouse doesn't change the perspective.
    /// </summary>
    void ActivateTilesChoiceMenu()
    {
        GameObject[] allGO = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject tileChoiceMenu = null;
        foreach (GameObject go in allGO)
        {
            if (go.name == "TileChoiceMenu")
            {
                //Disable player camera rotation until the preselection is made
                GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = false;

                //Show menu
                go.SetActive(true);
                tileChoiceMenu = go;
                break;
            }
        }
    }

    void HighlightSurface(GameObject surf)
    {
        //surf.GetComponent<Material>().SetColor("_Color", Color.red);
        surf.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    void ActivateCommentMenu()
    {
        GameObject[] allGO = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject commentMenu = null;
        foreach (GameObject go in allGO)
        {
            if (go.name == "CommentMenu")
            {
                //Disable player camera rotation until the preselection is made
                GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>().cameraCanMove = false;

                //Show menu
                go.SetActive(true);
                commentMenu = go;
                break;
            }
        }
    }
    */
}
