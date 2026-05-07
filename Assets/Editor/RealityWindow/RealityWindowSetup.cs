using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using XRMultiplayer;

namespace XRMultiplayer.EditorTools
{
    public static class RealityWindowSetup
    {
        const string AssetRoot = "Assets/VRMPAssets/Prefabs/RealityWindow";
        const string WindowPrefabPath = AssetRoot + "/Reality Window.prefab";
        const string LauncherPrefabPath = AssetRoot + "/Reality Window Menu.prefab";
        const string AlphaMaterialPath = AssetRoot + "/Reality Window Alpha Punch.mat";
        const string FrameMaterialPath = AssetRoot + "/Reality Window Frame.mat";
        const string ButtonMaterialPath = AssetRoot + "/Reality Window Button.mat";
        const string HeaderMaterialPath = AssetRoot + "/Reality Window Header.mat";
        const string HologramGlassMaterialPath = AssetRoot + "/Reality Window Hologram Glass.mat";
        const string HologramGlowMaterialPath = AssetRoot + "/Reality Window Hologram Glow.mat";
        const string HologramVioletMaterialPath = AssetRoot + "/Reality Window Hologram Violet.mat";
        const string HologramRailMaterialPath = AssetRoot + "/Reality Window Hologram Rail.mat";
        const string HologramDimRailMaterialPath = AssetRoot + "/Reality Window Hologram Dim Rail.mat";
        const string WindowArtworkPath = AssetRoot + "/Reality Window Frame Artwork.png";
        const string WindowArtworkMaterialPath = AssetRoot + "/Reality Window Frame Artwork.mat";
        const string ButtonArtworkPath = AssetRoot + "/Reality Window Button Artwork.png";
        const string ButtonArtworkMaterialPath = AssetRoot + "/Reality Window Button Artwork.mat";

        static readonly string[] ScenePaths =
        {
            "Assets/Scenes/BasicScene.unity",
            "Assets/Scenes/SampleScene.unity",
        };

        [MenuItem("XR Multiplayer Meeting/Install Reality Window")]
        public static void InstallIntoOpenScene()
        {
            var windowPrefab = EnsureWindowPrefab();
            var launcherPrefab = EnsureLauncherPrefab();
            EnsureSceneObjects(windowPrefab, launcherPrefab);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Reality Window setup complete.");
        }

        [MenuItem("XR Multiplayer Meeting/Style Reality Window Button")]
        public static void StyleRealityWindowButtonPrefab()
        {
            EnsureLauncherPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("Reality Window button style complete.");
        }

        [MenuItem("XR Multiplayer Meeting/Style Reality Window Frame")]
        public static void StyleRealityWindowFramePrefab()
        {
            EnsureWindowPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("Reality Window frame style complete.");
        }

        public static void RunBatchSetup()
        {
            var windowPrefab = EnsureWindowPrefab();
            var launcherPrefab = EnsureLauncherPrefab();

            foreach (var scenePath in ScenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                    continue;

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                EnsureSceneObjects(windowPrefab, launcherPrefab);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Reality Window batch setup complete.");
        }

        static RealityWindow EnsureWindowPrefab()
        {
            Directory.CreateDirectory(AssetRoot);

            var existing = AssetDatabase.LoadAssetAtPath<RealityWindow>(WindowPrefabPath);
            if (existing != null)
            {
                BakeWindowArtworkStyle();
                return AssetDatabase.LoadAssetAtPath<RealityWindow>(WindowPrefabPath);
            }

            var alphaMaterial = EnsureMaterial(AlphaMaterialPath, Shader.Find("XR Multiplayer Meeting/Reality Window Alpha Punch"), Color.clear);
            var frameMaterial = EnsureMaterial(FrameMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.05f, 0.45f, 0.9f, 1f));
            var headerMaterial = EnsureMaterial(HeaderMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.025f, 0.03f, 0.04f, 1f));
            var buttonMaterial = EnsureMaterial(ButtonMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.1f, 0.12f, 0.14f, 1f));

