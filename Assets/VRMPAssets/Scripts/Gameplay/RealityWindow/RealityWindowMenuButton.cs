using System.Collections.Generic;
using TMPro;
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
            ApplyRealityWindowButtonStyle();
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

        void ApplyRealityWindowButtonStyle()
        {
            if (!name.Contains("Reality Window"))
                return;

            if (transform.Find("Artwork Panel") != null)
                return;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(1.05f, 0.26f, 0.04f);

            var glassMaterial = CreateHologramMaterial(new Color(0.01f, 0.025f, 0.055f, 1f), new Color(0.0f, 0.08f, 0.18f, 1f), false);
            var outerGlowMaterial = CreateHologramMaterial(new Color(0.02f, 0.65f, 0.95f, 0.28f), new Color(0.0f, 0.72f, 1f, 1f), true);
            var blueGlowMaterial = CreateHologramMaterial(new Color(0.24f, 0.28f, 0.95f, 0.36f), new Color(0.16f, 0.22f, 1f, 1f), true);
            var railMaterial = CreateHologramMaterial(new Color(0.52f, 0.95f, 1f, 0.9f), new Color(0.0f, 0.86f, 1f, 1f), false);
            var dimRailMaterial = CreateHologramMaterial(new Color(0.04f, 0.18f, 0.34f, 0.56f), new Color(0.0f, 0.24f, 0.5f, 1f), true);

            if (TryGetComponent(out MeshRenderer rootRenderer))
            {
                rootRenderer.sharedMaterial = glassMaterial;
                rootRenderer.enabled = true;
            }

            SetRoundedRect("Outer Glow", 1.02f, 0.72f, 0f, -0.54f, outerGlowMaterial);
            SetRoundedRect("Violet Edge", 0.98f, 0.64f, 0f, -0.58f, blueGlowMaterial);
            SetRoundedRect("Glass Plate", 0.93f, 0.5f, 0f, -0.62f, glassMaterial);
            SetRoundedRect("Inner Glass", 0.78f, 0.34f, 0f, -0.67f, dimRailMaterial);

            SetBar("Top Cyan Rail", new Vector3(-0.08f, 0.295f, -0.76f), new Vector3(0.68f, 0.012f, 0.018f), Quaternion.identity, railMaterial);
            SetBar("Top Violet Rail", new Vector3(0.38f, 0.295f, -0.76f), new Vector3(0.16f, 0.012f, 0.018f), Quaternion.identity, blueGlowMaterial);
            SetBar("Bottom Cyan Rail", new Vector3(0.08f, -0.295f, -0.76f), new Vector3(0.72f, 0.012f, 0.018f), Quaternion.identity, railMaterial);
            SetBar("Left Detail Rail", new Vector3(-0.48f, 0f, -0.76f), new Vector3(0.012f, 0.42f, 0.018f), Quaternion.identity, railMaterial);
            SetBar("Right Detail Rail", new Vector3(0.48f, 0f, -0.76f), new Vector3(0.012f, 0.42f, 0.018f), Quaternion.identity, railMaterial);
            SetBar("Left Lower Detail", new Vector3(-0.34f, -0.19f, -0.76f), new Vector3(0.12f, 0.01f, 0.018f), Quaternion.identity, dimRailMaterial);
            SetBar("Right Lower Detail", new Vector3(0.34f, -0.19f, -0.76f), new Vector3(0.12f, 0.01f, 0.018f), Quaternion.identity, dimRailMaterial);
            SetBar("Center Notch", new Vector3(0f, -0.285f, -0.78f), new Vector3(0.075f, 0.018f, 0.018f), Quaternion.Euler(0f, 0f, 45f), railMaterial);

            for (var index = 0; index < 5; index++)
            {
                var y = -0.16f + index * 0.08f;
                SetBar($"Left Light Tick {index}", new Vector3(-0.525f, y, -0.76f), new Vector3(0.014f, 0.026f, 0.018f), Quaternion.identity, dimRailMaterial);
                SetBar($"Right Light Tick {index}", new Vector3(0.525f, y, -0.76f), new Vector3(0.014f, 0.026f, 0.018f), Quaternion.identity, dimRailMaterial);
            }

            foreach (var text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.transform == transform)
                    continue;

                text.text = "Reality Window";
                text.color = new Color(0.88f, 0.98f, 1f, 1f);
                text.fontSize = 0.16f;
                text.fontStyle = FontStyles.Bold;
                text.characterSpacing = 0f;
                text.raycastTarget = false;
                text.enableWordWrapping = false;
                text.alignment = TextAlignmentOptions.Center;

                if (text.rectTransform != null)
                {
                    text.rectTransform.localPosition = new Vector3(0f, 0f, -0.88f);
                    text.rectTransform.anchoredPosition = Vector2.zero;
                    text.rectTransform.localScale = Vector3.one;
                    text.rectTransform.sizeDelta = new Vector2(1.18f, 0.34f);
                }
            }
        }

        void SetRoundedRect(string childName, float width, float height, float radius, float localZ, Material material)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var rectObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rectObject.name = childName;
                rectObject.transform.SetParent(transform, false);
                if (rectObject.TryGetComponent(out Collider rectCollider))
                    Destroy(rectCollider);
                child = rectObject.transform;
            }
            else if (child.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh == null)
            {
                var replacement = GameObject.CreatePrimitive(PrimitiveType.Cube);
                replacement.name = childName;
                replacement.transform.SetParent(transform, false);
                replacement.transform.SetSiblingIndex(child.GetSiblingIndex());
                if (replacement.TryGetComponent(out Collider replacementCollider))
                    Destroy(replacementCollider);
                Destroy(child.gameObject);
                child = replacement.transform;
            }

            child.localPosition = new Vector3(0f, 0f, localZ);
            child.localRotation = Quaternion.identity;
            child.localScale = new Vector3(width, height, 0.022f);

            if (child.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer.sharedMaterial = material;
        }

        void SetBar(string childName, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            var child = transform.Find(childName);
            if (child == null)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = childName;
                bar.transform.SetParent(transform, false);
                if (bar.TryGetComponent(out Collider barCollider))
                    Destroy(barCollider);
                child = bar.transform;
            }

            child.localPosition = localPosition;
            child.localRotation = localRotation;
            child.localScale = localScale;

            if (child.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer.sharedMaterial = material;
        }

        static Mesh CreateRoundedRectMesh(float width, float height, float radius, int cornerSegments)
        {
            radius = Mathf.Min(radius, width * 0.5f, height * 0.5f);

            var vertices = new List<Vector3> { Vector3.zero };
            AddCorner(vertices, new Vector2(width * 0.5f - radius, height * 0.5f - radius), radius, 0f, 90f, cornerSegments);
            AddCorner(vertices, new Vector2(-width * 0.5f + radius, height * 0.5f - radius), radius, 90f, 180f, cornerSegments);
            AddCorner(vertices, new Vector2(-width * 0.5f + radius, -height * 0.5f + radius), radius, 180f, 270f, cornerSegments);
            AddCorner(vertices, new Vector2(width * 0.5f - radius, -height * 0.5f + radius), radius, 270f, 360f, cornerSegments);

            var triangles = new List<int>();
            for (var index = 1; index < vertices.Count; index++)
            {
                var nextIndex = index == vertices.Count - 1 ? 1 : index + 1;
                triangles.Add(0);
                triangles.Add(index);
                triangles.Add(nextIndex);
                triangles.Add(0);
                triangles.Add(nextIndex);
                triangles.Add(index);
            }

            var mesh = new Mesh
            {
                name = "Reality Window Hologram Plate",
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static void AddCorner(List<Vector3> vertices, Vector2 center, float radius, float startAngle, float endAngle, int segments)
        {
            for (var index = 0; index <= segments; index++)
            {
                var angle = Mathf.Lerp(startAngle, endAngle, index / (float)segments) * Mathf.Deg2Rad;
                vertices.Add(new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, 0f));
            }
        }

        static Material CreateHologramMaterial(Color baseColor, Color emissionColor, bool transparent)
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
                material.SetFloat("_Smoothness", 0.86f);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0.12f);
            if (material.HasProperty("_Cull"))
                material.SetFloat("_Cull", 0f);

            if (transparent)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = 3000;
                if (material.HasProperty("_Surface"))
                    material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_Blend"))
                    material.SetFloat("_Blend", 0f);
                if (material.HasProperty("_AlphaClip"))
                    material.SetFloat("_AlphaClip", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            return material;
        }
    }
}
