using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// UnityEvent target for reality window controls.
    /// </summary>
    public class RealityWindowAction : MonoBehaviour
    {
        public enum ActionType
        {
            ToggleWindow,
            CloseWindow,
            ResetWindow,
            IncreaseSize,
            DecreaseSize
        }

        [SerializeField] ActionType m_Action;
        [SerializeField] RealityWindow m_Window;

        public void Execute()
        {
            switch (m_Action)
            {
                case ActionType.ToggleWindow:
                    RealityWindowManager.Instance?.ToggleWindow();
                    break;
                case ActionType.CloseWindow:
                    m_Window?.Close();
                    break;
                case ActionType.ResetWindow:
                    m_Window?.ResetPose();
                    break;
                case ActionType.IncreaseSize:
                    m_Window?.IncreaseSize();
                    break;
                case ActionType.DecreaseSize:
                    m_Window?.DecreaseSize();
                    break;
            }
        }
    }
}
