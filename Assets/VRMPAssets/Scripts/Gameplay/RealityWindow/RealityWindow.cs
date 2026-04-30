using UnityEngine;
using TMPro;

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
        Material m_FrameMaterial;
        Material m_HeaderMaterial;
        Material m_ButtonMaterial;
        Material m_AccentMaterial;

        void Awake()
        {
            ApplyVisualPolish();
        }

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
            var lookDirection = transform.position - camera.transform.position;
            if (lookDirection.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
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

        void ApplyVisualPolish()
        {
            m_FrameMaterial = CreateMaterial(new Color(0.02f, 0.22f, 0.26f, 1f), new Color(0.0f, 0.28f, 0.32f, 1f));
            m_HeaderMaterial = CreateMaterial(new Color(0.025f, 0.032f, 0.04f, 1f), new Color(0.0f, 0.07f, 0.08f, 1f));
            m_ButtonMaterial = CreateMaterial(new Color(0.08f, 0.12f, 0.14f, 1f), new Color(0.0f, 0.12f, 0.15f, 1f));
            m_AccentMaterial = CreateMaterial(new Color(0.0f, 0.76f, 0.84f, 1f), new Color(0.0f, 0.42f, 0.5f, 1f));

            SetChildVisual("Header", new Vector3(0f, 0.405f, -0.012f), new Vector3(1.26f, 0.11f, 0.02f), m_HeaderMaterial);
            SetChildVisual("Passthrough Cutout", new Vector3(0f, -0.012f, 0f), new Vector3(1.2f, 0.69f, 1f), null);
            SetChildVisual("Frame Top", new Vector3(0f, 0.36f, -0.024f), new Vector3(1.24f, 0.024f, 0.026f), m_FrameMaterial);
            SetChildVisual("Frame Bottom", new Vector3(0f, -0.36f, -0.024f), new Vector3(1.24f, 0.024f, 0.026f), m_FrameMaterial);
            SetChildVisual("Frame Left", new Vector3(-0.625f, 0f, -0.024f), new Vector3(0.024f, 0.72f, 0.026f), m_FrameMaterial);
            SetChildVisual("Frame Right", new Vector3(0.625f, 0f, -0.024f), new Vector3(0.024f, 0.72f, 0.026f), m_FrameMaterial);

            SetChildVisual("Smaller Button", new Vector3(0.16f, 0.405f, -0.05f), new Vector3(0.084f, 0.084f, 0.026f), m_ButtonMaterial);
            SetChildVisual("Larger Button", new Vector3(0.28f, 0.405f, -0.05f), new Vector3(0.084f, 0.084f, 0.026f), m_ButtonMaterial);
            SetChildVisual("Reset Button", new Vector3(0.4f, 0.405f, -0.05f), new Vector3(0.084f, 0.084f, 0.026f), m_ButtonMaterial);
            SetChildVisual("Close Button", new Vector3(0.52f, 0.405f, -0.05f), new Vector3(0.084f, 0.084f, 0.026f), m_ButtonMaterial);

            AddOrUpdateAccent("Accent Top", new Vector3(0f, 0.333f, -0.052f), new Vector3(1.1f, 0.01f, 0.012f));
            AddOrUpdateAccent("Accent Bottom", new Vector3(0f, -0.333f, -0.052f), new Vector3(1.1f, 0.01f, 0.012f));
            AddOrUpdateAccent("Accent Left", new Vector3(-0.596f, 0f, -0.052f), new Vector3(0.01f, 0.62f, 0.012f));
            AddOrUpdateAccent("Accent Right", new Vector3(0.596f, 0f, -0.052f), new Vector3(0.01f, 0.62f, 0.012f));

            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
            {
                text.color = new Color(0.88f, 0.98f, 1f, 1f);
                text.raycastTarget = false;
                if (text.transform.name == "Title")
                {
                    text.text = "Reality View";
                    text.fontSize = 0.066f;
                    text.rectTransform.anchoredPosition = new Vector2(-0.28f, 0.405f);
                    text.rectTransform.sizeDelta = new Vector2(0.68f, 0.14f);
                }
                else
                {
                    text.fontSize = 0.064f;
                }
            }
        }

        void SetChildVisual(string childName, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var child = FindChild(childName);
            if (child == null)
                return;

            child.localPosition = localPosition;
            child.localScale = localScale;

            if (material != null && child.TryGetComponent(out Renderer renderer))
                renderer.sharedMaterial = material;
        }

        void AddOrUpdateAccent(string childName, Vector3 localPosition, Vector3 localScale)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var accent = GameObject.CreatePrimitive(PrimitiveType.Cube);
                accent.name = childName;
                accent.transform.SetParent(transform, false);
                if (accent.TryGetComponent(out Collider accentCollider))
                    Destroy(accentCollider);
                child = accent.transform;
            }

            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = localScale;

            if (child.TryGetComponent(out Renderer renderer))
                renderer.sharedMaterial = m_AccentMaterial;
        }

        Transform FindChild(string childName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                    return child;
            }

            return null;
        }

        static Material CreateMaterial(Color baseColor, Color emissionColor)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var material = new Material(shader)
            {
                color = baseColor
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", baseColor);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor);
            }
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.72f);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0.08f);

            return material;
        }
    }
}
