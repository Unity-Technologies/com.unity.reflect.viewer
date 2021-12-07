using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [CreateAssetMenu(fileName = "TransformVariable", menuName = "ScriptableObjects/Variable/" + nameof(TransformVariable))]
    public class TransformVariable : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string Description = "";
#endif

        public Transform Value;

        public void SetValue(Transform value)
        {
            Value = value;
        }

        public void SetValue(TransformVariable value)
        {
            Value = value.Value;
        }
    }
}
