using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using InspectorElement = Unity.Properties.UI.InspectorElement;

namespace Unity.Properties.Editor
{
    [CustomEditor(typeof(PropertyBehaviour), true)]
    class PropertyBehaviourEditor : UnityEditor.Editor
    {
        InspectorElement m_RootElement;
        PropertyBehaviour Target => target as PropertyBehaviour;

        void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void OnUndoRedo()
        {
            Target.Load();
            m_RootElement?.SetTarget(target);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new BindableElement();
            var monoScript = new ObjectField("Script")
            {
                value = MonoScript.FromMonoBehaviour(Target)
            };
            monoScript.Q<Label>().style.paddingLeft = 0;
            monoScript.Q(className: "unity-object-field__selector").SetEnabled(false);
            monoScript.RegisterCallback<ChangeEvent<UnityEngine.Object>, ObjectField>(
                (evt, element) => element.value = evt.previousValue, monoScript);

            root.contentContainer.Add(monoScript);
            m_RootElement = new InspectorElement();
            m_RootElement.RegisterCallback<AttachToPanelEvent, (InspectorElement inspector, PropertyBehaviour target)>((evt, ctx) =>
            {
                ctx.inspector.SetTarget(ctx.target);
            }, (m_RootElement, Target));
            m_RootElement.OnChanged += (element, path) => { Target.Save(); };
            root.contentContainer.Add(m_RootElement);
            root.AddToClassList("unity-inspector-element");
            StylingUtility.AlignInspectorLabelWidth(root);
            root.RegisterCallback<GeometryChangedEvent, BindableElement>(OnGeometryChanged, root);

            return root;
        }

        static void OnGeometryChanged(GeometryChangedEvent evt, BindableElement root)
        {
            StylingUtility.AlignInspectorLabelWidth(root);
        }
    }
}
