using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace XRMultiplayer
{
    /// <summary>
    /// Small XR-selectable button used by the local reality window menu and controls.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class RealityWindowMenuButton : MonoBehaviour
    {
        [SerializeField] UnityEvent m_OnPressed = new();

        XRSimpleInteractable m_Interactable;

        public UnityEvent onPressed => m_OnPressed;

        void Awake()
        {
            m_Interactable = GetComponent<XRSimpleInteractable>();
        }

        void OnEnable()
        {
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        public void Press()
        {
            m_OnPressed.Invoke();
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            Press();
        }
    }
}
