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
                return existing;

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

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, WindowPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<RealityWindow>();
        }

        static GameObject EnsureLauncherPrefab()
        {
            Directory.CreateDirectory(AssetRoot);

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LauncherPrefabPath);
            if (existing != null)
                return existing;

            var buttonMaterial = EnsureMaterial(ButtonMaterialPath, Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"), new Color(0.1f, 0.12f, 0.14f, 1f));

            var root = new GameObject("Reality Window Menu");
            root.AddComponent<RealityWindowMenuFollower>();
            CreateControlButton("Toggle Reality Window", root.transform, "Reality Window", Vector3.zero, RealityWindowAction.ActionType.ToggleWindow, null, buttonMaterial, new Vector3(0.42f, 0.16f, 0.035f), 0.052f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, LauncherPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
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
                return material;

            material = new Material(shader)
            {
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
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
    }
}
