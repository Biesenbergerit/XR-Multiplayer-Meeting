using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRMultiplayer
{
    /// <summary>
    /// Local-only manager for a movable passthrough window inside the VR multiplayer room.
    /// </summary>
    public class RealityWindowManager : MonoBehaviour
    {
        public static RealityWindowManager Instance { get; private set; }

        [SerializeField] RealityWindow m_WindowPrefab;
        [SerializeField] float m_SpawnDistance = 1.2f;
        [SerializeField] Vector3 m_SpawnViewportPosition = new(0.5f, 0.52f, 0f);
        [SerializeField] bool m_ShowOnStart = false;
        [SerializeField] bool m_HideRegularMenusWhileVisible = true;

        RealityWindow m_WindowInstance;
        RealityWindowMenuFollower m_MenuFollower;
        readonly List<CanvasState> m_HiddenCanvasStates = new();
        Camera m_MainCamera;
        Behaviour m_ARSession;
        Behaviour m_ARCameraManager;
        Behaviour m_ARCameraBackground;
        CameraClearFlags m_OriginalClearFlags;
        Color m_OriginalBackgroundColor;
        bool m_CameraSettingsCaptured;

        public bool isWindowVisible => m_WindowInstance != null && m_WindowInstance.gameObject.activeSelf;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            FindMainCamera();
            FindMenuFollower();

            if (m_ShowOnStart)
                ShowWindow();
            else
            {
                ConfigurePassthrough(false);
                SetLauncherVisible(true);
                RestoreRegularMenus();
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            RestoreCameraSettings();
            RestoreRegularMenus();
        }

        public void ToggleWindow()
        {
            if (isWindowVisible)
                HideWindow();
            else
                ShowWindow();
        }

        public void ShowWindow()
        {
            FindMainCamera();
            if (m_MainCamera == null)
            {
                Debug.LogWarning("Reality Window could not find a scene camera.", this);
                return;
            }

            EnsureWindow();

            if (m_WindowInstance == null)
                return;

            ConfigurePassthrough(true);
            SetLauncherVisible(true);
            HideRegularMenus();
            m_WindowInstance.gameObject.SetActive(true);
            m_WindowInstance.PlaceInFrontOfCamera(m_MainCamera, m_SpawnDistance, m_SpawnViewportPosition);
        }

        public void HideWindow()
        {
            if (m_WindowInstance != null)
                m_WindowInstance.gameObject.SetActive(false);

            ConfigurePassthrough(false);
            SetLauncherVisible(true);
            RestoreRegularMenus();
        }

        public void ResetWindowPose()
        {
            if (m_WindowInstance == null)
                return;

            FindMainCamera();
            m_WindowInstance.PlaceInFrontOfCamera(m_MainCamera, m_SpawnDistance, m_SpawnViewportPosition);
        }

        void EnsureWindow()
        {
            if (m_WindowInstance != null)
                return;

            if (m_WindowPrefab == null)
            {
                Debug.LogWarning("Reality Window prefab is missing on the RealityWindowManager.", this);
                return;
            }

            m_WindowInstance = Instantiate(m_WindowPrefab);
            m_WindowInstance.SetManager(this);
        }

        void FindMainCamera()
        {
            if (m_MainCamera != null)
                return;

            m_MainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        }

        void FindMenuFollower()
        {
            if (m_MenuFollower != null)
                return;

            m_MenuFollower = FindAnyObjectByType<RealityWindowMenuFollower>(FindObjectsInactive.Include);
        }

        void SetLauncherVisible(bool visible)
        {
            FindMenuFollower();

            if (m_MenuFollower != null)
                m_MenuFollower.gameObject.SetActive(visible);
        }

        void HideRegularMenus()
        {
            if (!m_HideRegularMenusWhileVisible)
                return;

            RestoreRegularMenus();

            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas == null || IsRealityWindowObject(canvas.transform))
                    continue;

                m_HiddenCanvasStates.Add(new CanvasState(canvas, canvas.enabled));
                canvas.enabled = false;
            }
        }

        void RestoreRegularMenus()
        {
            for (var index = 0; index < m_HiddenCanvasStates.Count; index++)
            {
                var state = m_HiddenCanvasStates[index];
                if (state.Canvas != null)
                    state.Canvas.enabled = state.WasEnabled;
            }

            m_HiddenCanvasStates.Clear();
        }

        bool IsRealityWindowObject(Transform target)
        {
            if (target == null)
                return false;

            return target.GetComponentInParent<RealityWindow>(true) != null
                || target.GetComponentInParent<RealityWindowMenuFollower>(true) != null;
        }

        void ConfigurePassthrough(bool enabled)
        {
            if (m_MainCamera == null)
                return;

            if (!m_CameraSettingsCaptured)
            {
                m_OriginalClearFlags = m_MainCamera.clearFlags;
                m_OriginalBackgroundColor = m_MainCamera.backgroundColor;
                m_CameraSettingsCaptured = true;
            }

            if (m_ARCameraManager == null)
                m_ARCameraManager = GetOptionalComponent(m_MainCamera.gameObject, "UnityEngine.XR.ARFoundation.ARCameraManager");

            if (m_ARCameraBackground == null)
                m_ARCameraBackground = GetOptionalComponent(m_MainCamera.gameObject, "UnityEngine.XR.ARFoundation.ARCameraBackground");

            EnsureARSession();
            SetOptionalBehaviourEnabled(m_ARSession, enabled);
            SetOptionalBehaviourEnabled(m_ARCameraManager, enabled);
            SetOptionalBehaviourEnabled(m_ARCameraBackground, enabled);

            if (enabled)
            {
                m_MainCamera.clearFlags = CameraClearFlags.SolidColor;
                var color = m_MainCamera.backgroundColor;
                color.a = 0f;
                m_MainCamera.backgroundColor = color;
            }
            else
            {
                RestoreCameraSettings();
            }
        }

        void EnsureARSession()
        {
            if (m_ARSession != null)
                return;

            m_ARSession = FindOptionalBehaviour("UnityEngine.XR.ARFoundation.ARSession");
            if (m_ARSession != null)
                return;

            var sessionType = FindType("UnityEngine.XR.ARFoundation.ARSession");
            if (sessionType == null)
                return;

            var sessionObject = new GameObject("Reality Window AR Session");
            sessionObject.transform.SetParent(transform, false);
            m_ARSession = sessionObject.AddComponent(sessionType) as Behaviour;
        }

        static Behaviour GetOptionalComponent(GameObject target, string typeName)
        {
            var existing = FindOptionalBehaviour(target, typeName);
            if (existing != null)
                return existing;

            var type = FindType(typeName);
            if (type == null)
                return null;

            return target.AddComponent(type) as Behaviour;
        }

        static Behaviour FindOptionalBehaviour(string typeName)
        {
            foreach (var behaviour in FindObjectsByType<Behaviour>(FindObjectsInactive.Include))
            {
                if (behaviour != null && behaviour.GetType().FullName == typeName)
                    return behaviour;
            }

            return null;
        }

        static Behaviour FindOptionalBehaviour(GameObject target, string typeName)
        {
            foreach (var behaviour in target.GetComponents<Behaviour>())
            {
                if (behaviour != null && behaviour.GetType().FullName == typeName)
                    return behaviour;
            }

            return null;
        }

        static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        static void SetOptionalBehaviourEnabled(Behaviour behaviour, bool enabled)
        {
            if (behaviour != null)
                behaviour.enabled = enabled;
        }

        void RestoreCameraSettings()
        {
            if (!m_CameraSettingsCaptured || m_MainCamera == null)
                return;

            m_MainCamera.clearFlags = m_OriginalClearFlags;
            m_MainCamera.backgroundColor = m_OriginalBackgroundColor;
        }

        readonly struct CanvasState
        {
            public CanvasState(Canvas canvas, bool wasEnabled)
            {
                Canvas = canvas;
                WasEnabled = wasEnabled;
            }

            public Canvas Canvas { get; }
            public bool WasEnabled { get; }
        }
    }
}
