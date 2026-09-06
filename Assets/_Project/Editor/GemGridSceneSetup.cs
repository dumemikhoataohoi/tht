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
    /// Menu commands that build the Boot and Gameplay scenes entirely through Unity's
    /// own scene/GameObject/component APIs — deliberately not hand-authored .unity YAML,
    /// since that could not be verified without a real Unity Editor (see README_M1.md /
    /// UNITY_SETUP.md for why). Requires the data assets from
    /// <see cref="GemGridAssetSetup"/> to already exist.
    ///
    /// NOT run in this environment (no Unity Editor available) — see UNITY_SETUP.md.
    /// </summary>
    public static class GemGridSceneSetup
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string BootScenePath = ScenesFolder + "/Boot.unity";
        private const string GameplayScenePath = ScenesFolder + "/Gameplay.unity";
        private const string BlockShapeSetPath = "Assets/_Project/ScriptableObjects/BlockShapeSet.asset";
        private const string GameplayConfigPath = "Assets/_Project/ScriptableObjects/GameplayConfig.asset";

        [MenuItem("GemGrid/Setup/3. Create Boot Scene")]
        public static void CreateBootScene()
        {
            var blockShapeSet = AssetDatabase.LoadAssetAtPath<BlockShapeSet>(BlockShapeSetPath);
            var gameplayConfig = AssetDatabase.LoadAssetAtPath<GameplayConfig>(GameplayConfigPath);
            if (blockShapeSet == null || gameplayConfig == null)
            {
                Debug.LogError("Run 'GemGrid/Setup/0. Create All Required Data Assets' first — " +
                                "the Boot scene needs BlockShapeSet + GameplayConfig assets to exist.");
                return;
            }

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
            Debug.Log($"Created {BootScenePath}. Add it to File ▸ Build Settings ▸ Scenes In Build (index 0) " +
                       "so it loads first — see UNITY_SETUP.md.");
        }

        [MenuItem("GemGrid/Setup/4. Create Gameplay Scene")]
        public static void CreateGameplayScene()
        {
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

            var serializedDragController = new SerializedObject(dragController);
            serializedDragController.FindProperty("gridController").objectReferenceValue = gridController;
            serializedDragController.ApplyModifiedPropertiesWithoutUndo();

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            Debug.Log($"Created {GameplayScenePath}. GridController has no cellSprite assigned yet, so cells " +
                       "won't render visibly until you assign a placeholder square sprite in the Inspector — " +
                       "grid state changes are still testable via the Console/debugger. Add this scene to " +
                       "File ▸ Build Settings ▸ Scenes In Build (after Boot) — see UNITY_SETUP.md.");
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
