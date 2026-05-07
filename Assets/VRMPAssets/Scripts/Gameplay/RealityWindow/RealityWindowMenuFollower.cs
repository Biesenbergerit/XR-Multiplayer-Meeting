using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Keeps the small local menu reachable near the player without making it a networked object.
    /// </summary>
    public class RealityWindowMenuFollower : MonoBehaviour
    {
        [SerializeField] Vector3 m_ViewportPosition = new(0.58f, 0.36f, 1.05f);
        [SerializeField] Vector3 m_OpenWindowViewportPosition = new(0.5f, 0.18f, 1.05f);
        [SerializeField] float m_PositionSharpness = 8f;
        [SerializeField] float m_RotationSharpness = 10f;

        Camera m_MainCamera;

        void LateUpdate()
        {
            if (m_MainCamera == null)
                m_MainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

            if (m_MainCamera == null)
                return;

            var viewportPosition = RealityWindowManager.Instance != null && RealityWindowManager.Instance.isWindowVisible
                ? m_OpenWindowViewportPosition
                : m_ViewportPosition;

            var targetPosition = m_MainCamera.ViewportToWorldPoint(viewportPosition);
            var lookDirection = targetPosition - m_MainCamera.transform.position;
            if (lookDirection.sqrMagnitude < 0.0001f)
                return;

            var targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * m_PositionSharpness);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSharpness);
        }
    }
}
