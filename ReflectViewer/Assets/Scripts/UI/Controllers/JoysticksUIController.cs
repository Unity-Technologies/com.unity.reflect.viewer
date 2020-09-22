using UnityEngine;
using Unity.TouchFramework;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible of managing the Joysticks UI
    /// </summary>
    [RequireComponent(typeof(DialogWindow))]
    public class JoysticksUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Button to toggle the joysticks.")]
        Button m_JoystickToggleButton;
        //[SerializeField, Tooltip("Reference to the Move component to manipulate.")]
        //Move m_Move;
        [SerializeField, Tooltip("Reference to the left JoystickControl.")]
        JoystickControl m_LeftJoystick;
        [SerializeField, Tooltip("Reference to the right JoystickControl.")]
        JoystickControl m_RightJoystick;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        bool m_Active;

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();
            UIStateManager.stateChanged += OnStateDataChanged;
        }
        
        void Start()
        {
            m_JoystickToggleButton.onClick.AddListener(OnJoystickToggleButtonClick);
            m_Active = true;
        }
        
        void OnStateDataChanged(UIStateData stateData)
        {
            if (stateData.activeDialog != DialogType.None)
            {
                if (m_DialogWindow.open)
                    m_Active = true;
                
                m_DialogWindow.Close();
            }
            else
            {
                if (m_Active)
                    m_DialogWindow.Open();
            }
        }

        void OnJoystickToggleButtonClick()
        {
            var model = UIStateManager.current.stateData;
            if (model.activeDialog != DialogType.None)
                return;
            
            if (m_DialogWindow.open)
            {
                m_DialogWindow.Close();
                m_Active = false;
            }
            else
            {
                m_DialogWindow.Open();
                m_Active = true;
            }
        }

        //public Move move
        //{
        //    get { return m_Move; }
        //    set { m_Move = value; }
        //}

        public JoystickControl leftJoystick
        {
            get { return m_LeftJoystick; }
            set { m_LeftJoystick = value; }
        }

        public JoystickControl rightJoystick
        {
            get { return m_RightJoystick; }
            set { m_RightJoystick = value; }
        }

        void Update()
        {
            //if (move == null)
            //    return;

            if (leftJoystick == null)
                return;

            if (rightJoystick == null)
                return;

            //move.forwardAxis = leftJoystick.inputAxis.y;
            //move.lateralAxis = leftJoystick.inputAxis.x;
            //move.verticalAxis = rightJoystick.inputAxis.y;
        }
    }
}
