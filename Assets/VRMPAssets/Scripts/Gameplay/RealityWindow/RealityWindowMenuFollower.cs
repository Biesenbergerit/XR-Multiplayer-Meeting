using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Keeps the small local menu reachable near the player without making it a networked object.
    /// </summary>
    public class RealityWindowMenuFollower : MonoBehaviour
    {
        [SerializeField] Vector3 m_ViewportPosition = new(0.78f, 0.33f, 1.15f);
        [SerializeField] float m_PositionSharpness = 8f;
        [SerializeField] float m_RotationSharpness = 10f;

        Camera m_MainCamera;

        void LateUpdate()
        {
            if (m_MainCamera == null)
                m_MainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

            if (m_MainCamera == null)
                return;

            var targetPosition = m_MainCamera.ViewportToWorldPoint(m_ViewportPosition);
            var targetRotation = Quaternion.LookRotation(transform.position - m_MainCamera.transform.position, Vector3.up);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * m_PositionSharpness);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotationSharpness);
        }
    }
}
