using Unity.Properties.UI.Internal;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Properties.UI
{
    [CustomEditor(typeof(InspectorContent), false)]
    class InspectorContentEditor : UnityEditor.Editor 
    {
        class UpdateBinding : IBinding
        {
            public SerializableContent Content;
            public UnityEditor.Editor Editor;

            void IBinding.PreUpdate() { }

            void IBinding.Update()
            {
                Content.Update();
                if (!Content.IsValid)
                {
                    DestroyImmediate(Editor);
                }
                
                // We are saving here because we want to store the data inside the scriptable object window so that it
                // survives domain reloads (global selection is not persisted across Unity sessions).
                Content.Save();
            }

            void IBinding.Release() { }
        }
        
        internal InspectorContent Target => (InspectorContent) target;

        // Invoked by the Unity update loop
        protected override void OnHeaderGUI()
        {
            // Intentionally left empty.
        }
        
        //Invoked by the Unity loop
        public override bool UseDefaultMargins() => Target.Content.InspectionContext.UseDefaultMargins;

        // Invoked by the Unity update loop
        public override VisualElement CreateInspectorGUI()
        {
            Target.Root.binding = new UpdateBinding
            {
                Content = Target.Content,
                Editor = this
            }; 
            return Target.Root;
        }
    }
}