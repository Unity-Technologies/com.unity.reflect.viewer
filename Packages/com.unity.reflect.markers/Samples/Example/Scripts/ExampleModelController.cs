using System;
using System.Collections.Generic;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

/// <summary>
/// Example Model/Marker controller.
/// </summary>
public class ExampleModelController : MonoBehaviour
{
    
    [SerializeField] private GameObject exampleModelPrefab = null;

    private GameObject spawnedModel = null;
    public GameObject Model
    {
        get 
        {
            if (!spawnedModel)
               spawnedModel = Instantiate(exampleModelPrefab);
            return spawnedModel;
        }
    }
    
}
