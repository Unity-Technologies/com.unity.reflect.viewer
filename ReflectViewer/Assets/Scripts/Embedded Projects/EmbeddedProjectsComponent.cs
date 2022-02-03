using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Reflect.Viewer
{
    public class EmbeddedProjectsComponent : MonoBehaviour
    {
        [SerializeField][FormerlySerializedAs("m_BuiltInProjectData")]
        EmbeddedProjectData[] m_EmbeddedProjectData;

        public IEnumerable<EmbeddedProjectData> projectsData => m_EmbeddedProjectData;
    }

    [Serializable]
    public struct EmbeddedProjectData
    {
        public string name;
        public Sprite thumbnail;
        public GameObject prefab;

    }
}
