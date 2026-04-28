using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace XRMultiplayer
{
    /// <summary>
    /// Places a collaborative marker at the current XR ray hit when the ray activate input is pressed.
    /// </summary>
    [RequireComponent(typeof(XRRayInteractor))]
    public class MeetingMarkerPlacer : MonoBehaviour
    {
        [SerializeField] XRRayInteractor m_RayInteractor;
        [SerializeField] MeetingMarkerManager m_Manager;
        [SerializeField] string m_DefaultLabel = "Meeting Point";
        [SerializeField] LayerMask m_PlacementLayers = ~0;
        [SerializeField] bool m_IgnoreWhenSelecting = true;
        [SerializeField] float m_MinSecondsBetweenMarkers = 0.35f;

        float m_NextAllowedPlacementTime;

        void Awake()
        {
            if (m_RayInteractor == null)
                m_RayInteractor = GetComponent<XRRayInteractor>();
        }

        void Update()
        {
            if (m_RayInteractor == null || Time.time < m_NextAllowedPlacementTime)
                return;

            if (m_IgnoreWhenSelecting && m_RayInteractor.hasSelection)
                return;

            if (!m_RayInteractor.logicalActivateState.wasPerformedThisFrame)
                return;

            if (!m_RayInteractor.TryGetCurrent3DRaycastHit(out var hit))
                return;

            if ((m_PlacementLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
                return;

            var manager = m_Manager != null ? m_Manager : MeetingMarkerManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("No MeetingMarkerManager found in the active scene.", this);
                return;
            }

            manager.RequestPlaceMarker(hit.point, hit.normal, XRINetworkGameManager.LocalPlayerColor.Value, m_DefaultLabel);
            m_NextAllowedPlacementTime = Time.time + m_MinSecondsBetweenMarkers;
        }
    }
}