            var root = new GameObject("Reality Window");
            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.28f, 0.86f, 0.06f);
            root.AddComponent<XRGrabInteractable>();
            var window = root.AddComponent<RealityWindow>();

            CreateQuad("Passthrough Cutout", root.transform, new Vector3(0f, -0.02f, 0f), new Vector3(1.18f, 0.68f, 1f), alphaMaterial);
            CreateFrame(root.transform, frameMaterial);
            CreateCube("Header", root.transform, new Vector3(0f, 0.42f, -0.01f), new Vector3(1.28f, 0.16f, 0.025f), headerMaterial);
            CreateText("Title", root.transform, "Reality Window", new Vector3(-0.28f, 0.42f, -0.03f), 0.085f, Color.white);

            CreateControlButton("Close Button", root.transform, "X", new Vector3(0.54f, 0.42f, -0.055f), RealityWindowAction.ActionType.CloseWindow, window, buttonMaterial);
            CreateControlButton("Reset Button", root.transform, "R", new Vector3(0.40f, 0.42f, -0.055f), RealityWindowAction.ActionType.ResetWindow, window, buttonMaterial);
            CreateControlButton("Larger Button", root.transform, "+", new Vector3(0.26f, 0.42f, -0.055f), RealityWindowAction.ActionType.IncreaseSize, window, buttonMaterial);
            CreateControlButton("Smaller Button", root.transform, "-", new Vector3(0.12f, 0.42f, -0.055f), RealityWindowAction.ActionType.DecreaseSize, window, buttonMaterial);
            ApplyWindowArtworkStyle(root.transform);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, WindowPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<RealityWindow>();
        }

        static GameObject EnsureLauncherPrefab()
        {
            Directory.CreateDirectory(AssetRoot);

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LauncherPrefabPath);
            if (existing != null)
            {
                BakeLauncherButtonStyle();
                return existing;
            }

            var buttonMaterial = EnsureMaterial(ButtonMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.1f, 0.12f, 0.14f, 1f));

            var root = new GameObject("Reality Window Menu");
            root.AddComponent<RealityWindowMenuFollower>();
            var button = CreateControlButton("Toggle Reality Window", root.transform, "Reality Windows", Vector3.zero, RealityWindowAction.ActionType.ToggleWindow, null, buttonMaterial, new Vector3(1.42f, 0.44f, 0.04f), 0.16f);
            ApplyArtworkButtonStyle(button.transform);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, LauncherPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void BakeLauncherButtonStyle()
        {
            var root = PrefabUtility.LoadPrefabContents(LauncherPrefabPath);
            try
            {
                var existingButton = root.transform.Find("Toggle Reality Window");
                if (existingButton != null)
                    Object.DestroyImmediate(existingButton.gameObject);

                var buttonMaterial = EnsureMaterial(ButtonMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.015f, 0.035f, 0.075f, 1f));
                var button = CreateControlButton("Toggle Reality Window", root.transform, "Reality Windows", Vector3.zero, RealityWindowAction.ActionType.ToggleWindow, null, buttonMaterial, new Vector3(1.42f, 0.44f, 0.04f), 0.16f);
                ApplyArtworkButtonStyle(button.transform);

                PrefabUtility.SaveAsPrefabAsset(root, LauncherPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void BakeWindowArtworkStyle()
        {
            var root = PrefabUtility.LoadPrefabContents(WindowPrefabPath);
            try
            {
                ApplyWindowArtworkStyle(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, WindowPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void EnsureSceneObjects(RealityWindow windowPrefab, GameObject launcherPrefab)
        {
            var manager = Object.FindAnyObjectByType<RealityWindowManager>(FindObjectsInactive.Include);
            if (manager == null)
            {
                var managerObject = new GameObject("Reality Window Manager");
                manager = managerObject.AddComponent<RealityWindowManager>();
            }

            var serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("m_WindowPrefab").objectReferenceValue = windowPrefab;
            serializedManager.FindProperty("m_ShowOnStart").boolValue = false;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);

            if (Object.FindAnyObjectByType<RealityWindowMenuFollower>(FindObjectsInactive.Include) == null && launcherPrefab != null)
            {
                var launcher = PrefabUtility.InstantiatePrefab(launcherPrefab) as GameObject;
                if (launcher != null)
                    launcher.name = "Reality Window Menu";
            }
        }

        static Material EnsureMaterial(string path, Shader shader, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                ApplyMaterialColor(material, color, Color.black, false);
                return material;
            }

            material = new Material(shader)
            {
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Material EnsureHologramMaterial(string path, Color baseColor, Color emissionColor, bool transparent)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            ApplyMaterialColor(material, baseColor, emissionColor, transparent);
            return material;
        }

        static Material EnsureButtonArtworkMaterial()
        {
            return EnsureArtworkMaterial(ButtonArtworkPath, ButtonArtworkMaterialPath, "button");
        }

        static Material EnsureWindowArtworkMaterial()
        {
            return EnsureArtworkMaterial(WindowArtworkPath, WindowArtworkMaterialPath, "frame");
        }

        static Material EnsureArtworkMaterial(string texturePath, string materialPath, string label)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
                throw new FileNotFoundException($"Missing Reality Window {label} artwork at {texturePath}");

            if (AssetImporter.GetAtPath(texturePath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.mipmapEnabled = true;
                importer.sRGBTexture = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent") ?? Shader.Find("Standard");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.shader = shader;
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = 3000;

            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_AlphaClip"))
                material.SetFloat("_AlphaClip", 0f);
            if (material.HasProperty("_Cull"))
                material.SetFloat("_Cull", 0f);
            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        static void ApplyMaterialColor(Material material, Color baseColor, Color emissionColor, bool transparent)
        {
            material.color = baseColor;
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

            EditorUtility.SetDirty(material);

            if (!transparent)
                return;

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
            EditorUtility.SetDirty(material);
        }

        static GameObject CreateQuad(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = localPosition;
            quad.transform.localScale = localScale;
            quad.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            return quad;
        }

        static GameObject CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            return cube;
        }

        static void CreateFrame(Transform parent, Material material)
        {
            const float width = 1.24f;
            const float height = 0.74f;
            const float thickness = 0.035f;
            CreateCube("Frame Top", parent, new Vector3(0f, height * 0.5f, -0.02f), new Vector3(width, thickness, 0.035f), material);
            CreateCube("Frame Bottom", parent, new Vector3(0f, -height * 0.5f, -0.02f), new Vector3(width, thickness, 0.035f), material);
            CreateCube("Frame Left", parent, new Vector3(-width * 0.5f, 0f, -0.02f), new Vector3(thickness, height, 0.035f), material);
            CreateCube("Frame Right", parent, new Vector3(width * 0.5f, 0f, -0.02f), new Vector3(thickness, height, 0.035f), material);
        }

        static TextMeshPro CreateText(string name, Transform parent, string text, Vector3 localPosition, float fontSize, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            var label = textObject.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = new Vector2(0.8f, 0.18f);
            return label;
        }

        static GameObject CreateControlButton(string name, Transform parent, string labelText, Vector3 localPosition, RealityWindowAction.ActionType actionType, RealityWindow window, Material material)
        {
            return CreateControlButton(name, parent, labelText, localPosition, actionType, window, material, new Vector3(0.1f, 0.1f, 0.035f), 0.075f);
        }

        static GameObject CreateControlButton(string name, Transform parent, string labelText, Vector3 localPosition, RealityWindowAction.ActionType actionType, RealityWindow window, Material material, Vector3 size, float fontSize)
        {
            var button = GameObject.CreatePrimitive(PrimitiveType.Cube);
            button.name = name;
            button.transform.SetParent(parent, false);
            button.transform.localPosition = localPosition;
            button.transform.localScale = size;
            button.GetComponent<Renderer>().sharedMaterial = material;

            var simpleInteractable = button.AddComponent<XRSimpleInteractable>();
            simpleInteractable.allowGazeInteraction = true;
            var menuButton = button.AddComponent<RealityWindowMenuButton>();
            var action = button.AddComponent<RealityWindowAction>();
            var serializedAction = new SerializedObject(action);
            serializedAction.FindProperty("m_Action").enumValueIndex = (int)actionType;
            serializedAction.FindProperty("m_Window").objectReferenceValue = window;
            serializedAction.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(menuButton.onPressed, action.Execute);

            var label = CreateText("Label", button.transform, labelText, new Vector3(0f, 0f, -0.021f), fontSize, Color.white);
            label.rectTransform.sizeDelta = new Vector2(size.x * 4f, size.y * 2f);

            return button;
        }

        static void ApplyWindowArtworkStyle(Transform windowRoot)
        {
            if (windowRoot.TryGetComponent(out BoxCollider collider))
            {
                collider.size = new Vector3(1.55f, 1.16f, 0.08f);
                collider.center = Vector3.zero;
                EditorUtility.SetDirty(collider);
            }

            var alphaMaterial = EnsureMaterial(AlphaMaterialPath, Shader.Find("XR Multiplayer Meeting/Reality Window Alpha Punch"), Color.clear);
            var cutout = windowRoot.Find("Passthrough Cutout");
            if (cutout == null)
                cutout = CreateQuad("Passthrough Cutout", windowRoot, Vector3.zero, Vector3.one, alphaMaterial).transform;

            cutout.localPosition = new Vector3(0f, -0.015f, 0f);
            cutout.localRotation = Quaternion.identity;
            cutout.localScale = new Vector3(1.19f, 0.69f, 1f);
            if (cutout.TryGetComponent(out Renderer cutoutRenderer))
            {
                cutoutRenderer.enabled = true;
                cutoutRenderer.sharedMaterial = alphaMaterial;
            }

            foreach (var legacyName in new[]
            {
                "Header",
                "Frame Top",
                "Frame Bottom",
                "Frame Left",
                "Frame Right",
                "Accent Top",
                "Accent Bottom",
                "Accent Left",
                "Accent Right",
                "Title"
            })
            {
                var legacy = windowRoot.Find(legacyName);
                if (legacy != null)
                    Object.DestroyImmediate(legacy.gameObject);
            }

            var artworkMaterial = EnsureWindowArtworkMaterial();
            var artwork = windowRoot.Find("Window Artwork Frame");
            if (artwork == null)
            {
                artwork = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                artwork.name = "Window Artwork Frame";
                artwork.SetParent(windowRoot, false);
            }

            artwork.localPosition = new Vector3(0f, 0f, -0.055f);
            artwork.localRotation = Quaternion.identity;
            artwork.localScale = new Vector3(1.55f, 1.16f, 1f);
            if (artwork.TryGetComponent(out Renderer artworkRenderer))
            {
                artworkRenderer.enabled = true;
                artworkRenderer.sharedMaterial = artworkMaterial;
            }
            if (artwork.TryGetComponent(out Collider artworkCollider))
                Object.DestroyImmediate(artworkCollider);

            var buttonMaterial = EnsureMaterial(ButtonMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.012f, 0.022f, 0.032f, 0.92f));
            ApplyWindowControlStyle(windowRoot.Find("Smaller Button"), new Vector3(0.36f, 0.455f, -0.08f), buttonMaterial);
            ApplyWindowControlStyle(windowRoot.Find("Larger Button"), new Vector3(0.46f, 0.455f, -0.08f), buttonMaterial);
            ApplyWindowControlStyle(windowRoot.Find("Reset Button"), new Vector3(0.56f, 0.455f, -0.08f), buttonMaterial);
            ApplyWindowControlStyle(windowRoot.Find("Close Button"), new Vector3(0.66f, 0.455f, -0.08f), buttonMaterial);

            EditorUtility.SetDirty(windowRoot);
        }

        static void ApplyWindowControlStyle(Transform control, Vector3 localPosition, Material material)
        {
            if (control == null)
                return;

            control.localPosition = localPosition;
            control.localRotation = Quaternion.identity;
            control.localScale = new Vector3(0.06f, 0.06f, 0.026f);

            if (control.TryGetComponent(out MeshRenderer renderer))
            {
                renderer.enabled = true;
                renderer.sharedMaterial = material;
            }

            foreach (var text in control.GetComponentsInChildren<TMP_Text>(true))
            {
                text.color = new Color(0.82f, 0.97f, 1f, 1f);
                text.fontSize = 0.04f;
                text.fontStyle = FontStyles.Bold;
                text.raycastTarget = false;
                text.enableWordWrapping = false;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.localPosition = new Vector3(0f, 0f, -0.03f);
                text.rectTransform.anchoredPosition = Vector2.zero;
                text.rectTransform.localScale = Vector3.one;
                text.rectTransform.sizeDelta = new Vector2(0.18f, 0.12f);
                EditorUtility.SetDirty(text);
            }

            EditorUtility.SetDirty(control);
        }

        static void ApplyArtworkButtonStyle(Transform button)
        {
            button.localPosition = Vector3.zero;
            button.localRotation = Quaternion.identity;
            button.localScale = new Vector3(1.42f, 0.44f, 0.04f);

            if (button.TryGetComponent(out MeshRenderer rootRenderer))
                rootRenderer.enabled = false;

            for (var index = button.childCount - 1; index >= 0; index--)
                Object.DestroyImmediate(button.GetChild(index).gameObject);

            var artworkMaterial = EnsureButtonArtworkMaterial();
            var artwork = GameObject.CreatePrimitive(PrimitiveType.Quad);
            artwork.name = "Artwork Panel";
            artwork.transform.SetParent(button, false);
            artwork.transform.localPosition = new Vector3(0f, 0f, -0.08f);
            artwork.transform.localRotation = Quaternion.identity;
            artwork.transform.localScale = Vector3.one;
            artwork.GetComponent<Renderer>().sharedMaterial = artworkMaterial;
            Object.DestroyImmediate(artwork.GetComponent<Collider>());

            if (button.TryGetComponent(out BoxCollider collider))
            {
                collider.size = new Vector3(1f, 1f, 0.18f);
                collider.center = Vector3.zero;
            }

            EditorUtility.SetDirty(button);
        }

        static void ApplyHologramButtonStyle(Transform button)
        {
            button.localPosition = Vector3.zero;
            button.localRotation = Quaternion.identity;
            button.localScale = new Vector3(1.05f, 0.26f, 0.04f);

            foreach (var rootText in button.GetComponents<TMP_Text>())
                Object.DestroyImmediate(rootText);
            foreach (var rootCanvasRenderer in button.GetComponents<CanvasRenderer>())
                Object.DestroyImmediate(rootCanvasRenderer);

            var legacyButtonMaterial = EnsureHologramMaterial(ButtonMaterialPath, new Color(0.006f, 0.014f, 0.028f, 1f), new Color(0f, 0.05f, 0.1f, 1f), false);
            var glassMaterial = EnsureHologramMaterial(HologramGlassMaterialPath, new Color(0.01f, 0.025f, 0.055f, 1f), new Color(0f, 0.08f, 0.18f, 1f), false);
            var outerGlowMaterial = EnsureHologramMaterial(HologramGlowMaterialPath, new Color(0.02f, 0.65f, 0.95f, 0.28f), new Color(0f, 0.72f, 1f, 1f), true);
            var blueGlowMaterial = EnsureHologramMaterial(HologramVioletMaterialPath, new Color(0.24f, 0.28f, 0.95f, 0.36f), new Color(0.16f, 0.22f, 1f, 1f), true);
            var railMaterial = EnsureHologramMaterial(HologramRailMaterialPath, new Color(0.52f, 0.95f, 1f, 0.9f), new Color(0f, 0.86f, 1f, 1f), false);
            var dimRailMaterial = EnsureHologramMaterial(HologramDimRailMaterialPath, new Color(0.04f, 0.18f, 0.34f, 0.56f), new Color(0f, 0.24f, 0.5f, 1f), true);

            if (button.TryGetComponent(out MeshRenderer rootRenderer))
            {
                rootRenderer.sharedMaterial = legacyButtonMaterial;
                rootRenderer.enabled = true;
            }

            SetRoundedRect(button, "Outer Glow", 1.02f, 0.72f, 0f, -0.54f, outerGlowMaterial);
            SetRoundedRect(button, "Violet Edge", 0.98f, 0.64f, 0f, -0.58f, blueGlowMaterial);
            SetRoundedRect(button, "Glass Plate", 0.93f, 0.5f, 0f, -0.62f, glassMaterial);
            SetRoundedRect(button, "Inner Glass", 0.78f, 0.34f, 0f, -0.67f, dimRailMaterial);

            SetBar(button, "Top Cyan Rail", new Vector3(-0.08f, 0.295f, -0.76f), new Vector3(0.68f, 0.012f, 0.018f), Quaternion.identity, railMaterial);
            SetBar(button, "Top Violet Rail", new Vector3(0.38f, 0.295f, -0.76f), new Vector3(0.16f, 0.012f, 0.018f), Quaternion.identity, blueGlowMaterial);
            SetBar(button, "Bottom Cyan Rail", new Vector3(0.08f, -0.295f, -0.76f), new Vector3(0.72f, 0.012f, 0.018f), Quaternion.identity, railMaterial);
            SetBar(button, "Left Detail Rail", new Vector3(-0.48f, 0f, -0.76f), new Vector3(0.012f, 0.42f, 0.018f), Quaternion.identity, railMaterial);
            SetBar(button, "Right Detail Rail", new Vector3(0.48f, 0f, -0.76f), new Vector3(0.012f, 0.42f, 0.018f), Quaternion.identity, railMaterial);
            SetBar(button, "Left Lower Detail", new Vector3(-0.34f, -0.19f, -0.76f), new Vector3(0.12f, 0.01f, 0.018f), Quaternion.identity, dimRailMaterial);
            SetBar(button, "Right Lower Detail", new Vector3(0.34f, -0.19f, -0.76f), new Vector3(0.12f, 0.01f, 0.018f), Quaternion.identity, dimRailMaterial);
            SetBar(button, "Center Notch", new Vector3(0f, -0.285f, -0.78f), new Vector3(0.075f, 0.018f, 0.018f), Quaternion.Euler(0f, 0f, 45f), railMaterial);

            for (var index = 0; index < 5; index++)
            {
                var y = -0.16f + index * 0.08f;
                SetBar(button, $"Left Light Tick {index}", new Vector3(-0.525f, y, -0.76f), new Vector3(0.014f, 0.026f, 0.018f), Quaternion.identity, dimRailMaterial);
                SetBar(button, $"Right Light Tick {index}", new Vector3(0.525f, y, -0.76f), new Vector3(0.014f, 0.026f, 0.018f), Quaternion.identity, dimRailMaterial);
            }

            foreach (var text in button.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.transform == button)
                    continue;

                text.text = "Reality Window";
                text.color = new Color(0.88f, 0.98f, 1f, 1f);
                text.fontSize = 0.16f;
                text.fontStyle = FontStyles.Bold;
                text.characterSpacing = 0f;
                text.raycastTarget = false;
                text.enableWordWrapping = false;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.localPosition = new Vector3(0f, 0f, -0.88f);
                text.rectTransform.anchoredPosition = Vector2.zero;
                text.rectTransform.localScale = Vector3.one;
                text.rectTransform.sizeDelta = new Vector2(1.18f, 0.34f);
                EditorUtility.SetDirty(text);
            }

            EditorUtility.SetDirty(button);
        }

        static void SetRoundedRect(Transform parent, string childName, float width, float height, float radius, float localZ, Material material)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var rectObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rectObject.name = childName;
                rectObject.transform.SetParent(parent, false);
                Object.DestroyImmediate(rectObject.GetComponent<Collider>());
                child = rectObject.transform;
            }

            child.localPosition = new Vector3(0f, 0f, localZ);
            child.localRotation = Quaternion.identity;
            child.localScale = new Vector3(width, height, 0.022f);

            var meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh == null)
            {
                var tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meshFilter.sharedMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(tempCube);
            }

            var meshRenderer = child.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.enabled = true;
        }

        static void SetBar(Transform parent, string childName, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = childName;
                bar.transform.SetParent(parent, false);
                Object.DestroyImmediate(bar.GetComponent<Collider>());
                child = bar.transform;
            }

            child.localPosition = localPosition;
            child.localRotation = localRotation;
            child.localScale = localScale;

            var meshRenderer = child.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.enabled = true;
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
    }
}
