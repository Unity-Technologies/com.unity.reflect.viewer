namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class InstancesPerFrameLimiter : MonoBehaviour
    {
        [SerializeField] Camera m_Camera = null;
        [SerializeField] float m_TargetFrameRate = 30;
        [SerializeField] float m_TargetFrameRateStatic = 15;
        [SerializeField] int m_InitialMaxInstancesPerFrame = 10;
        [SerializeField] int m_StaticChangePerFrame = 10;
        [SerializeField] int m_LowerLimit = 10;
        [SerializeField] int m_UpperLimit = 200;


        Transform m_CameraTransform;
        bool m_IsMoving;

        public int maxInstancesPerFrame { get; private set; }
        public bool isBelowTargetDeltaTime { get; private set; }

        void Start()
        {
            m_CameraTransform = m_Camera.transform;
            maxInstancesPerFrame = m_InitialMaxInstancesPerFrame;
        }

        void Update()
        {
            m_IsMoving = m_CameraTransform.hasChanged;
            isBelowTargetDeltaTime = Time.deltaTime < 1f / (m_IsMoving ? m_TargetFrameRate : m_TargetFrameRateStatic);

            if (m_IsMoving)
            {
                maxInstancesPerFrame = isBelowTargetDeltaTime ? Mathf.Min(m_UpperLimit, maxInstancesPerFrame + 1) : m_LowerLimit;
                m_CameraTransform.hasChanged = false;
                return;
            }

            maxInstancesPerFrame = Mathf.Clamp(isBelowTargetDeltaTime
                ? maxInstancesPerFrame + m_StaticChangePerFrame
                : maxInstancesPerFrame - m_StaticChangePerFrame, m_LowerLimit, m_UpperLimit);
        }

        public void Reset()
        {
            maxInstancesPerFrame = m_InitialMaxInstancesPerFrame;
        }
    }
}
