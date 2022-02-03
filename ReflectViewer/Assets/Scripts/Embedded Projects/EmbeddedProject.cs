using System;
using Unity.Reflect;

namespace UnityEngine.Reflect.Viewer
{
    public class EmbeddedProject : Project
    {
        readonly EmbeddedProjectData m_Data;

        public override string name => m_Data.name;
        public override string description => "Embedded";
        public override DateTime lastPublished => DateTime.Now;

        public Sprite thumbnailOverride => m_Data.thumbnail;

        public GameObject prefab => m_Data.prefab;

        public EmbeddedProject(EmbeddedProjectData projectData) : base(new UnityProject(projectData.name, projectData.name))
        {
            m_Data = projectData;
        }
    }
}

