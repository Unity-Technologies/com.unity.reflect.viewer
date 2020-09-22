using System;

namespace UnityEngine.Reflect
{
    [CreateAssetMenu(fileName = nameof(BuildState), menuName = "ScriptableObjects/" + nameof(BuildState))]
    public class BuildState : ScriptableObject
    {
        public string bundleVersion;
        public string buildNumber;

        public override string ToString()
        {
            return $"bundleVersion : {bundleVersion}, buildNumber : {buildNumber}";
        }
    }
}
