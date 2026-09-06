using GemGrid.Audio;
using GemGrid.Bootstrap;
using GemGrid.Configuration;
using GemGrid.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GemGrid.EditorTools
{
    /// <summary>
    /// Builds the Boot and Gameplay scenes entirely through Unity's own
    /// scene/GameObject/component APIs — deliberately not hand-authored .unity YAML,
    /// since that could not be verified without a real Unity Editor (see README_M1.md /
    /// UNITY_SETUP.md for why). Requires the data assets from
    /// <see cref="GemGridAssetSetup"/> to already exist.
    ///
    /// Exposes idempotent <c>Ensure...</c> methods (skip + return early if the scene
    /// file already exists — never silently overwrites manual edits) alongside menu
    /// items for manual/explicit use. Used by <see cref="GemGridAutoSetup"/>.
    ///
    /// NOT run in this environment (no Unity Editor available) — see UNITY_SETUP.md.
    /// </summary>
    public static class GemGridSceneSetup
    {
        internal const string ScenesFolder = "Assets/_Project/Scenes";
        internal const string BootScenePath = ScenesFolder + "/Boot.unity";
        internal const string GameplayScenePath = ScenesFolder + "/Gameplay.unity";

        /// <summary>Creates Boot.unity if it doesn't already exist. Returns true if it was (or already is) present.</summary>
        internal static bool EnsureBootScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(BootScenePath) != null)
                return true;

            var blockShapeSet = GemGridAssetSetup.EnsureBlockShapeSet();
            var gameplayConfig = GemGridAssetSetup.EnsureGameplayConfig();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            cameraGo.tag = "MainCamera";

            var bootstrapGo = new GameObject("Bootstrap");
            var gameManagerBehaviour = bootstrapGo.AddComponent<GameManagerBehaviour>();

            var serializedGameManager = new SerializedObject(gameManagerBehaviour);
            serializedGameManager.FindProperty("blockShapeSet").objectReferenceValue = blockShapeSet;
            serializedGameManager.FindProperty("gameplayConfig").objectReferenceValue = gameplayConfig;
            serializedGameManager.ApplyModifiedPropertiesWithoutUndo();

            bootstrapGo.AddComponent<BootLoader>();

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, BootScenePath);
            Debug.Log($"[GemGrid] Created {BootScenePath}.");
            return true;
        }

        /// <summary>Creates Gameplay.unity if it doesn't already exist. Returns true if it was (or already is) present.</summary>
        internal static bool EnsureGameplayScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GameplayScenePath) != null)
                return true;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(3.5f, 3.5f, -10f);
            cameraGo.tag = "MainCamera";

            var rootGo = new GameObject("GameplayRoot");
            var gridController = rootGo.AddComponent<GridController>();
            var dragController = rootGo.AddComponent<BlockDragController>();
            rootGo.AddComponent<HapticHookListener>();
            rootGo.AddComponent<GameplayAnimationHooks>();
            rootGo.AddComponent<AudioHookListener>();
            rootGo.AddComponent<GameplayDebugHud>();

            var serializedDragController = new SerializedObject(dragController);
            serializedDragController.FindProperty("gridController").objectReferenceValue = gridController;
            serializedDragController.ApplyModifiedPropertiesWithoutUndo();

            var trayGo = new GameObject("BlockTray");
            trayGo.transform.SetParent(rootGo.transform, false);
            var trayView = trayGo.AddComponent<BlockTrayView>();
            var serializedTrayView = new SerializedObject(trayView);
            serializedTrayView.FindProperty("dragController").objectReferenceValue = dragController;
            serializedTrayView.ApplyModifiedPropertiesWithoutUndo();

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            Debug.Log($"[GemGrid] Created {GameplayScenePath}.");
            return true;
        }

        [MenuItem("GemGrid/Setup/3. Create Boot Scene")]
        public static void CreateBootScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(BootScenePath) != null)
            {
                Debug.LogWarning($"{BootScenePath} already exists — not overwriting. Delete it first if you want to regenerate.");
                return;
            }
            EnsureBootScene();
            Debug.Log("Add it to File ▸ Build Settings ▸ Scenes In Build (index 0) so it loads first — see UNITY_SETUP.md.");
        }

        [MenuItem("GemGrid/Setup/4. Create Gameplay Scene")]
        public static void CreateGameplayScene()
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GameplayScenePath) != null)
            {
                Debug.LogWarning($"{GameplayScenePath} already exists — not overwriting. Delete it first if you want to regenerate.");
                return;
            }
            EnsureGameplayScene();
            Debug.Log("Add it to File ▸ Build Settings ▸ Scenes In Build (after Boot) — see UNITY_SETUP.md.");
        }

        [MenuItem("GemGrid/Setup/5. Create Boot And Gameplay Scenes")]
        public static void CreateAllScenes()
        {
            CreateBootScene();
            CreateGameplayScene();
        }

        private static void EnsureScenesFolder()
        {
            if (AssetDatabase.IsValidFolder(ScenesFolder)) return;

            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
            {
                Debug.LogError("Expected folder 'Assets/_Project' to already exist in the project.");
                return;
            }

            AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        }
    }
}
