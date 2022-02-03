using Unity.Properties.UI;
using UnityEditor;
using UnityEngine;

namespace Unity.Properties.Debug
{
    partial class Explorer
    {
        [GeneratePropertyBag]
        public class Provider : ContentProvider
        {
            [MenuItem("internal:Window/Properties/Property Bag Explorer")]
            static void Show()
            {
                SelectionUtility.ShowInWindow(new Provider(),
                    new ContentWindowParameters
                    {
                        ApplyInspectorStyling = false,
                        AddScrollView = false,
                        MinSize = new Vector2(500, 400)
                    });
            }

            [CreateProperty] public readonly Explorer.Context Context = new Explorer.Context();

            public override string Name { get; } = "Property Bag Explorer";

            public override object GetContent()
            {
                return new Explorer(this);
            }
        }
    }
}
