using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class UIContextContainer : MonoBehaviour, IContextContainer
    {
        public Transform GetInsertionPoint()
        {
            return transform;
        }
    }

    public interface IContextContainer
    {
        Transform GetInsertionPoint();
    }
}
