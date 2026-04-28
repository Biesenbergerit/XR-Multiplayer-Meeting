using System.IO;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using XRMultiplayer;

namespace XRMultiplayer.EditorTools
{
    public static class MeetingMarkerSetup
    {
        const string MarkerRoot = "Assets/VRMPAssets/Prefabs/NetworkedPrefabs/MeetingMarkers";
        const string MarkerPrefabPath = MarkerRoot + "/Meeting Marker Networked.prefab";
        const string MarkerMaterialPath = MarkerRoot + "/Meeting Marker.mat";
        const string MarkerPrefabsListPath = MarkerRoot + "/MeetingMarkerNetworkPrefabsList.asset";
        const string PlayerPrefabPath = "Assets/VRMPAssets/Prefabs/PlayerPrefabs/XRMPT_XR_Origin_Setup.prefab";
        const string NetworkManagerPrefabPath = "Assets/VRMPAssets/Prefabs/Managers/Network Manager VR Multiplayer.prefab";

        static readonly string[] ScenePaths =
        {
            "Assets/Scenes/BasicScene.unity",
            "Assets/Scenes/SampleScene.unity",
        };

        [MenuItem("XR Multiplayer Meeting/Install Meeting Markers")]
        public static void InstallIntoCurrentScene()
        {
            var markerPrefab = EnsureMarkerPrefab();
            EnsureNetworkPrefabsList(markerPrefab);
            EnsurePlayerPrefabPlacer();
            EnsureSceneManager(markerPrefab);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("Meeting marker setup complete.");
        }

        public static void RunBatchSetup()
        {
            var markerPrefab = EnsureMarkerPrefab();
            EnsureNetworkPrefabsList(markerPrefab);
            EnsurePlayerPrefabPlacer();

            foreach (var scenePath in ScenePaths)
            {
                if (!File.Exists(scenePath))
                    continue;

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                EnsureSceneManager(markerPrefab);
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Meeting marker batch setup complete.");
        }

        static MeetingMarkerNetwork EnsureMarkerPrefab()
        {
            Directory.CreateDirectory(MarkerRoot);

            var existing = AssetDatabase.LoadAssetAtPath<MeetingMarkerNetwork>(MarkerPrefabPath);
            if (existing != null)
                return existing;

            var material = AssetDatabase.LoadAssetAtPath<Material>(MarkerMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
                {
                    color = new Color(0.1f, 0.62f, 1f, 1f)
                };
                AssetDatabase.CreateAsset(material, MarkerMaterialPath);
            }

            var root = new GameObject("Meeting Marker Networked");
            root.AddComponent<NetworkObject>();
            var markerNetwork = root.AddComponent<MeetingMarkerNetwork>();

            var pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pin.name = "Pin";
            pin.transform.SetParent(root.transform, false);
            pin.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            pin.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(pin.GetComponent<Collider>());

            var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(root.transform, false);
            stem.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            stem.transform.localScale = new Vector3(0.018f, 0.1f, 0.018f);
            stem.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(stem.GetComponent<Collider>());

            var label = new GameObject("Label");
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.17f, 0f);
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.text = "Meeting Point";
            text.fontSize = 0.18f;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(1.2f, 0.35f);
            label.AddComponent<Billboard>();

            var serializedMarker = new SerializedObject(markerNetwork);
            serializedMarker.FindProperty("m_ColorRenderers").arraySize = 2;
            serializedMarker.FindProperty("m_ColorRenderers").GetArrayElementAtIndex(0).objectReferenceValue = pin.GetComponent<Renderer>();
            serializedMarker.FindProperty("m_ColorRenderers").GetArrayElementAtIndex(1).objectReferenceValue = stem.GetComponent<Renderer>();
            serializedMarker.FindProperty("m_LabelText").objectReferenceValue = text;
            serializedMarker.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, MarkerPrefabPath);
            Object.DestroyImmediate(root);

            return prefab.GetComponent<MeetingMarkerNetwork>();
        }

        static void EnsureNetworkPrefabsList(MeetingMarkerNetwork markerPrefab)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(MarkerPrefabsListPath);
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(list, MarkerPrefabsListPath);
            }

            if (!list.Contains(markerPrefab.gameObject))
            {
                list.Add(new NetworkPrefab { Prefab = markerPrefab.gameObject });
                EditorUtility.SetDirty(list);
            }

            var networkManagers = new[]
            {
                AssetDatabase.LoadAssetAtPath<NetworkManager>(NetworkManagerPrefabPath)
            }.Where(manager => manager != null);

            foreach (var manager in networkManagers)
            {
                if (!manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Contains(list))
                {
                    manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
                    EditorUtility.SetDirty(manager);
                }
            }
        }

        static void EnsurePlayerPrefabPlacer()
        {
            if (!File.Exists(PlayerPrefabPath))
                return;

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            var rayInteractors = root.GetComponentsInChildren<XRRayInteractor>(true);
            var rightRay = rayInteractors.FirstOrDefault(ray => ray.name.ToLowerInvariant().Contains("right")) ?? rayInteractors.FirstOrDefault();

            if (rightRay != null && rightRay.GetComponent<MeetingMarkerPlacer>() == null)
                rightRay.gameObject.AddComponent<MeetingMarkerPlacer>();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void EnsureSceneManager(MeetingMarkerNetwork markerPrefab)
        {
            var manager = Object.FindFirstObjectByType<MeetingMarkerManager>();
            if (manager == null)
            {
                var gameObject = new GameObject("Meeting Marker Manager");
                gameObject.AddComponent<NetworkObject>();
                manager = gameObject.AddComponent<MeetingMarkerManager>();
            }

            var serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("m_MarkerPrefab").objectReferenceValue = markerPrefab;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
        }
    }
}
