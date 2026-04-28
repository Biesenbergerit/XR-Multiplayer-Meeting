using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Receives marker placement requests and spawns shared marker objects.
    /// </summary>
    public class MeetingMarkerManager : NetworkBehaviour
    {
        public static MeetingMarkerManager Instance { get; private set; }

        [SerializeField] MeetingMarkerNetwork m_MarkerPrefab;
        [SerializeField] float m_SurfaceOffset = 0.03f;
        [SerializeField] int m_MaxMarkers = 32;

        int m_SpawnedMarkerCount;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple MeetingMarkerManager instances found. Keeping the first one.", this);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RequestPlaceMarker(Vector3 hitPoint, Vector3 hitNormal, Color color, string label)
        {
            var safeLabel = string.IsNullOrWhiteSpace(label) ? "Marker" : label;

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                SpawnMarkerOffline(hitPoint, hitNormal, color, safeLabel);
                return;
            }

            RequestPlaceMarkerRpc(hitPoint, hitNormal.normalized, color, safeLabel);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        void RequestPlaceMarkerRpc(Vector3 hitPoint, Vector3 hitNormal, Color color, FixedString64Bytes label, RpcParams rpcParams = default)
        {
            if (m_MarkerPrefab == null)
            {
                Debug.LogWarning("Meeting marker prefab is missing.", this);
                return;
            }

            if (m_SpawnedMarkerCount >= m_MaxMarkers)
                return;

            var marker = Instantiate(m_MarkerPrefab, GetPlacementPosition(hitPoint, hitNormal), GetPlacementRotation(hitNormal));
            marker.NetworkObject.Spawn();
            marker.ConfigureServer(color, label, rpcParams.Receive.SenderClientId);

            m_SpawnedMarkerCount++;
        }

        void SpawnMarkerOffline(Vector3 hitPoint, Vector3 hitNormal, Color color, string label)
        {
            if (m_MarkerPrefab == null)
            {
                Debug.LogWarning("Meeting marker prefab is missing.", this);
                return;
            }

            var marker = Instantiate(m_MarkerPrefab, GetPlacementPosition(hitPoint, hitNormal), GetPlacementRotation(hitNormal));
            marker.ConfigureOffline(color, label);
        }

        Vector3 GetPlacementPosition(Vector3 hitPoint, Vector3 hitNormal)
        {
            return hitPoint + hitNormal.normalized * m_SurfaceOffset;
        }

        static Quaternion GetPlacementRotation(Vector3 hitNormal)
        {
            var normal = hitNormal.sqrMagnitude > 0.0001f ? hitNormal.normalized : Vector3.up;
            return Quaternion.LookRotation(normal, Vector3.up);
        }
    }
}
