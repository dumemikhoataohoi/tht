using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GemGrid.EditorTools
{
    /// <summary>
    /// Runs automatically once Unity finishes compiling scripts, so the project is
    /// guaranteed to have working data assets/scenes even if the "GemGrid" top-level
    /// menu is somehow not visible/discoverable in a given Editor session — the
    /// gameplay-critical setup no longer depends on anyone finding or clicking a menu
    /// item. Idempotent: every step checks for existing assets/scenes/Build Settings
    /// entries first and never overwrites — safe to run on every domain reload.
    ///
    /// The <c>GemGrid ▸ Setup ▸ ...</c> menu items (see <see cref="GemGridAssetSetup"/>,
    /// <see cref="GemGridSceneSetup"/>) still exist for manual/explicit re-runs.
    ///
    /// NOT run in this environment (no Unity Editor available) — see UNITY_SETUP.md.
    /// </summary>
    [InitializeOnLoad]
    public static class GemGridAutoSetup
    {
        static GemGridAutoSetup()
        {
            // Defer past the current domain-reload callback — creating assets/scenes
            // directly inside a static constructor is unsafe timing in Unity.
            EditorApplication.delayCall += RunOnce;
        }

        private static void RunOnce()
        {
            EditorApplication.delayCall -= RunOnce;

            try
            {
                GemGridAssetSetup.EnsureBlockShapeSet();
                GemGridAssetSetup.EnsureGameplayConfig();
                GemGridSceneSetup.EnsureBootScene();
                GemGridSceneSetup.EnsureMainMenuScene();
                GemGridSceneSetup.EnsureGameplayScene();
                EnsureBuildSettingsScenes();
                EnsurePlayModeStartScene();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GemGrid] Automatic setup failed — you can still run it manually via " +
                                $"GemGrid ▸ Setup ▸ 0/6, or fix the error below and it will retry on next compile.\n{ex}");
            }
        }

        private static void EnsureBuildSettingsScenes()
        {
            string[] canonicalOrder =
            {
                GemGridSceneSetup.BootScenePath,
                GemGridSceneSetup.MainMenuScenePath,
                GemGridSceneSetup.GameplayScenePath,
            };

            var current = EditorBuildSettings.scenes;
            bool alreadyCorrect = current.Length >= canonicalOrder.Length
                                  && canonicalOrder.SequenceEqual(current.Take(canonicalOrder.Length).Select(s => s.path));
            if (alreadyCorrect) return;

            // Keep any other scenes the project already had, appended after the three
            // canonical ones, so this never silently discards unrelated Build Settings entries.
            var extras = current.Where(s => !canonicalOrder.Contains(s.path)).ToList();

            var newScenes = new List<EditorBuildSettingsScene>();
            foreach (var path in canonicalOrder)
                newScenes.Add(new EditorBuildSettingsScene(path, true));
            newScenes.AddRange(extras);

            EditorBuildSettings.scenes = newScenes.ToArray();
            Debug.Log("[GemGrid] Set Build Settings scene order: Boot, MainMenu, Gameplay.");
        }

        /// <summary>
        /// Root cause of "GameManagerBehaviour.Instance is null" when pressing Play
        /// with MainMenu.unity or Gameplay.unity open: by default, the Unity Editor
        /// Play button runs whichever scene is currently open in the Editor, NOT
        /// Boot.unity — so GameManagerBehaviour (and its DontDestroyOnLoad instance)
        /// never gets created at all. Setting <see cref="EditorSceneManager.playModeStartScene"/>
        /// forces every Play press to always start from Boot first, regardless of
        /// which scene tab is open, which is exactly what the Boot ▸ MainMenu ▸
        /// Gameplay flow requires. This is an Editor-only workflow setting (stored in
        /// local, gitignored Library/UserSettings state) — reapplied here on every
        /// compile so it doesn't depend on anyone configuring it by hand or on it
        /// surviving a fresh clone.
        /// </summary>
        private static void EnsurePlayModeStartScene()
        {
            var bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GemGridSceneSetup.BootScenePath);
            if (bootScene == null) return; // EnsureBootScene() above failed silently; nothing to point to yet.

            if (EditorSceneManager.playModeStartScene != bootScene)
            {
                EditorSceneManager.playModeStartScene = bootScene;
                Debug.Log("[GemGrid] Set Play Mode Start Scene to Boot.unity — pressing Play now always " +
                           "starts from Boot regardless of which scene tab is open in the Editor.");
            }
        }
    }
}
