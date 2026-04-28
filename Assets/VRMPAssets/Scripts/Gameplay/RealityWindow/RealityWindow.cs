using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Controls placement and sizing for a local passthrough window.
    /// </summary>
    public class RealityWindow : MonoBehaviour
    {
        [SerializeField] Vector2 m_SizeRange = new(0.65f, 1.8f);
        [SerializeField] float m_ScaleStep = 0.15f;

        RealityWindowManager m_Manager;
        float m_CurrentScale = 1f;

        public void SetManager(RealityWindowManager manager)
        {
            m_Manager = manager;
        }

        public void PlaceInFrontOfCamera(Camera camera, float distance, Vector3 viewportPosition)
        {
            if (camera == null)
                return;

            var viewportPoint = new Vector3(viewportPosition.x, viewportPosition.y, distance);
            transform.position = camera.ViewportToWorldPoint(viewportPoint);
            transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position, Vector3.up);
        }

        public void Close()
        {
            if (m_Manager != null)
                m_Manager.HideWindow();
            else
                gameObject.SetActive(false);
        }

        public void ResetPose()
        {
            m_Manager?.ResetWindowPose();
        }

        public void IncreaseSize()
        {
            SetScale(m_CurrentScale + m_ScaleStep);
        }

        public void DecreaseSize()
        {
            SetScale(m_CurrentScale - m_ScaleStep);
        }

        void SetScale(float targetScale)
        {
            m_CurrentScale = Mathf.Clamp(targetScale, m_SizeRange.x, m_SizeRange.y);
            transform.localScale = Vector3.one * m_CurrentScale;
        }
    }
}
