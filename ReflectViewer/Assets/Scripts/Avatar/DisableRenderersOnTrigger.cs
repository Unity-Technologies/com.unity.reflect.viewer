using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reflect.Viewer
{
    public class DisableRenderersOnTrigger : MonoBehaviour
    {
        public string triggerTag;
        public Renderer[] renderers;

        [ContextMenu("Assign Child Renderers")]
        public void AssignChildRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag(triggerTag))
            {
                foreach(var renderer in renderers)
                {
                    renderer.enabled = false;
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(triggerTag))
            {
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                }
            }
        }
    }

}
