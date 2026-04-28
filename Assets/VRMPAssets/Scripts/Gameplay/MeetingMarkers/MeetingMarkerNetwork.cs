using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Network-synchronized visual state for a collaborative meeting marker.
    /// </summary>
    public class MeetingMarkerNetwork : NetworkBehaviour
    {
        [SerializeField] Renderer[] m_ColorRenderers;
        [SerializeField] TMP_Text m_LabelText;

        readonly NetworkVariable<Color> m_MarkerColor = new(Color.white);
        readonly NetworkVariable<FixedString64Bytes> m_Label = new("Marker");
        readonly NetworkVariable<ulong> m_CreatorClientId = new();

        public ulong creatorClientId => m_CreatorClientId.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_MarkerColor.OnValueChanged += OnColorChanged;
            m_Label.OnValueChanged += OnLabelChanged;

            ApplyColor(m_MarkerColor.Value);
            ApplyLabel(m_Label.Value);
        }

        public override void OnNetworkDespawn()
        {
            m_MarkerColor.OnValueChanged -= OnColorChanged;
            m_Label.OnValueChanged -= OnLabelChanged;

            base.OnNetworkDespawn();
        }

        public void ConfigureServer(Color color, FixedString64Bytes label, ulong creatorClientId)
        {
            if (!IsServer)
                return;

            m_MarkerColor.Value = color;
            m_Label.Value = label;
            m_CreatorClientId.Value = creatorClientId;
        }

        public void ConfigureOffline(Color color, string label)
        {
            ApplyColor(color);
            ApplyLabel(label);
        }

        void OnColorChanged(Color previousValue, Color newValue)
        {
            ApplyColor(newValue);
        }

        void OnLabelChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            ApplyLabel(newValue);
        }

        void ApplyColor(Color color)
        {
            if (m_ColorRenderers == null)
                return;

            foreach (var colorRenderer in m_ColorRenderers)
            {
                if (colorRenderer == null)
                    continue;

                colorRenderer.material.color = color;
            }
        }

        void ApplyLabel(FixedString64Bytes label)
        {
            ApplyLabel(label.ToString());
        }

        void ApplyLabel(string label)
        {
            if (m_LabelText != null)
                m_LabelText.text = label;
        }
    }
}
