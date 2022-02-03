namespace UnityEngine.Reflect.Viewer.Core
{
    [DefaultExecutionOrder(100000)]
    class UISelectorStats: MonoBehaviour
    {
#if UNITY_EDITOR && UI_SELECTOR_STATS
        [SerializeField]
        bool m_logOnSceneUnloaded;

        [SerializeField]
        bool m_logOnDestroy;

        void Awake()
        {
            SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void SceneManager_sceneUnloaded(SceneManagement.Scene scene)
        {
            SceneManagement.SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
            if (m_logOnSceneUnloaded)
            {
                Debug.Log($"UISELECTORS_STATS: {scene.name}.Unloading log began");
                Log();
                Debug.Log($"UISELECTORS_STATS: {scene.name}.Unloading log ended");
            }
        }

        void OnDestroy()
        {
            if (m_logOnDestroy)
            {
                Debug.Log($"UISELECTORS_STATS: {gameObject.scene.name}.{gameObject.name} OnDestroy log began");
                Log();
                Debug.Log($"UISELECTORS_STATS: {gameObject.scene.name}.{gameObject.name} OnDestroy log ended");
            }
        }

        void Log()
        {
            Debug.Log($"UISELECTORS_STATS: Not disposed {UISelectorFactory.CreatedSelectors.Count}");
            foreach (var item in UISelectorFactory.CreatedSelectors)
            {
                Debug.Log($"UISELECTORS_ {item.Key}, {item.Value.Item1}, {item.Value.Item2} is not disposed");
            }
        }
#endif
    }
}
